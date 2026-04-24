using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IAuditContext _auditContext;

        // Tables we deliberately skip auditing —
        // AuditLog itself (infinite loop prevention) and
        // RefreshTokens (too noisy, no business value)
        private static readonly HashSet<string> SkippedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(AuditLog),
        nameof(RefreshToken)
    };

        // Properties that must never appear in audit logs
        private static readonly HashSet<string> RedactedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash",
        "GoogleId"
    };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AuditInterceptor(IAuditContext auditContext)
        {
            _auditContext = auditContext;
        }

        // ── Sync path ──────────────────────────────────────────────────────────────

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        // ── Async path (used by all handlers) ─────────────────────────────────────

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // ── Core logic ─────────────────────────────────────────────────────────────

        private void AddAuditLogs(DbContext? context)
        {
            if (context is null) return;

            var userId = _auditContext.CurrentUserId;
            var ipAddress = _auditContext.CurrentIpAddress ?? "system";

            // Detect all pending changes before the save happens
            var entries = context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is not AuditLog              // never audit the audit table
                         && e.Entity is not RefreshToken          // skip noisy token churn
                         && e.State is EntityState.Added
                                  or EntityState.Modified
                                  or EntityState.Deleted)
                .ToList();

            if (entries.Count == 0) return;

            var auditLogs = entries
                .Select(entry => BuildAuditLog(entry, userId, ipAddress))
                .Where(log => log is not null)
                .Cast<AuditLog>()
                .ToList();

            if (auditLogs.Count > 0)
                context.Set<AuditLog>().AddRange(auditLogs);
        }

        private AuditLog? BuildAuditLog(
            EntityEntry entry,
            int? userId,
            string ipAddress)
        {
            var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name;

            // Skip any table in the exclusion list
            if (SkippedTables.Contains(tableName))
                return null;

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => (AuditAction?)null
            };

            if (action is null) return null;

            var recordId = GetPrimaryKey(entry);
            var oldValues = action == AuditAction.Create
                ? null
                : SerializeValues(entry.OriginalValues, entry);

            var newValues = action == AuditAction.Delete
                ? null
                : SerializeValues(entry.CurrentValues, entry);

            // For updates, skip if nothing meaningful actually changed
            if (action == AuditAction.Update && oldValues == newValues)
                return null;

            return new AuditLog(
                tableName: tableName,
                recordId: recordId,
                action: action.Value,
                performedByUserId: userId,
                oldValues: oldValues,
                newValues: newValues,
                ipAddress: ipAddress
            );
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        private static string GetPrimaryKey(EntityEntry entry)
        {
            var keyValues = entry.Metadata
                .FindPrimaryKey()
                ?.Properties
                .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "null");

            return keyValues is not null
                ? string.Join(",", keyValues)
                : "unknown";
        }

        private static string? SerializeValues(PropertyValues values, EntityEntry entry)
        {
            if (values is null) return null;

            var dict = new Dictionary<string, object?>();

            foreach (var property in values.Properties)
            {
                // Never log sensitive fields
                if (RedactedProperties.Contains(property.Name))
                    continue;

                // For updates: only log properties that actually changed
                if (entry.State == EntityState.Modified)
                {
                    var prop = entry.Property(property.Name);
                    if (!prop.IsModified) continue;
                }

                var value = values[property];

                // Convert non-serializable types to strings
                dict[property.Name] = value switch
                {
                    Guid g => g.ToString(),
                    DateTime dt => dt.ToString("O"),   // ISO 8601
                    DateOnly d => d.ToString("yyyy-MM-dd"),
                    byte[] _ => "[binary]",
                    _ => value
                };
            }

            return dict.Count == 0
                ? null
                : JsonSerializer.Serialize(dict, JsonOptions);
        }
    }
}
