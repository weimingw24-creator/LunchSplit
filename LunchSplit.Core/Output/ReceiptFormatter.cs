using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunchSplit.Core.Models;

namespace LunchSplit.Core.Output
{
    public class ReceiptFormatter
    {
        private const string StudentName = "Weiming";

        public string Format(Bill bill, List<Attendee> attendees, List<Share> roundedShares)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== BILL SPLIT RECEIPT ===");
            sb.AppendLine($"Student: {StudentName}");
            sb.AppendLine($"Date: {DateTime.Now:yyyy - MM - dd}");
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy - MM - dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine($"Subtotal: ${bill.Subtotal:F2}");
            sb.AppendLine($"Tax: ${bill.Tax:F2}");
            sb.AppendLine();
            sb.AppendLine("--- Attendee Shares ---");

            foreach (var share in roundedShares)
            {
                sb.AppendLine($"{share.Name}: ${share.Amount:F2}");
            }

            sb.AppendLine();
            sb.AppendLine("========================");

            return sb.ToString();
        }

        public string ExportToCsv(string receipt)
        {
            throw new NotImplementedException("CSV export functionality is not implemented yet.");
        }
    }
}