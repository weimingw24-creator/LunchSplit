namespace LunchSplit.Core.Payments;

/// <summary>
/// Domain view of a transaction's lifecycle state. The infrastructure layer is
/// responsible for mapping the vendor's string status onto this enum.
/// </summary>
public enum PaymentStatus
{
    /// <summary>The gateway has not produced a usable status yet.</summary>
    Unknown = 0,

    /// <summary>Funds were captured and the payment is final.</summary>
    Completed,

    /// <summary>Authorised but not yet captured.</summary>
    Approved,

    /// <summary>The payment was cancelled or failed.</summary>
    Failed
}
