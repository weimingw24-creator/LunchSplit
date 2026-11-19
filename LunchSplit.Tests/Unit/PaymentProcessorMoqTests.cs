using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LunchSplit.Core.Engines;
using LunchSplit.Core.Models;
using LunchSplit.Core.Payments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LunchSplit.Tests.Unit
{
    /// <summary>
    /// Tier 2 of the Assignment 3 test suite.
    /// Every test in this class runs fully in memory: the Square SDK is replaced
    /// by a Moq proxy over <see cref="IPaymentProcessor"/>, so no network, no
    /// credentials and no sandbox account are required.
    /// </summary>
    [TestClass]
    public class PaymentProcessorMoqTests
    {
        /// <summary>
        /// M3-03 - a gateway that throws HttpRequestException (socket drop,
        /// DNS failure, firewall) must surface as the domain-level
        /// PaymentGatewayTimeoutException so callers can retry cleanly.
        /// </summary>
        [TestMethod]
        public async Task M3_03_ProcessPaymentAsync_GatewayThrowsHttpRequestException_ThrowsPaymentGatewayTimeoutException()
        {
            // Arrange
            var mockProcessor = new Mock<IPaymentProcessor>();
            mockProcessor
                .Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Simulated socket drop"));

            var manager = new PaymentManager(mockProcessor.Object);
            var request = new PaymentRequest(
                Amount: 25.00m,
                CurrencyCode: "CAD",
                IdempotencyKey: "m3-03-key",
                CardNonce: "cnon:card-nonce-ok",
                ReferenceId: "Alice");

            // Act & Assert
            await Assert.ThrowsExactlyAsync<PaymentGatewayTimeoutException>(
                () => manager.ProcessPaymentAsync(request));
        }

        /// <summary>
        /// M3-04 - the manager must hand the exact dollar amount to the
        /// processor. Mock.Verify() proves both the value and that the call
        /// happened exactly once.
        /// </summary>
        [TestMethod]
        public async Task M3_04_ProcessPaymentAsync_VerifiesExactAmountPassedToProcessor()
        {
            // Arrange
            var mockProcessor = new Mock<IPaymentProcessor>();
            mockProcessor
                .Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PaymentResult.Success("txn-m3-04"));

            var manager = new PaymentManager(mockProcessor.Object);
            var request = new PaymentRequest(
                Amount: 32.75m,
                CurrencyCode: "CAD",
                IdempotencyKey: "m3-04-key",
                CardNonce: "cnon:card-nonce-ok",
                ReferenceId: "Bob");

            // Act
            await manager.ProcessPaymentAsync(request);

            // Assert - the processor received exactly $32.75, once.
            mockProcessor.Verify(
                p => p.ProcessPaymentAsync(
                    It.Is<PaymentRequest>(r => r.Amount == 32.75m),
                    It.IsAny<CancellationToken>()),
                Times.Once,
                "The processor must receive the exact dollar amount.");
        }

        /// <summary>
        /// M3-05 - a $0.00 bill has nothing to charge. The manager must
        /// short-circuit and never touch the gateway.
        /// </summary>
        [TestMethod]
        public async Task M3_05_ProcessBillAsync_ZeroDollarBill_NeverInvokesProcessor()
        {
            // Arrange
            var mockProcessor = new Mock<IPaymentProcessor>();
            var manager = new PaymentManager(mockProcessor.Object);

            var zeroBill = new Bill(Subtotal: 0.00m, Tax: 0.00m, TipMode: TipMode.None, TipInput: 0.00m);
            var attendees = new List<Attendee>
            {
                new("Alice", weight: 1, included: true),
                new("Bob", weight: 1, included: true)
            };

            // Act
            var summary = await manager.ProcessBillAsync(
                zeroBill, attendees, RoundingMode.Bankers, "cnon:card-nonce-ok");

            // Assert
            mockProcessor.Verify(
                p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()),
                Times.Never,
                "A $0.00 bill must never call the payment gateway.");
            Assert.IsFalse(summary.HasPayments, "No payment lines should be produced.");
        }

        /// <summary>
        /// M3-06 - a four-attendee split must produce exactly four gateway
        /// calls, one per included attendee.
        /// </summary>
        [TestMethod]
        public async Task M3_06_ProcessBillAsync_FourAttendeeSplit_InvokesProcessorExactlyFourTimes()
        {
            // Arrange
            var mockProcessor = new Mock<IPaymentProcessor>();
            mockProcessor
                .Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PaymentResult.Success("txn-m3-06"));

            var manager = new PaymentManager(mockProcessor.Object);
            var bill = new Bill(Subtotal: 80.00m, Tax: 0.00m, TipMode: TipMode.None, TipInput: 0.00m);
            var attendees = new List<Attendee>
            {
                new("Alice", weight: 1, included: true),
                new("Bob", weight: 1, included: true),
                new("Carol", weight: 1, included: true),
                new("Dave", weight: 1, included: true)
            };

            // Act
            var summary = await manager.ProcessBillAsync(
                bill, attendees, RoundingMode.Bankers, "cnon:card-nonce-ok");

            // Assert
            mockProcessor.Verify(
                p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()),
                Times.Exactly(4),
                "Each included attendee must be charged once.");
            Assert.AreEqual(4, summary.SuccessCount);
        }


    }
}
