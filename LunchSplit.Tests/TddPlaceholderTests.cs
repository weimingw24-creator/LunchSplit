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
    public class TddPlaceholderTests
    {
        private readonly Splitter splitter = new();
        private readonly ReceiptFormatter receiptFormatter = new();
        //TDD-26
        [TestMethod]
        public void Format_CsvReceiptExporter_MatchesSchema()
        {
            //Arrange
            var bill = new Bill(100m, 13m, TipMode.None, 0m);
            var attendees = new List<Attendee>
            {
                new("Alice", 1),
                new("Bob", 1)
            };
            //Act
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.Bankers);
            string receipt = receiptFormatter.Format(bill, attendees, shares);
            string csvReceipt = receiptFormatter.ExportToCsv(receipt);
            //Assert
            StringAssert.Contains(csvReceipt, "Alice");
            StringAssert.Contains(csvReceipt, "Bob");
            StringAssert.Contains(csvReceipt, "56.50");
        }
        //TDD-27
        [TestMethod]
        public void CalculateShares_PaymentRequest_ValidatesStripeMock()
        {
            // Arrange 
            var bill = new Bill(100m, 13m, TipMode.None, 0m);
            var attendees = new List<Attendee>
    {
        new("Alice", 1),
        new("Bob", 1)
    };

            // Act 
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.Bankers);
            var paymentRequest = splitter.CreatePaymentRequest(shares);

            // Assert 
            Assert.IsNotNull(paymentRequest, "Payment request must be generated before Stripe validation.");
        }

    }
}

