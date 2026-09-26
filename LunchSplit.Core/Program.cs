using System;
using System.Collections.Generic;
using LunchSplit.Core.Models;
using LunchSplit.Core.Engines;
namespace LunchSplit.Core
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bill bill = new Bill(100.00m, 15.00m, TipMode.Percent, 15.00m);

            List<Attendee> attendees = new List<Attendee>
            {
                new Attendee("Alice", 1, true),
                new Attendee("Bob", 2, true),
                new Attendee("Charlie", 1, false)
            };

            var splitter = new Splitter();
            var result = splitter.CalculateShare(bill, attendees, RoundingMode.RoundDown);
            
            foreach (var item in result)
            {
                Console.WriteLine(item); // 内部自动调用 item.ToString()
            }


        }
    }
}
