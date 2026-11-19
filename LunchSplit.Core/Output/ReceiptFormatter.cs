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

        /// <summary>
        /// Converts the formatted receipt into a two-column CSV
        /// (Attendee,Amount) so the split can be imported into a spreadsheet.
        /// Only the attendee share block is exported; the bill header lines
        /// (subtotal, tax, timestamps) are intentionally skipped.
        /// </summary>
        public string ExportToCsv(string receipt)
        {
            if (string.IsNullOrWhiteSpace(receipt))
            {
                throw new ArgumentException("A formatted receipt is required.", nameof(receipt));
            }

            var csv = new StringBuilder();
            csv.AppendLine("Attendee,Amount");

            var insideShareBlock = false;
            foreach (var rawLine in receipt.Split('\n'))
            {
                var line = rawLine.Trim();

                if (line.StartsWith("--- Attendee Shares ---", StringComparison.Ordinal))
                {
                    insideShareBlock = true;
                    continue;
                }

                if (!insideShareBlock || line.Length == 0)
                {
                    continue;
                }

                if (line.StartsWith("===", StringComparison.Ordinal))
                {
                    break;
                }

                var separatorIndex = line.LastIndexOf(": $", StringComparison.Ordinal);
                if (separatorIndex < 0)
                {
                    continue;
                }

                var attendee = line.Substring(0, separatorIndex).Trim();
                var amountText = line.Substring(separatorIndex + 3).Trim();

                if (attendee.Length == 0 || !decimal.TryParse(amountText, out var amount))
                {
                    continue;
                }

                csv.AppendLine($"{attendee},{amount:F2}");
            }

            return csv.ToString();
        }
    }
}