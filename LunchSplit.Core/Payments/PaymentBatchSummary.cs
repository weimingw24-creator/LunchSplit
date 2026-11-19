using System.Collections.Generic;
using System.Linq;

namespace LunchSplit.Core.Payments;

/// <summary>
/// One attendee's payment line inside a batch run.
/// </summary>
public sealed record PaymentLine(string AttendeeName, PaymentResult Result);

/// <summary>
/// Aggregate outcome of charging a whole split bill. A decline for one attendee
/// must never abort the remaining charges, so the summary reports every line
/// and lets the caller decide what to retry.
/// </summary>
public sealed class PaymentBatchSummary
{
    private readonly List<PaymentLine> _lines = new();

    /// <summary>Every attempted charge, in the order the shares were produced.</summary>
    public IReadOnlyList<PaymentLine> Lines => _lines;

    /// <summary>Number of charges the gateway authorised.</summary>
    public int SuccessCount => _lines.Count(l => l.Result.IsSuccess);

    /// <summary>Number of charges the gateway rejected.</summary>
    public int FailureCount => _lines.Count(l => !l.Result.IsSuccess);

    /// <summary>True when at least one attempt was made.</summary>
    public bool HasPayments => _lines.Count > 0;

    /// <summary>Records one attempt.</summary>
    public void Add(string attendeeName, PaymentResult result) =>
        _lines.Add(new PaymentLine(attendeeName, result));

    /// <summary>Names of attendees whose charge failed, useful for a retry prompt.</summary>
    public IReadOnlyList<string> FailedAttendees =>
        _lines.Where(l => !l.Result.IsSuccess).Select(l => l.AttendeeName).ToList();
}
