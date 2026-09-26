using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunchSplit.Core.Models;
using LunchSplit.Core.Engines;

namespace LunchSplit.Tests
{
    [TestClass]
    public class CalculateShareTests
    {
        private readonly Splitter splitter = new();
        //M4-17
        //[TestMethod]
        //public void CalculateShares_EqualSplit_ApportionsEvenly()
        //{
        //    // Arrange
        //    var bill = new Bill(90m, 0m, TipMode.None, 0m);
        //    var attendees = new List<Attendee>
        //    {
        //        new ("Alice", weight:1),
        //        new ("Bob", weight:1),
        //        new ("Charlie", weight:1)
        //    };

        //    // Act
        //    List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.None);

        //    // Assert
        //    Assert.HasCount(3, shares);
        //    Assert.AreEqual(30m, shares[0].Amount);
        //    Assert.AreEqual(30m, shares[1].Amount);
        //    Assert.AreEqual(30m, shares[2].Amount);
        //}
        [TestMethod]
        public void CalculateShares_EqualSplit_ApportionsEvenly()
        {
            // Arrange — $90.00 among 3 attendees with equal weight
            var bill = new Bill(
                Subtotal: 90.00m,
                Tax: 0m,
                TipMode: TipMode.None,
                TipInput: 0m);

            var attendees = new List<Attendee>
        {
            new("Alice", weight: 1),
            new("Bob",   weight: 1),
            new("Carol", weight: 1)
        };

            // Act
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.RoundUp);

            // Assert — each participant owes exactly $30.00
            Assert.HasCount(3, shares);
            Assert.AreEqual(30.00m, shares[0].Amount, "Alice must owe $30.00.");
            Assert.AreEqual(30.00m, shares[1].Amount, "Bob must owe $30.00.");
            Assert.AreEqual(30.00m, shares[2].Amount, "Carol must owe $30.00.");
            Assert.AreEqual(90.00m, shares.Sum(s => s.Amount), "Sum must equal $90.00.");
        }

        //M4-18
        [TestMethod]
        public void CalculateShares_ProportionalSplit_ApportionsWeighted()
        {
            // Arrange
            var bill = new Bill(90.00m, 0m, TipMode.None, 0m);
            var attendees = new List<Attendee>
            {
                new ("Alice", weight:1, included: true),
                new ("Bob", weight:2, included: true),
            };
            var splitter = new Splitter();
            // Act
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.Bankers);
            
            // Assert
            Assert.HasCount(2, shares);
            Assert.AreEqual(30.00m, shares[0].Amount); // Alice
            Assert.AreEqual(60.00m, shares[1].Amount); // Bob
        }
        //M4-19
        [TestMethod]
        public void CalculateShares_ExcludedAttendee_ApportionsZero()
        {
            // Arrange
            var bill = new Bill(60.00m, 0m, TipMode.None, 0m);
            var attendees = new List<Attendee>
            {
                new ("Alice", weight:1, included: true),
                new ("Bob", weight:1, included: true),
                new ("Charlie", weight:1, included: false)
            };
            // Act
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.Bankers);
            // Assert
            var charlieShare = shares.First(s => s.Name == "Charlie");
            Assert.AreEqual(0.00m, charlieShare.Amount,
                "Excluded attendee Charlie must receive exactly $0.00.");

            decimal activeTotal = shares.Where(s => s.Name != "Charlie").Sum(s => s.Amount);
            Assert.AreEqual(60.00m, activeTotal,
                "Active attendees must split the full $60.00.");
        }
        //M4-20
        [TestMethod]
        public void CalculateShares_TaxAndPercentTip_ApportionsTotal()
        {
            // Arrange
            var bill = new Bill(100.00m, 13.00m, TipMode.Percent, 15.00m);
            var attendees = new List<Attendee>
            {
                new ("Alice", weight:1, included: true),
                new ("Bob", weight:1, included: true)
            };
            // Act
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.Bankers);
            // Assert
            decimal expectedTotal = 100.00m + 13.00m + (100.00m * 0.15m); // $128.00
            decimal actualTotal = shares.Sum(s => s.Amount);
            Assert.AreEqual(expectedTotal, actualTotal,
                "The sum of all shares must equal the grand total of $128.00.");
        }
        //M4-21
        [TestMethod]
        public void CalculateShares_TaxAndFixedTip_ApportionsTotal()
        {
            // Arrange
            var bill = new Bill(100.00m, 13.00m, TipMode.Fixed, 20.00m);
            var attendees = new List<Attendee>
            {
                new ("Alice", weight:1, included: true),
                new ("Bob", weight:1, included: true)
            };
            // Act
            List<Share> shares = splitter.CalculateShare(bill, attendees, RoundingMode.Bankers);
            // Assert
            decimal expectedTotal = 100.00m + 13.00m + 20.00m; // $133.00
            decimal actualTotal = shares.Sum(s => s.Amount);
            Assert.AreEqual(expectedTotal, actualTotal,
                "The sum of all shares must equal the grand total of $133.00.");
        }
        //M4-22
        [TestMethod]
        public void CalculateShares_AllAttendeesExcluded_ThrowsException()
        {
            // Arrange
            var bill = new Bill(100.00m, 0m, TipMode.None, 0m);
            var attendees = new List<Attendee>
            {
                new ("Alice", weight:1, included: false),
                new ("Bob", weight:1, included: false)
            };
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                splitter.CalculateShare(bill, attendees, RoundingMode.Bankers),
                "An exception should be thrown when all attendees are excluded.");
        }
        //M4-23
        [TestMethod]
        public void CalculateShares_TotalWeightsZero_ThrowsException()
        {
            // Arrange
            var bill = new Bill(100.00m, 0m, TipMode.None, 0m);
            var attendees = new List<Attendee>
            {
                new ("Alice", weight:0, included: true),
                new ("Bob", weight:0, included: true)
            };
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                splitter.CalculateShare(bill, attendees, RoundingMode.Bankers),
                "An exception should be thrown when the total weight of included attendees is zero.");
        }

    }
}
