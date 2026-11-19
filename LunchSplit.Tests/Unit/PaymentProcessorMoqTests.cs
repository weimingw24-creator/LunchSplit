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


    }
}
