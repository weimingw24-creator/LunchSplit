using System.Threading;
using System.Threading.Tasks;

namespace LunchSplit.Core.Payments;

/// <summary>
/// The single seam between the LunchSplit domain and any payment vendor.
/// Because every collaborator depends on this interface (and never on the
/// Square SDK), the domain can be exercised in memory with Moq.
/// </summary>
public interface IPaymentProcessor
{
    /// <summary>
    /// Charges one attendee's share. Implementations must translate a network
    /// failure into <see cref="PaymentGatewayTimeoutException"/> and a gateway
    /// rejection into a failed <see cref="PaymentResult"/>.
    /// </summary>
    Task<PaymentResult> ProcessPaymentAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the current state of an existing transaction (GET /v2/payments/{id}).
    /// </summary>
    Task<PaymentStatus> GetTransactionStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default);
}
