using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using Al_Nawras.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Persistence.Seeders
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context, ILogger logger)
        {
            // Reporting templates should be seeded independently, even for existing databases.
            await ReportTemplateSeeder.SeedAsync(context, logger);

            // Guard — skip the legacy sample data if business data already exists
            if (await context.Clients.AnyAsync())
            {
                logger.LogInformation("DataSeeder: data already exists, skipping.");
                return;
            }

            logger.LogInformation("DataSeeder: starting seed...");

            await SeedCurrencyRatesAsync(context, logger);
            await SeedUsersAsync(context, logger);
            await SeedClientsAsync(context, logger);
            await SeedDealsAndRelatedAsync(context, logger);
            await SeedNotificationsAsync(context, logger);

            logger.LogInformation("DataSeeder: completed successfully.");
        }

        // ══════════════════════════════════════════════════════════════════
        // 1. CURRENCY RATES
        // ══════════════════════════════════════════════════════════════════

        private static async Task SeedCurrencyRatesAsync(AppDbContext context, ILogger logger)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var rates = new List<CurrencyRate>
        {
            new("EUR", "USD", 1.0842m, today.AddDays(-1), "Manual"),
            new("GBP", "USD", 1.2671m, today.AddDays(-1), "Manual"),
            new("AED", "USD", 0.2723m, today.AddDays(-1), "Manual"),
            new("SAR", "USD", 0.2666m, today.AddDays(-1), "Manual"),
            new("CNY", "USD", 0.1381m, today.AddDays(-1), "Manual"),
            new("JPY", "USD", 0.0067m, today.AddDays(-1), "Manual"),
            new("EGP", "USD", 0.0204m, today.AddDays(-1), "Manual"),

            // Historical rates for older payments
            new("EUR", "USD", 1.0780m, today.AddMonths(-3), "Manual"),
            new("EUR", "USD", 1.0650m, today.AddMonths(-6), "Manual"),
            new("GBP", "USD", 1.2510m, today.AddMonths(-3), "Manual"),
            new("AED", "USD", 0.2723m, today.AddMonths(-3), "Manual"),
        };

            await context.CurrencyRates.AddRangeAsync(rates);
            await context.SaveChangesAsync();
            logger.LogInformation("DataSeeder: seeded {Count} currency rates.", rates.Count);
        }

        // ══════════════════════════════════════════════════════════════════
        // 2. USERS (one per role)
        // ══════════════════════════════════════════════════════════════════

        private static async Task SeedUsersAsync(AppDbContext context, ILogger logger)
        {
            // Admin already seeded in migration — just verify
            if (await context.Users.AnyAsync(u => u.RoleId == 1))
            {
                logger.LogInformation("DataSeeder: admin user already present, skipping users.");
                return;
            }

            var hash = new AuthService();

            var users = new List<User>
        {
            // Sales team
            new("sara.ahmed@alnawras.com",
                hash.HashPassword("Sales@123"),
                "Sara", "Ahmed", roleId: 2),

            new("omar.hassan@alnawras.com",
                hash.HashPassword("Sales@123"),
                "Omar", "Hassan", roleId: 2),

            // Operations team
            new("karim.nour@alnawras.com",
                hash.HashPassword("Ops@123"),
                "Karim", "Nour", roleId: 3),

            // Accounts team
            new("layla.mansour@alnawras.com",
                hash.HashPassword("Accounts@123"),
                "Layla", "Mansour", roleId: 4),
        };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
            logger.LogInformation("DataSeeder: seeded {Count} internal users.", users.Count);
        }

        // ══════════════════════════════════════════════════════════════════
        // 3. CLIENTS
        // ══════════════════════════════════════════════════════════════════

        private static async Task SeedClientsAsync(AppDbContext context, ILogger logger)
        {
            var salesUsers = await context.Users
                .Where(u => u.RoleId == 2)
                .ToListAsync();

            var sara = salesUsers.FirstOrDefault(u => u.Email.StartsWith("sara"));
            var omar = salesUsers.FirstOrDefault(u => u.Email.StartsWith("omar"));

            var clients = new List<Client>
        {
            new("Gulf Steel Trading LLC",
                "procurement@gulfsteel.ae",
                "+971 4 123 4567",
                "UAE",
                "Gulf Steel Trading LLC",
                sara?.Id),

            new("Nile Textiles Co.",
                "imports@niletextiles.eg",
                "+20 2 2345 6789",
                "Egypt",
                "Nile Textiles Co.",
                sara?.Id),

            new("Riyadh Industrial Group",
                "supply@riyadhindustrial.sa",
                "+966 11 234 5678",
                "Saudi Arabia",
                "Riyadh Industrial Group",
                omar?.Id),

            new("Beijing Export Partners",
                "exports@beijingpartners.cn",
                "+86 10 1234 5678",
                "China",
                "Beijing Export Partners",
                omar?.Id),

            new("London Commodities Ltd",
                "trading@londoncommodities.co.uk",
                "+44 20 7123 4567",
                "United Kingdom",
                "London Commodities Ltd",
                sara?.Id),

            new("Cairo Trading House",
                "info@cairotrading.eg",
                "+20 2 3456 7890",
                "Egypt",
                "Cairo Trading House",
                omar?.Id),
        };

            await context.Clients.AddRangeAsync(clients);
            await context.SaveChangesAsync();
            logger.LogInformation("DataSeeder: seeded {Count} clients.", clients.Count);
        }

        // ══════════════════════════════════════════════════════════════════
        // 4. DEALS + STATUS HISTORY + SHIPMENTS + PAYMENTS + DOCUMENTS + TASKS
        // ══════════════════════════════════════════════════════════════════

        private static async Task SeedDealsAndRelatedAsync(AppDbContext context, ILogger logger)
        {
            var clients = await context.Clients.ToListAsync();
            var users = await context.Users.ToListAsync();

            var admin = users.FirstOrDefault(u => u.RoleId == 1);
            var sara = users.FirstOrDefault(u => u.Email.StartsWith("sara"));
            var omar = users.FirstOrDefault(u => u.Email.StartsWith("omar"));
            var karim = users.FirstOrDefault(u => u.RoleId == 3);
            var layla = users.FirstOrDefault(u => u.RoleId == 4);

            var gulfSteel = clients.First(c => c.CompanyName.Contains("Gulf Steel"));
            var nileTex = clients.First(c => c.CompanyName.Contains("Nile Textiles"));
            var riyadhGroup = clients.First(c => c.CompanyName.Contains("Riyadh"));
            var beijingPartners = clients.First(c => c.CompanyName.Contains("Beijing"));
            var londonComm = clients.First(c => c.CompanyName.Contains("London"));
            var cairoHouse = clients.First(c => c.CompanyName.Contains("Cairo"));

            var adminId = admin?.Id ?? 1;
            var saraId = sara?.Id ?? 2;
            var omarId = omar?.Id ?? 3;
            var karimId = karim?.Id ?? 4;
            var laylaId = layla?.Id ?? 5;

            var now = DateTime.UtcNow;

            // ── Closed deal — full lifecycle, 6 months ago ─────────────────────────
            var deal1 = CreateDeal(gulfSteel.Id, "Steel Coils — Grade 304",
                125000m, "USD", saraId, "China", "UAE",
                createdAt: now.AddMonths(-6));

            AdvanceDeal(deal1, DealStatus.Closed, adminId, now.AddMonths(-6));
            deal1.SetCreatedAt(now.AddMonths(-6));

            await context.Deals.AddAsync(deal1);
            await context.SaveChangesAsync();

            AddShipment(context, deal1.Id,
                "COSCO Shipping", "Shanghai Port", "Jebel Ali Port",
                etd: now.AddMonths(-5).AddDays(-10),
                eta: now.AddMonths(-5),
                status: ShipmentStatus.Delivered,
                tracking: "COSCO-2024-001-SH");

            AddPayment(context, deal1.Id, 50000m, "USD", 1m, PaymentType.Advance,
                dueDate: now.AddMonths(-5).AddDays(-20),
                status: PaymentStatus.FullyPaid,
                paidAt: now.AddMonths(-5).AddDays(-18));

            AddPayment(context, deal1.Id, 75000m, "USD", 1m, PaymentType.OnDelivery,
                dueDate: now.AddMonths(-5),
                status: PaymentStatus.FullyPaid,
                paidAt: now.AddMonths(-4).AddDays(-28));

            AddDocument(context, deal1.Id, DocumentType.Invoice,
                "invoice-SH-2024-001.pdf", adminId);
            AddDocument(context, deal1.Id, DocumentType.BillOfLading,
                "bl-COSCO-2024-001.pdf", karimId);
            AddDocument(context, deal1.Id, DocumentType.CertificateOfOrigin,
                "coo-CN-2024-001.pdf", karimId);
            AddDocument(context, deal1.Id, DocumentType.PackingList,
                "packing-2024-001.pdf", karimId);

            // ── Closed deal — 4 months ago ─────────────────────────────────────────
            var deal2 = CreateDeal(nileTex.Id, "Cotton Fabric — 200gsm",
                87500m, "USD", saraId, "India", "Egypt",
                createdAt: now.AddMonths(-4));

            AdvanceDeal(deal2, DealStatus.Closed, saraId, now.AddMonths(-4));
            await context.Deals.AddAsync(deal2);
            await context.SaveChangesAsync();

            AddShipment(context, deal2.Id,
                "MSC Shipping", "Mumbai Port", "Alexandria Port",
                etd: now.AddMonths(-3).AddDays(-15),
                eta: now.AddMonths(-3),
                status: ShipmentStatus.Delivered,
                tracking: "MSC-2024-045-MU");

            AddPayment(context, deal2.Id, 87500m, "USD", 1m, PaymentType.Advance,
                dueDate: now.AddMonths(-3).AddDays(-10),
                status: PaymentStatus.FullyPaid,
                paidAt: now.AddMonths(-3).AddDays(-9));

            AddDocument(context, deal2.Id, DocumentType.Invoice, "invoice-IN-2024-045.pdf", saraId);
            AddDocument(context, deal2.Id, DocumentType.PackingList, "packing-IN-2024-045.pdf", karimId);
            AddDocument(context, deal2.Id, DocumentType.BillOfLading, "bl-MSC-2024-045.pdf", karimId);

            // ── Delivered deal ─────────────────────────────────────────────────────
            var deal3 = CreateDeal(riyadhGroup.Id, "Industrial Machinery — Hydraulic Press",
                320000m, "SAR", omarId, "Germany", "Saudi Arabia",
                createdAt: now.AddMonths(-3));

            AdvanceDeal(deal3, DealStatus.Delivered, omarId, now.AddMonths(-3));
            await context.Deals.AddAsync(deal3);
            await context.SaveChangesAsync();

            AddShipment(context, deal3.Id,
                "Hapag-Lloyd", "Hamburg Port", "Dammam Port",
                etd: now.AddMonths(-2).AddDays(-5),
                eta: now.AddMonths(-1).AddDays(-20),
                status: ShipmentStatus.Delivered,
                tracking: "HL-2024-312-HH");

            var sarToUsd = 0.2666m;
            AddPayment(context, deal3.Id, 128000m, "SAR", sarToUsd, PaymentType.Advance,
                dueDate: now.AddMonths(-2).AddDays(-20),
                status: PaymentStatus.FullyPaid,
                paidAt: now.AddMonths(-2).AddDays(-18));

            AddPayment(context, deal3.Id, 192000m, "SAR", sarToUsd, PaymentType.OnDelivery,
                dueDate: now.AddDays(-15),
                status: PaymentStatus.Pending);  // still pending after delivery

            AddDocument(context, deal3.Id, DocumentType.Invoice, "invoice-DE-2024-312.pdf", omarId);
            AddDocument(context, deal3.Id, DocumentType.BillOfLading, "bl-HL-2024-312.pdf", karimId);

            AddTask(context, deal3.Id, laylaId,
                "Follow up on remaining payment",
                "SAR 192,000 payment due — contact Riyadh Industrial Group.",
                TaskPriority.High,
                dueDate: now.AddDays(5));

            // ── Customs deal ───────────────────────────────────────────────────────
            var deal4 = CreateDeal(beijingPartners.Id, "Electronic Components — PCB Assemblies",
                215000m, "CNY", omarId, "China", "Egypt",
                createdAt: now.AddMonths(-2));

            AdvanceDeal(deal4, DealStatus.Customs, omarId, now.AddMonths(-2));
            await context.Deals.AddAsync(deal4);
            await context.SaveChangesAsync();

            AddShipment(context, deal4.Id,
                "Evergreen Marine", "Tianjin Port", "Alexandria Port",
                etd: now.AddMonths(-1).AddDays(-20),
                eta: now.AddDays(-3),
                status: ShipmentStatus.AtCustoms,
                tracking: "EVG-2024-789-TJ");

            var cnyToUsd = 0.1381m;
            AddPayment(context, deal4.Id, 107500m, "CNY", cnyToUsd, PaymentType.Advance,
                dueDate: now.AddMonths(-1).AddDays(-15),
                status: PaymentStatus.FullyPaid,
                paidAt: now.AddMonths(-1).AddDays(-14));

            AddPayment(context, deal4.Id, 107500m, "CNY", cnyToUsd, PaymentType.OnDelivery,
                dueDate: now.AddDays(10),
                status: PaymentStatus.Pending);

            AddDocument(context, deal4.Id, DocumentType.CertificateOfOrigin, "coo-CN-2024-789.pdf", karimId);
            AddDocument(context, deal4.Id, DocumentType.PackingList, "packing-CN-2024-789.pdf", karimId);

            AddTask(context, deal4.Id, karimId,
                "Clear customs — Alexandria",
                "Shipment EVG-2024-789-TJ is at Alexandria customs. Submit HS codes and certificates.",
                TaskPriority.Urgent,
                dueDate: now.AddDays(2));

            // ── Shipping deal ──────────────────────────────────────────────────────
            var deal5 = CreateDeal(londonComm.Id, "Crude Palm Oil — 500 MT",
                425000m, "GBP", saraId, "Malaysia", "United Kingdom",
                createdAt: now.AddMonths(-2).AddDays(-10));

            AdvanceDeal(deal5, DealStatus.Shipping, saraId, now.AddMonths(-2).AddDays(-10));
            await context.Deals.AddAsync(deal5);
            await context.SaveChangesAsync();

            var gbpToUsd = 1.2671m;
            AddShipment(context, deal5.Id,
                "Maersk Line", "Port Klang", "Felixstowe Port",
                etd: now.AddDays(-18),
                eta: now.AddDays(14),
                status: ShipmentStatus.InTransit,
                tracking: "MSK-2024-556-KL");

            AddPayment(context, deal5.Id, 212500m, "GBP", gbpToUsd, PaymentType.Advance,
                dueDate: now.AddMonths(-1),
                status: PaymentStatus.FullyPaid,
                paidAt: now.AddMonths(-1).AddDays(1));

            AddPayment(context, deal5.Id, 212500m, "GBP", gbpToUsd, PaymentType.OnDelivery,
                dueDate: now.AddDays(20),
                status: PaymentStatus.Pending);

            AddDocument(context, deal5.Id, DocumentType.Invoice, "invoice-MY-2024-556.pdf", saraId);
            AddDocument(context, deal5.Id, DocumentType.BillOfLading, "bl-MSK-2024-556.pdf", karimId);

            // ── Confirmed deal ─────────────────────────────────────────────────────
            var deal6 = CreateDeal(cairoHouse.Id, "Polypropylene Granules — 200 MT",
                98000m, "USD", omarId, "Saudi Arabia", "Egypt",
                createdAt: now.AddDays(-25));

            AdvanceDeal(deal6, DealStatus.Confirmed, omarId, now.AddDays(-25));
            await context.Deals.AddAsync(deal6);
            await context.SaveChangesAsync();

            AddPayment(context, deal6.Id, 29400m, "USD", 1m, PaymentType.Advance,
                dueDate: now.AddDays(-5),
                status: PaymentStatus.FullyPaid,
                paidAt: now.AddDays(-4));

            AddPayment(context, deal6.Id, 68600m, "USD", 1m, PaymentType.OnShipment,
                dueDate: now.AddDays(30),
                status: PaymentStatus.Pending);

            AddTask(context, deal6.Id, karimId,
                "Book freight — Dammam to Alexandria",
                "Confirmed deal ready for shipment booking. Contact Hapag-Lloyd for next available vessel.",
                TaskPriority.High,
                dueDate: now.AddDays(3));

            // ── Negotiation deal ───────────────────────────────────────────────────
            var deal7 = CreateDeal(gulfSteel.Id, "Stainless Steel Pipes — Grade 316L",
                185000m, "USD", saraId, "South Korea", "UAE",
                createdAt: now.AddDays(-12));

            AdvanceDeal(deal7, DealStatus.Negotiation, saraId, now.AddDays(-12));
            await context.Deals.AddAsync(deal7);
            await context.SaveChangesAsync();

            AddTask(context, deal7.Id, saraId,
                "Send revised price quote",
                "Gulf Steel requested 5% discount on 316L pipes. Prepare revised quotation.",
                TaskPriority.Medium,
                dueDate: now.AddDays(2));

            // ── Lead deal 1 ────────────────────────────────────────────────────────
            var deal8 = CreateDeal(nileTex.Id, "Polyester Yarn — 1000 Tonnes",
                340000m, "USD", saraId, "Vietnam", "Egypt",
                createdAt: now.AddDays(-5));

            // Lead — stays at initial status (no advance needed)
            await context.Deals.AddAsync(deal8);
            await context.SaveChangesAsync();

            AddTask(context, deal8.Id, saraId,
                "Qualify lead — verify credit history",
                "New lead from Nile Textiles for large polyester yarn order. Run credit check.",
                TaskPriority.Medium,
                dueDate: now.AddDays(7));

            // ── Lead deal 2 ────────────────────────────────────────────────────────
            var deal9 = CreateDeal(riyadhGroup.Id, "Industrial Pumps — Centrifugal Type",
                76000m, "USD", omarId, "Germany", "Saudi Arabia",
                createdAt: now.AddDays(-3));

            await context.Deals.AddAsync(deal9);
            await context.SaveChangesAsync();

            // ── Overdue payment deal — for dashboard drama ─────────────────────────
            var deal10 = CreateDeal(cairoHouse.Id, "Aluminum Ingots — 6061 Alloy",
                145000m, "USD", omarId, "UAE", "Egypt",
                createdAt: now.AddMonths(-3).AddDays(-5));

            AdvanceDeal(deal10, DealStatus.Delivered, omarId, now.AddMonths(-3).AddDays(-5));
            await context.Deals.AddAsync(deal10);
            await context.SaveChangesAsync();

            AddShipment(context, deal10.Id,
                "Emirates Shipping", "Jebel Ali Port", "Alexandria Port",
                etd: now.AddMonths(-2).AddDays(-10),
                eta: now.AddMonths(-2),
                status: ShipmentStatus.Delivered,
                tracking: "ES-2024-101-JA");

            AddPayment(context, deal10.Id, 58000m, "USD", 1m, PaymentType.Advance,
                dueDate: now.AddMonths(-2).AddDays(-15),
                status: PaymentStatus.FullyPaid,
                paidAt: now.AddMonths(-2).AddDays(-14));

            // Deliberately overdue — DueDate in the past, status Overdue
            AddPayment(context, deal10.Id, 87000m, "USD", 1m, PaymentType.OnDelivery,
                dueDate: now.AddDays(-45),
                status: PaymentStatus.Overdue);   // already marked by background job

            AddDocument(context, deal10.Id, DocumentType.Invoice, "invoice-AE-2024-101.pdf", omarId);
            AddDocument(context, deal10.Id, DocumentType.BillOfLading, "bl-ES-2024-101.pdf", karimId);

            AddTask(context, deal10.Id, laylaId,
                "Chase overdue payment — Cairo Trading House",
                "USD 87,000 overdue for 45 days. Escalate to management if no response within 48 hours.",
                TaskPriority.Urgent,
                dueDate: now.AddDays(1));

            // ── EUR deal — for multi-currency reporting ─────────────────────────────
            var deal11 = CreateDeal(londonComm.Id, "Pharmaceutical Raw Materials",
                290000m, "EUR", saraId, "Germany", "United Kingdom",
                createdAt: now.AddMonths(-1).AddDays(-10));

            AdvanceDeal(deal11, DealStatus.Shipping, saraId, now.AddMonths(-1).AddDays(-10));
            await context.Deals.AddAsync(deal11);
            await context.SaveChangesAsync();

            var eurToUsd = 1.0842m;
            AddShipment(context, deal11.Id,
                "DB Schenker", "Rotterdam Port", "Tilbury Port",
                etd: now.AddDays(-8),
                eta: now.AddDays(5),
                status: ShipmentStatus.InTransit,
                tracking: "DBS-2024-221-RT");

            AddPayment(context, deal11.Id, 145000m, "EUR", eurToUsd, PaymentType.Advance,
                dueDate: now.AddMonths(-1),
                status: PaymentStatus.FullyPaid,
                paidAt: now.AddDays(-30));

            AddPayment(context, deal11.Id, 145000m, "EUR", eurToUsd, PaymentType.OnDelivery,
                dueDate: now.AddDays(12),
                status: PaymentStatus.Pending);

            await context.SaveChangesAsync();
            logger.LogInformation("DataSeeder: seeded 11 deals with full related data.");
        }

        // ══════════════════════════════════════════════════════════════════
        // 5. NOTIFICATIONS
        // ══════════════════════════════════════════════════════════════════

        private static async Task SeedNotificationsAsync(AppDbContext context, ILogger logger)
        {
            var users = await context.Users.ToListAsync();
            var layla = users.FirstOrDefault(u => u.RoleId == 4);
            var karim = users.FirstOrDefault(u => u.RoleId == 3);
            var sara = users.FirstOrDefault(u => u.Email.StartsWith("sara"));

            if (layla is null || karim is null || sara is null) return;

            var notifications = new List<Notification>
        {
            new(layla.Id, NotificationType.PaymentOverdue,
                "Overdue payment — Cairo Trading House",
                "USD 87,000 on deal AL-2024-101 is 45 days overdue.",
                relatedEntityType: "Deal"),

            new(layla.Id, NotificationType.PaymentOverdue,
                "Follow up required — Riyadh Industrial Group",
                "SAR 192,000 payment pending after delivery confirmation.",
                relatedEntityType: "Deal"),

            new(karim.Id, NotificationType.ShipmentDelayed,
                "Customs clearance pending — Alexandria",
                "Shipment EVG-2024-789-TJ has been at customs for 3 days.",
                relatedEntityType: "Shipment"),

            new(sara.Id, NotificationType.DealStatusChanged,
                "Deal confirmed — Cairo Trading House",
                "Polypropylene Granules deal has been confirmed and is ready for shipment booking.",
                relatedEntityType: "Deal"),

            new(sara.Id, NotificationType.TaskAssigned,
                "Task assigned — Nile Textiles lead",
                "New task: Qualify lead — verify credit history for Polyester Yarn order.",
                relatedEntityType: "Deal"),
        };

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
            logger.LogInformation("DataSeeder: seeded {Count} notifications.", notifications.Count);
        }

        // ══════════════════════════════════════════════════════════════════
        // PRIVATE FACTORY HELPERS
        // ══════════════════════════════════════════════════════════════════

        private static Deal CreateDeal(
            Guid clientId, string commodity,
            decimal totalValue, string currency,
            int assignedSalesUserId,
            string origin, string destination,
            DateTime createdAt,
            string? notes = null)
        {
            var deal = new Deal(clientId, commodity, totalValue, currency,
                assignedSalesUserId, origin, destination, notes);

            deal.SetCreatedAt(createdAt);
            return deal;
        }

        private static void AdvanceDeal(Deal deal, DealStatus targetStatus, int userId, DateTime baseTime)
        {
            // Walk the deal through every stage up to target
            var stages = new[]
            {
            DealStatus.Negotiation, DealStatus.Confirmed, DealStatus.Shipping,
            DealStatus.Customs,     DealStatus.Delivered, DealStatus.Closed
        };

            int dayOffset = 3;
            foreach (var stage in stages)
            {
                if (deal.Status >= targetStatus) break;
                deal.MoveToStatus(stage, userId, $"Moved to {stage}");
                deal.SetUpdatedAt(baseTime.AddDays(dayOffset));
                dayOffset += 5;
            }
        }

        private static void AddShipment(
            AppDbContext context,
            Guid dealId, string carrier,
            string pol, string pod,
            DateTime? etd, DateTime? eta,
            ShipmentStatus status,
            string? tracking = null)
        {
            var shipment = new Shipment(dealId, carrier, pol, pod, etd, eta);
            shipment.UpdateStatus(status, tracking);
            context.Shipments.Add(shipment);
        }

        private static void AddPayment(
            AppDbContext context,
            Guid dealId, decimal amount, string currency,
            decimal exchangeRate, PaymentType type,
            DateTime dueDate, PaymentStatus status,
            DateTime? paidAt = null)
        {
            var payment = new Payment(dealId, amount, currency, exchangeRate, type, dueDate);

            if (status == PaymentStatus.FullyPaid)
                payment.MarkAsPaid();
            else if (status == PaymentStatus.Overdue)
                payment.MarkAsOverdue();

            context.Payments.Add(payment);
        }

        private static void AddDocument(
            AppDbContext context,
            Guid dealId, DocumentType docType,
            string fileName, int uploadedByUserId,
            Guid? shipmentId = null)
        {
            var doc = new Document(
                dealId, docType, fileName,
                $"deals/{dealId}/documents/{fileName}",
                fileSizeBytes: Faker.FakeFileSize(),
                mimeType: "application/pdf",
                uploadedByUserId: uploadedByUserId,
                shipmentId: shipmentId
            );

            context.Documents.Add(doc);
        }

        private static void AddTask(
            AppDbContext context,
            Guid dealId, int assignedToUserId,
            string title, string description,
            TaskPriority priority,
            DateTime? dueDate = null)
        {
            var task = new DealTask(dealId, assignedToUserId, title, description, priority, dueDate);
            context.Tasks.Add(task);
        }

        // ── Fake helpers ────────────────────────────────────────────────
        private static class Faker
        {
            private static readonly Random Rng = new(42);
            public static long FakeFileSize() => Rng.Next(45_000, 850_000);
        }
    }
}
