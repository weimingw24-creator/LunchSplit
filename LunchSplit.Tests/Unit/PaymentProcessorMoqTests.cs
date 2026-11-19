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

    }
}
