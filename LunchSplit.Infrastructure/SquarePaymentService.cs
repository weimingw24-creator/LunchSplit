using LunchSplit.Core.Payments;
using Square;
using Square.Payments;

namespace LunchSplit.Infrastructure;

/// <summary>
/// Infrastructure adapter that wraps the official Square C# SDK and translates
/// between LunchSplit domain DTOs and Square API payloads.
///
/// This is the only file in the solution that knows Square exists. Everything
/// above it depends on <see cref="IPaymentProcessor"/> instead, which keeps the
/// domain testable without a network connection.
/// </summary>
public sealed class SquarePaymentService : IPaymentProcessor
{
    /// <summary>Default timeout for every gateway call, so scripted runs never hang.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    private readonly SquareClient _client;
    private readonly string _locationId;
    private readonly TimeSpan _timeout;

    /// <param name="accessToken">Sandbox access token (loaded from config, never hardcoded).</param>
    /// <param name="locationId">Sandbox location id that receives the payment.</param>
    /// <param name="baseUrl">Gateway base URL; sandbox by default.</param>
    /// <param name="timeout">Per-request timeout, five seconds by default.</param>
    public SquarePaymentService(
        string accessToken,
        string locationId,
        string baseUrl = SquareEnvironment.Sandbox,
        TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("A Square access token is required.", nameof(accessToken));
        }

        if (string.IsNullOrWhiteSpace(locationId))
        {
            throw new ArgumentException("A Square location id is required.", nameof(locationId));
        }

        _locationId = locationId;
        _timeout = timeout ?? DefaultTimeout;

        // Explicit HttpClient timeout: protects automated `dotnet test` runs
        // from hanging when the socket silently drops.
        var httpClient = new HttpClient { Timeout = _timeout };

        var options = new ClientOptions
        {
            BaseUrl = baseUrl,
            Timeout = _timeout,
            HttpClient = httpClient,
            MaxRetries = 0 // idempotency is handled explicitly, retries would mask failures
        };

        _client = new SquareClient(accessToken, options);
    }

    

    /// <inheritdoc />
    public async Task<PaymentResult> ProcessPaymentAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var body = new CreatePaymentRequest
        {
            SourceId = request.CardNonce,
            IdempotencyKey = request.IdempotencyKey,
            AmountMoney = new Money
            {
                Amount = request.AmountInMinorUnits,
                Currency = ToSquareCurrency(request.CurrencyCode)
            },
            LocationId = request.LocationId ?? _locationId,
            ReferenceId = request.ReferenceId,
            // Autocomplete captures immediately, which makes a sandbox
            // transaction reach COMPLETED without a second API call.
            Autocomplete = true,
            Note = $"LunchSplit share {request.Amount:0.00} {request.CurrencyCode}"
        };

        try
        {
            var response = await _client.Payments
                .CreateAsync(body, new RequestOptions { Timeout = _timeout }, cancellationToken);

            var payment = response.Payment;
            if (payment?.Id is null)
            {
                return PaymentResult.Failure("EMPTY_RESPONSE", "Gateway returned no payment object.", 200);
            }

            return PaymentResult.Success(payment.Id);
        }
        catch (SquareApiException ex)
        {
            // Declines and validation failures are business outcomes, not crashes.
            return PaymentResult.Failure(
                ExtractErrorCode(ex),
                ex.Message,
                ex.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            throw new PaymentGatewayTimeoutException("Square sandbox is unreachable.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PaymentGatewayTimeoutException("Square sandbox call timed out.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<PaymentStatus> GetTransactionStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new ArgumentException("A transaction id is required.", nameof(transactionId));
        }

        try
        {
            var response = await _client.Payments
                .GetAsync(
                    new GetPaymentsRequest { PaymentId = transactionId },
                    new RequestOptions { Timeout = _timeout },
                    cancellationToken)
                .ConfigureAwait(false);

            return MapStatus(response.Payment?.Status);
        }
        catch (SquareApiException ex)
        {
            if (ex.StatusCode == 404)
            {
                return PaymentStatus.Unknown;
            }

            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new PaymentGatewayTimeoutException("Square sandbox is unreachable.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PaymentGatewayTimeoutException("Square sandbox call timed out.", ex);
        }
    }

    /// <summary>Maps a Square payment status string onto the domain enum.</summary>
    private static PaymentStatus MapStatus(string? status) => status switch
    {
        "COMPLETED" => PaymentStatus.Completed,
        "APPROVED" => PaymentStatus.Approved,
        "CANCELED" or "FAILED" => PaymentStatus.Failed,
        _ => PaymentStatus.Unknown
    };

    /// <summary>Returns the first vendor error code, or a generic fallback.</summary>
    private static string ExtractErrorCode(SquareApiException ex)
    {
        var firstError = ex.Errors is { Count: > 0 } ? ex.Errors[0] : null;
        return firstError is null ? "GATEWAY_ERROR" : firstError.Code.ToString();
    }

    /// <summary>Maps an ISO-4217 code onto the SDK's currency enum.</summary>
    private static Currency ToSquareCurrency(string currencyCode) =>
        currencyCode?.Trim().ToUpperInvariant() switch
        {
            "CAD" => Currency.Cad,
            "USD" => Currency.Usd,
            "EUR" => Currency.Eur,
            "GBP" => Currency.Gbp,
            _ => Currency.FromCustom(currencyCode?.Trim().ToUpperInvariant() ?? "CAD")
        };
}
