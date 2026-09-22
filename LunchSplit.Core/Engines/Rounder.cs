using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunchSplit.Core.Models;

namespace LunchSplit.Core.Engines;

public class Rounder
{
    public List<Share> RoundShares(List<Share> rawShares, RoundingMode mode,
                                   decimal? grandTotal = null)
    {
        var rounded = rawShares
           .Select(s => new Share(s.Name, ApplyRounding(s.Amount, mode)))
           .ToList();

        if (rounded.Sum(s => s.Amount) != grandTotal && grandTotal.HasValue)
        {
            var difference = grandTotal.Value - rounded.Sum(s => s.Amount);
            if (difference != 0)
            {
                // Adjust the first share to account for the rounding difference
                var firstShare = rounded.First();
                rounded[0] = new Share(firstShare.Name, firstShare.Amount + difference);
            }
        }
        return rounded;
    }

    public static decimal ApplyRounding(decimal amount, RoundingMode mode)
    {
        return mode switch
        {
            RoundingMode.None => amount,
            RoundingMode.Bankers => Math.Round(amount, 2, MidpointRounding.ToEven),
            RoundingMode.RoundUp => Math.Ceiling(amount * 100) / 100,
            RoundingMode.RoundDown => Math.Floor(amount * 100) / 100,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

}
