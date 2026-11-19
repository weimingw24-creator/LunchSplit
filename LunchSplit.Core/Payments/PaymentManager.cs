using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LunchSplit.Core.Engines;
using LunchSplit.Core.Models;

namespace LunchSplit.Core.Payments;

/// <summary>
/// Application service that turns a split bill into one payment per included
/// attendee. It depends only on <see cref="IPaymentProcessor"/>, which is what
/// makes the whole class testable in memory.
/// </summary>
public sealed class PaymentManager
{
    private readonly IPaymentProcessor _processor;
    private readonly Splitter _splitter;
    private readonly Func<string, string> _idempotencyKeyFactory;

    /// <param name="processor">The gateway abstraction (real service or a Moq proxy).</param>
    /// <param name="splitter">Split engine; a default instance is used when omitted.</param>
    /// <param name="idempotencyKeyFactory">
    /// Produces one idempotency key per attendee. Injectable so tests can keep
    /// the keys deterministic; defaults to a GUID-based key.
    /// </param>
    public PaymentManager(
        IPaymentProcessor processor,
        Splitter? splitter = null,
        Func<string, string>? idempotencyKeyFactory = null)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _splitter = splitter ?? new Splitter();
        _idempotencyKeyFactory = idempotencyKeyFactory ?? defaultKeyFactory;

        static string defaultKeyFactory(string attendeeName) =>
            $"lunchsplit-{attendeeName}-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Charges a single request. Network failures are translated into
    /// <see cref="PaymentGatewayTimeoutException"/>; gateway rejections are
    /// returned as a failed result rather than thrown.
    /// </summary>
    public async Task<PaymentResult> ProcessPaymentAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _processor
                .ProcessPaymentAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new PaymentGatewayTimeoutException(
                $"Payment gateway did not respond for attendee '{request.ReferenceId}'.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient raises TaskCanceledException when its own timeout fires.
            throw new PaymentGatewayTimeoutException(
                $"Payment gateway timed out for attendee '{request.ReferenceId}'.", ex);
        }
    }

    /// <summary>
    /// Splits the bill and charges every included attendee in turn.
    /// A rejected charge is recorded and the batch keeps going, so one declined
    /// card cannot block the rest of the table. A zero-total bill short-circuits
    /// before the processor is ever touched.
    /// </summary>
    public async Task<PaymentBatchSummary> ProcessBillAsync(
        Bill bill,
        IEnumerable<Attendee> attendees,
        RoundingMode roundingMode,
        string cardNonce,
        string currencyCode = "CAD",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bill);
        ArgumentNullException.ThrowIfNull(attendees);

        var attendeeList = attendees.ToList();
        var shares = _splitter.CalculateShare(bill, attendeeList, roundingMode);
        var summary = new PaymentBatchSummary();

        foreach (var share in shares)
        {
            // A $0.00 share (excluded attendee, or a zero-total bill) must not
            // reach the gateway: there is nothing to authorise.
            if (share.Amount <= 0m)
            {
                continue;
            }

            var request = new PaymentRequest(
                Amount: share.Amount,
                CurrencyCode: currencyCode,
                IdempotencyKey: _idempotencyKeyFactory(share.Name),
                CardNonce: cardNonce,
                ReferenceId: share.Name);

            var result = await ProcessPaymentAsync(request, cancellationToken)
                .ConfigureAwait(false);

            summary.Add(share.Name, result);
        }

        return summary;
    }
}
