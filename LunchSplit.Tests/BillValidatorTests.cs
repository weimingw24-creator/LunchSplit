using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunchSplit.Core.Models;
using LunchSplit.Core.Helpers;

namespace LunchSplit.Tests
{
    [TestClass]
    public class BillValidatorTests
    {
        [TestMethod]
        public void Validate_CompleteBillDetails_ReturnsOk()
        {
            // Arrange
            var bill = new Bill(100m, 10m, TipMode.Percent, 15m);
            var attendees = new List<Attendee>
            {
                new ("Alice", weight:1, included: true),
                new ("Bob", weight:1, included: true)
            };
            var validator = new BillValidator();
            // Act
            var result = validator.Validate(bill, attendees);
            // Assert
            Assert.IsTrue(result._IsValid);
            Assert.AreEqual(string.Empty, result._ErrorMessage);
        }
        [TestMethod]
        public void Validate_EmptyAttendeesList_ReturnsError()
        {
            // Arrange
            var bill = new Bill(100m, 10m, TipMode.Percent, 15m);
            var attendees = new List<Attendee>();
            var validator = new BillValidator();
            // Act
            var result = validator.Validate(bill, attendees);
            // Assert
            Assert.IsFalse(result._IsValid);
            Assert.AreEqual("attendees list must not be empty.", result._ErrorMessage);
        }
        [TestMethod]
        public void Validate_NegativeSubtotal_ReturnsError()
        {
            // Arrange
            var bill = new Bill(-100m, 10m, TipMode.Percent, 15m);
            var attendees = new List<Attendee>
            {
                new ("Alice", weight:1, included: true),
                new ("Bob", weight:1, included: true)
            };
            var validator = new BillValidator();
            // Act
            var result = validator.Validate(bill, attendees);
            // Assert
            Assert.IsFalse(result._IsValid);
            Assert.AreEqual("subtotal must be greater than 0.", result._ErrorMessage);
        }

        [TestMethod]
        public void Validate_NegativeTax_ReturnsError()
        {
            // Arrange
            var bill = new Bill(100m, -10m, TipMode.Percent, 15m);
            var attendees = new List<Attendee>
            {
                new ("Alice", weight:1, included: true),
                new ("Bob", weight:1, included: true)
            };
            var validator = new BillValidator();
            // Act
            var result = validator.Validate(bill, attendees);
            // Assert
            Assert.IsFalse(result._IsValid);
            Assert.AreEqual("tax must be greater than or equal to 0.", result._ErrorMessage);
        }

    }
    
}
