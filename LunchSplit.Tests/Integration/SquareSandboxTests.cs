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


    }
}
