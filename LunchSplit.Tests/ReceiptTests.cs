using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunchSplit.Core.Engines;
using LunchSplit.Core.Models;
using LunchSplit.Core.Output;

namespace LunchSplit.Tests
{
    [TestClass]
    public class ReceiptTests
    {
        private readonly Splitter splitter = new();
        private readonly ReceiptFormatter receiptFormatter = new();
        //M5-24
        [TestMethod]
        public void Format_DefaultBillReceipt_ContainsLineItems()
        {
            // Arrange
            var bill = new Bill(100m, 13m, TipMode.None, 0m);
            var attendees = new List<Attendee>
            {
                new("Alice", 1),
                new("Bob", 1)
            };
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.Bankers);

            // Act
            string receipt = receiptFormatter.Format(bill, attendees, shares);

            // Assert
            StringAssert.Contains(receipt, "Alice");
            StringAssert.Contains(receipt, "Bob");
            StringAssert.Contains(receipt, "56.50");   // (100+13)/2 = 56.50
        }
        //M5-25
        [TestMethod]
        public void Format_ReceiptOutput_ContainsRequiredMetadata()
        {
            // Arrange
            var bill = new Bill(100m, 13m, TipMode.None, 0m);
            var attendees = new List<Attendee>
            {
                new("Alice", 1),
                new("Bob", 1)
            };
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.Bankers);
            // Act
            string receipt = receiptFormatter.Format(bill, attendees, shares);
            // Assert
            StringAssert.Contains(receipt, "Weiming");
            StringAssert.Contains(receipt, DateTime.Now.ToString("yyyy - MM - dd"));
            StringAssert.Contains(receipt, DateTime.Now.ToString("yyyy - MM - dd HH:mm:ss"));
        }
     }
}
