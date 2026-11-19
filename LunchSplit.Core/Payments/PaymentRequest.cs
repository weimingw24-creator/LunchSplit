using System;

namespace LunchSplit.Core.Payments;

/// <summary>
/// Domain-level instruction to charge one attendee's share of a lunch bill.
/// This type is deliberately vendor-neutral: it carries no Square-specific
/// fields, so the core domain never has to reference the payment SDK.
/// </summary>
/// <param name="Amount">Amount owed in major currency units (e.g. 12.34 means $12.34).</param>
/// <param name="CurrencyCode">ISO-4217 code, for example "CAD".</param>
/// <param name="IdempotencyKey">Client-generated key that makes a retry safe (no double charge).</param>
/// <param name="CardNonce">One-time token produced by the payment SDK on the client device.</param>
/// <param name="LocationId">Optional seller location override; the service default is used when null.</param>
/// <param name="ReferenceId">Optional human readable reference, typically the attendee name.</param>
public sealed record PaymentRequest(
    decimal Amount,
    string CurrencyCode,
    string IdempotencyKey,
    string CardNonce,
    string? LocationId = null,
    string? ReferenceId = null)
{
    /// <summary>
    /// Converts the decimal dollar amount into the smallest currency unit
    /// (cents) that payment gateways expect. Uses banker's rounding so that
    /// half-cents are not systematically rounded upwards.
    /// </summary>
    public long AmountInMinorUnits =>
        (long)Math.Round(Amount * 100m, MidpointRounding.ToEven);
}
