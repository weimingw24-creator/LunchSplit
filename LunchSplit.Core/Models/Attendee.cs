using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunchSplit.Core.Models
{
    public class Attendee
    {
        public string Name { get; set; }
        public int Weight { get; set; }
        public bool Included { get; set; }

        public Attendee(string name, int weight = 1, bool included= true)
        {
            Name = name;
            Weight = weight;
            Included = included;
        }
    }
}
