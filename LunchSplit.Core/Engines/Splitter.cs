using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunchSplit.Core.Models;

namespace LunchSplit.Core.Engines
{
    public class Splitter
    {
        public decimal CalculateTip(decimal totalAmount, TipMode tipMode, decimal tipInput)
        {
            if (totalAmount < 0)
            {
                throw new ArgumentException("Total amount cannot be negative.");

            }
            if (tipMode == TipMode.Fixed && tipInput < 0)
            {
                throw new ArgumentException("Tip input cannot be negative.");
            }

            return tipMode switch
            {
                TipMode.None => 0m,
                TipMode.Percent => Math.Round(totalAmount * (tipInput / 100), 4),
                TipMode.Fixed => tipInput,
                _ => 0m
            };


        }

        //public List<Share> CalculateShares(Bill bill, List<Attendee> attendees, RoundingMode roundingMode)
        //{
        //    var activeAttendees = attendees.Where(a => a.Included).ToList();
        //    if(activeAttendees.Count == 0)
        //    {
        //        throw new InvalidOperationException("No active attendees to split the bill.");
        //    }

        //    int totalWeight = activeAttendees.Sum(a => a.Weight);

        //    decimal tip = CalculateTip(bill.Subtotal + bill.Tax, bill.TipMode, bill.TipInput);

        //    decimal grandTotal = bill.Subtotal + bill.Tax + tip;

        //    var rawShares = new List<Share>();
        //    foreach(var attendee in activeAttendees)
        //    {
        //        decimal shareAmount = ((decimal)attendee.Weight / (decimal)totalWeight) * grandTotal;
        //        rawShares.Add(new Share(attendee.Name, shareAmount));
        //    }
        //    var rounder = new Rounder();
        //    return rounder.RoundShares(rawShares, roundingMode, grandTotal);
        //}


        public List<Share> CalculateShare(Bill bill, List<Attendee> attendees, RoundingMode roundingMode)
        {
            var activeAttendees = attendees.Where(a => a.Included).ToList();

            if (activeAttendees.Count == 0)
                throw new ArgumentException(
                    "At least one attendee must be included in the split.",
                    nameof(attendees));

            int totalWeight = activeAttendees.Sum(a => a.Weight);
            if (totalWeight == 0)
                throw new ArgumentException(
                    "The combined weight of all included attendees cannot be zero.",
                    nameof(attendees));

            decimal tip = CalculateTip(bill.Subtotal, bill.TipMode, bill.TipInput);
            decimal grandTotal = bill.Subtotal + bill.Tax + tip;

            // Build raw (un-rounded) shares
            var rawShares = new List<Share>();
            foreach (var attendee in attendees)
            {
                if (!attendee.Included)
                {
                    rawShares.Add(new Share(attendee.Name, 0m));
                    continue;
                }

                decimal rawAmount = grandTotal * ((decimal)attendee.Weight / totalWeight);
                rawShares.Add(new Share(attendee.Name, rawAmount));
            }

            // Delegate rounding + reconciliation to Rounder.
            // Pass grandTotal so the Penny Reconciliation guarantee is enforced.
            var rounder = new Rounder();
            return rounder.RoundShares(rawShares, roundingMode, grandTotal);
        }

        public object CreatePaymentRequest(List<Share> shares)
        {
            // This method would create a payment request for Stripe or another payment processor.
            // Implementation would depend on the specific API and requirements.
            throw new NotImplementedException();
        }
    }
}