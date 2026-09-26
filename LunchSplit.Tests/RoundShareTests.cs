using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunchSplit.Core.Engines;
using LunchSplit.Core.Models;

namespace LunchSplit.Tests
{
    [TestClass]
    public class RoundShareTests
    {
        private readonly Rounder rounder = new();

        //M2-6
        [TestMethod]
        public void RoundShares_NoRounding_PreservesRawDecimals()
        {
            var shares = new List<Share>
            {
                new ("Alice", 10.123m),
                new ("Bob", 20.456m),
                new ("Charlie", 30.789m)
            };

            List<Share> roundedShares = rounder.RoundShares(shares, RoundingMode.None);

            Assert.AreEqual(10.123m, roundedShares[0].Amount, "Alice's share is correct");
            Assert.AreEqual(20.456m, roundedShares[1].Amount, "Bob's share is correct");
            Assert.AreEqual(30.789m, roundedShares[2].Amount, "Charlie's share is correct");
        }
        //M2-7 IEEE754
        [TestMethod]
        public void RoundShares_BankersRounding_RoundsToNearestEven()
        {
            var shares = new List<Share>
            {
                new ("Alice", 10.325m),
                new ("Bob", 10.335m),
            };

            List<Share> roundedShares = rounder.RoundShares(shares, RoundingMode.Bankers);

            Assert.AreEqual(10.32m, roundedShares[0].Amount, "Alice's share is correct");
            Assert.AreEqual(10.34m, roundedShares[1].Amount, "Bob's share is correct");
        }
        //M2-8
        [TestMethod]
        public void RoundShares_RoundUp_AppliesCeilingToCents()
        {
            var shares = new List<Share>
            {
                new ("Alice", 10.321m),
                new ("Bob", 10.329m),
            };
            List<Share> roundedShares = rounder.RoundShares(shares, RoundingMode.RoundUp);
            Assert.AreEqual(10.33m, roundedShares[0].Amount, "Alice's share is correct");
            Assert.AreEqual(10.33m, roundedShares[1].Amount, "Bob's share is correct");
        }
        //M2-9
        [TestMethod]
        public void RoundShares_RoundDown_AppliesFloorToCents()
        {
            var shares = new List<Share>
            {
                new ("Alice", 10.321m),
                new ("Bob", 10.329m),
            };
            List<Share> roundedShares = rounder.RoundShares(shares, RoundingMode.RoundDown);
            Assert.AreEqual(10.32m, roundedShares[0].Amount, "Alice's share is correct");
            Assert.AreEqual(10.32m, roundedShares[1].Amount, "Bob's share is correct");
        }
        //M2-10
        [TestMethod]
        public void RoundShares_UnevenSplit_ReconcilesRemainder()
        {
            decimal totalAmount = 10.00m;
            decimal splitAmount = totalAmount / 3m; // 3.333333...
            var shares = new List<Share>
            {
                new ("Alice", splitAmount),
                new ("Bob", splitAmount),
                new ("Charlie", splitAmount)
            };

            List<Share> roundedShares = rounder.RoundShares(shares, RoundingMode.Bankers, totalAmount);

            decimal actualSum = roundedShares.Sum(s => s.Amount);
            Assert.AreEqual(totalAmount, actualSum,
                "Penny reconciliation must ensure sum(shares) == $10.00.");

            // One share must be $3.34 and two must be $3.33
            int countHigher = roundedShares.Count(s => s.Amount == 3.34m);
            int countLower = roundedShares.Count(s => s.Amount == 3.33m);
            Assert.AreEqual(1, countHigher, "Exactly one share must absorb the reconciled penny ($3.34).");
            Assert.AreEqual(2, countLower, "The remaining two shares must each be $3.33.");
        }
        //M2-11
        [TestMethod]
        public void RoundShares_EmptyShareCollection_ReturnsEmpty()
        {
            var shares = new List<Share>();

            List<Share> roundedShares = rounder.RoundShares(shares, RoundingMode.Bankers);

            Assert.AreEqual(0, roundedShares.Count, "Empty share collection should return empty.");
        }

    }
}
