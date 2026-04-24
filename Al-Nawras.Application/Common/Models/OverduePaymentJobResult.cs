using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Models
{
    public record OverduePaymentJobResult(
        int ProcessedCount,
        int MarkedOverdueCount,
        int NotificationsSentCount,
        List<string> Errors,
        DateTime RanAt,
        TimeSpan Duration
    )
    {
        public bool HasErrors => Errors.Count > 0;

        public override string ToString() =>
            $"Ran at {RanAt:yyyy-MM-dd HH:mm:ss} UTC | " +
            $"Processed: {ProcessedCount} | " +
            $"Marked overdue: {MarkedOverdueCount} | " +
            $"Notifications: {NotificationsSentCount} | " +
            $"Errors: {Errors.Count} | " +
            $"Duration: {Duration.TotalSeconds:F2}s";
    }
}
