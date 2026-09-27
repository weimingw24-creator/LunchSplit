using LunchSplit.Core.Models;
using LunchSplit.Core.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LunchSplit.Tests
{
    [TestClass]
    public sealed class TipComputeTest
    {
        [TestMethod]
        public void ComputeTip_NoTipMode_ReturnsZero()
        {
            Splitter splitter = new Splitter();
            decimal totalAmount = 120.00m;
            TipMode tipMode = TipMode.None;
            decimal tipInput = 0m;

            decimal actualResult = splitter.CalculateTip(totalAmount, tipMode, tipInput);

            Assert.AreEqual(0.00m, actualResult,"TipMode None should return zero tip");
        }

        [TestMethod]
        public void ComputeTip_PercentTipMode_ReturnsCorrectTip()
        {
            Splitter splitter = new Splitter();
            decimal totalAmount = 100m;
            TipMode tipMode = TipMode.Percent;
            decimal tipInput = 15m;

            decimal actualResult = splitter.CalculateTip(totalAmount, tipMode, tipInput);

            Assert.AreEqual(15m, actualResult);
        }
        [TestMethod]
        public void ComputeTip_FixedTipMode_ReturnsCorrectTip()
        {
            Splitter splitter = new Splitter();
            decimal totalAmount = 100m;
            TipMode tipMode = TipMode.Fixed;
            decimal tipInput = 20m;

            decimal actualResult = splitter.CalculateTip(totalAmount, tipMode, tipInput);

            Assert.AreEqual(20m, actualResult);
        }

        [TestMethod]
        public void ComputeTip_NegativeSubtotal_ThrowsArgumentOutOfRange()
        {

            Splitter splitter = new Splitter();
            decimal badSubtotal = -100m;

            Assert.Throws<ArgumentException>(() => splitter.CalculateTip(badSubtotal, TipMode.Percent, 15m));
        }

        [TestMethod]
        public void ComputeTip_NegativeFixedTip_ThrowsArgumentOutOfRange()
        {
            Splitter splitter = new Splitter();
            decimal totalAmount = 100m;
            TipMode tipMode = TipMode.Fixed;
            decimal badTipInput = -20m;
            Assert.Throws<ArgumentException>(() => splitter.CalculateTip(totalAmount, tipMode, badTipInput));
        }

       
    }
}
