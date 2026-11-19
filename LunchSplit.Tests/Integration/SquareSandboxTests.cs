using System;
using System.Threading.Tasks;
using LunchSplit.Core.Payments;
using LunchSplit.Infrastructure;
using LunchSplit.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LunchSplit.Tests.Integration
{
    /// <summary>
    /// Tier 1 of the Assignment 3 test suite: live calls against the Square
    /// Sandbox gateway using the documented Magic Test Nonces.
    ///
    /// These tests require SQUARE_SANDBOX_ACCESS_TOKEN and
    /// SQUARE_SANDBOX_LOCATION_ID. When they are absent the tests report
    /// Inconclusive, so a fresh clone can still run `dotnet test` for Tier 2.
    /// </summary>
    [TestClass]
    [TestCategory("Tier1")]
    public class SquareSandboxTests
    {
        /// <summary>Sandbox timeout used by every Tier 1 test.</summary>
        private static readonly TimeSpan SandboxTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// M3-01 - the magic nonce cnon:card-nonce-ok must authorise a live
        /// payment and return a non-empty transaction id.
        /// </summary>
        [TestMethod]
        public async Task M3_01_ProcessPaymentAsync_MagicNonceOk_ReturnsSuccessfulPaymentWithTransactionId()
        {
            var processor = CreateProcessor();

            var request = NewRequest(12.50m, "CAD", "cnon:card-nonce-ok");

            var result = await processor.ProcessPaymentAsync(request);

            Assert.IsTrue(result.IsSuccess, $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.TransactionId),
                "A successful sandbox payment must return a transaction id.");
        }

        /// <summary>
        /// M3-02 - the magic nonce cnon:card-nonce-declined must be rejected
        /// with the CARD_DECLINED error code.
        /// </summary>
        [TestMethod]
        public async Task M3_02_ProcessPaymentAsync_MagicNonceDeclined_ReturnsCardDeclined()
        {
            var processor = CreateProcessor();

            var request = NewRequest(12.50m, "CAD", "cnon:card-nonce-declined");

            var result = await processor.ProcessPaymentAsync(request);

            Assert.IsFalse(result.IsSuccess, "A declined nonce must not produce a successful payment.");
            Assert.AreEqual("CARD_DECLINED", result.ErrorCode,
                "Square must report the decline as CARD_DECLINED.");
        }

        /// <summary>
        /// M3-08 - a CAD location must reject a charge submitted in EUR
        /// with an HTTP 400 validation error.
        /// </summary>
        [TestMethod]
        public async Task M3_08_ProcessPaymentAsync_UnsupportedCurrency_ReturnsHttp400ValidationError()
        {
            var processor = CreateProcessor();

            var request = NewRequest(12.50m, "EUR", "cnon:card-nonce-ok");

            var result = await processor.ProcessPaymentAsync(request);

            Assert.IsFalse(result.IsSuccess, "A currency mismatch must not succeed.");
            Assert.AreEqual(400, result.HttpStatusCode,
                "Square must reject the request with HTTP 400.");
        }


    }
}
