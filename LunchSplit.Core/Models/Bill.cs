using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunchSplit.Core.Models
{
    public record Bill
    {
        decimal _Subtotal;
        decimal _Tax;
        TipMode _TipMode;
        decimal _TipInput;

        
    }
}
