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
        //TDD-26
        [TestMethod]
        public void Format_CsvReceiptExporter_MatchesSchema()
        {
            //Arrange
            var bill = new Bill(100m,13m,TipMode.None, 0m);
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
            // Arrange —— 一份账单 + 两个参与者
            var bill = new Bill(100m, 13m, TipMode.None, 0m);
            var attendees = new List<Attendee>
    {
        new("Alice", 1),
        new("Bob", 1)
    };

            // Act —— 算出分摊结果，并模拟生成支付请求
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.Bankers);
            var paymentRequest = splitter.CreatePaymentRequest(shares);  // ← 未来 F6 的方法

            // Assert —— 占位：期待支付请求通过 Stripe 校验
            // 此方法尚未实现，将保持 RED 直到 F6
            Assert.IsNotNull(paymentRequest, "Payment request must be generated before Stripe validation.");
        }

    }
}
