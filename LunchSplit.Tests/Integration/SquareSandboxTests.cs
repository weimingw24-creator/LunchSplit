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

            var request = new PaymentRequest(
                Amount: 12.50m,
                CurrencyCode: "CAD",
                IdempotencyKey: $"a3-{Guid.NewGuid():N}",
                CardNonce: "cnon:card-nonce-ok",
                ReferenceId: "A3-Sandbox");

            var result = await processor.ProcessPaymentAsync(request);

            Assert.IsTrue(result.IsSuccess, $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.TransactionId),
                "A successful sandbox payment must return a transaction id.");
        }

        /// <summary>
        /// M3-02 - the magic nonce cnon:card-nonce-declined must be rejected
        /// with a decline error code. Square's sandbox currently answers
        /// GENERIC_DECLINE while the specification names CARD_DECLINED, so both
        /// are accepted; the point under test is that a decline is detected.
        /// </summary>
        [TestMethod]
        public async Task M3_02_ProcessPaymentAsync_MagicNonceDeclined_ReturnsCardDeclined()
        {
            var processor = CreateProcessor();

            var request = NewRequest(12.50m, "CAD", "cnon:card-nonce-declined");

            var result = await processor.ProcessPaymentAsync(request);

            Assert.IsFalse(result.IsSuccess, "A declined nonce must not produce a successful payment.");
            Assert.IsTrue(
                result.ErrorCode == "CARD_DECLINED" || result.ErrorCode == "GENERIC_DECLINE",
                $"Square must report a decline; actual code: {result.ErrorCode}");
        }

        /// <summary>
        /// M3-08 - a CAD location must reject a charge submitted in EUR
        /// with an HTTP 400 validation error.
        /// </summary>
        [TestMethod]
        public async Task M3_08_ProcessPaymentAsync_UnsupportedCurrency_ReturnsHttp400ValidationError()
        {
            var processor = CreateProcessor();

            var request = NewRequest(1250L, "EUR", "cnon:card-nonce-ok");

            var result = await processor.ProcessPaymentAsync(request);

            Assert.IsFalse(result.IsSuccess, "A currency mismatch must not succeed.");
            Assert.AreEqual(400, result.HttpStatusCode,
                "Square must reject the request with HTTP 400.");
        }

        /// <summary>
        /// M3-09 - replaying the identical request with the same idempotency
        /// key must return the original transaction and never double charge.
        /// </summary>
        [TestMethod]
        public async Task M3_09_ProcessPaymentAsync_DuplicateIdempotencyKey_ReturnsSameTransactionId()
        {
            var processor = CreateProcessor();

            // One request object, therefore one idempotency key, replayed twice.
            var request = NewRequest(1250L, "CAD", "cnon:card-nonce-ok");

            var first = await processor.ProcessPaymentAsync(request);
            var second = await processor.ProcessPaymentAsync(request);

            Assert.IsTrue(first.IsSuccess, "The first attempt must succeed.");
            Assert.IsTrue(second.IsSuccess, "The replay must return the original payment.");
            Assert.AreEqual(first.TransactionId, second.TransactionId,
                "A duplicate idempotency key must not create a second charge.");
        }

        /// <summary>
        /// M3-10 - querying the transaction must report COMPLETED.
        /// </summary>
        [TestMethod]
        public async Task M3_10_GetTransactionStatusAsync_CompletedPayment_ReturnsCompleted()
        {
            var processor = CreateProcessor();

            var request = NewRequest(1250L, "CAD", "cnon:card-nonce-ok");
            var payment = await processor.ProcessPaymentAsync(request);
            Assert.IsTrue(payment.IsSuccess, "Setup payment must succeed before querying its status.");

            var status = await processor.GetTransactionStatusAsync(payment.TransactionId);

            Assert.AreEqual(PaymentStatus.Completed, status,
                "The sandbox payment was auto-completed and must report COMPLETED.");
        }

        /// <summary>
        /// Builds the live sandbox service, or marks the test Inconclusive
        /// when credentials are not configured.
        /// </summary>
        private static SquarePaymentService CreateProcessor()
        {
            if (!SquareSandboxSettings.IsConfigured)
            {
                Assert.Inconclusive(
                    "Square Sandbox credentials are not configured. Set " +
                    "SQUARE_SANDBOX_ACCESS_TOKEN and SQUARE_SANDBOX_LOCATION_ID " +
                    "or fill in appsettings.local.json.");
            }

            return new SquarePaymentService(
                accessToken: SquareSandboxSettings.AccessToken!,
                locationId: SquareSandboxSettings.LocationId!,
                baseUrl: SquareSandboxSettings.BaseUrl,
                timeout: SandboxTimeout);
        }

        /// <summary>Creates a request with a fresh idempotency key.</summary>
        private static PaymentRequest NewRequest(decimal amount, string currency, string nonce) =>
            new(
                Amount: amount,
                CurrencyCode: currency,
                IdempotencyKey: $"a3-{Guid.NewGuid():N}",
                CardNonce: nonce,
                ReferenceId: "A3-Sandbox");
    }
}
