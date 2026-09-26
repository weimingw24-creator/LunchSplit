using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunchSplit.Core.Models;

namespace LunchSplit.Core.Helpers
{
    public class BillValidator
    {
        public ValidationResult Validate(Bill bill, List<Attendee> attendees)
        {
            //rule 1: subtotal must be greater than 0
            if (bill.Subtotal <= 0)
            {
                return new ValidationResult(false, "subtotal must be greater than 0.");

            }
            //rule 2: tax must be greater than or equal to 0
            if (bill.Tax < 0)
            {
                return new ValidationResult(false, "tax must be greater than or equal to 0.");
                
            }
            //rule 3: attendees list must not be empty
            if (attendees == null || attendees.Count == 0)
            {
                return new ValidationResult(false, "attendees list must not be empty.");

            }
            //rule 4: all attendees must have a weight greater than 0
            if (attendees.Any(a => a.Weight <0))
            {
                return new ValidationResult(false, "All attendees must have a weight greater than 0.");
                
            }
            return new ValidationResult(true, string.Empty);
        }
    }
}
