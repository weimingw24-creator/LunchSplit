using System;

namespace LunchSplit.Core.Payments;

/// <summary>
/// Outcome of a single payment attempt, expressed in domain terms rather than
/// in vendor SDK types. Every failure path (decline, validation error, HTTP
/// error) is normalised into this record so callers never need to catch SDK
/// exceptions to know what happened.
/// </summary>
/// <param name="IsSuccess">True only when the gateway authorised the charge.</param>
/// <param name="TransactionId">Gateway transaction id; empty when the charge failed.</param>
/// <param name="ErrorCode">Vendor error code such as "CARD_DECLINED"; null on success.</param>
/// <param name="ErrorMessage">Human readable detail suitable for logs.</param>
/// <param name="HttpStatusCode">HTTP status returned by the gateway (200 on success).</param>
public sealed record PaymentResult(
    bool IsSuccess,
    string TransactionId,
    string? ErrorCode,
    string? ErrorMessage,
    int HttpStatusCode)
{
    /// <summary>Builds a successful result around the gateway transaction id.</summary>
    public static PaymentResult Success(string transactionId) =>
        new(true, transactionId, null, null, 200);

    /// <summary>Builds a failed result carrying the vendor error code and HTTP status.</summary>
    public static PaymentResult Failure(string? errorCode, string? errorMessage, int httpStatusCode) =>
        new(false, string.Empty, errorCode, errorMessage, httpStatusCode);
}
