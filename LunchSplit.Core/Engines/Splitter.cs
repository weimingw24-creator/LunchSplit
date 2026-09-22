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
            if(totalAmount < 0)
            {
                throw new ArgumentException("Total amount cannot be negative.");
                 
            }
            if (tipMode== TipMode.Fixed && tipInput < 0)
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

        public List<Share> CalculateShares(Bill bill, List<Attendee> attendees, RoundingMode roundingMode)
        {
            var activeAttendees = attendees.Where(a => a.Included).ToList();
            if(activeAttendees.Count == 0)
            {
                throw new InvalidOperationException("No active attendees to split the bill.");
            }

            int totalWeight = activeAttendees.Sum(a => a.Weight);

            decimal tip = CalculateTip(bill.Subtotal + bill.Tax, bill.TipMode, bill.TipInput);

            decimal grandTotal = bill.Subtotal + bill.Tax + tip;

            var rawShares = new List<Share>();
            foreach(var attendee in activeAttendees)
            {
                decimal shareAmount = (attendee.Weight / (decimal)totalWeight) * grandTotal;
                rawShares.Add(new Share(attendee.Name, shareAmount));
            }
            var rounder = new Rounder();
            return rounder.RoundShares(rawShares, roundingMode, grandTotal);
        }



    }
}
