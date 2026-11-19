using System;

namespace LunchSplit.Core.Payments;

/// <summary>
/// Raised when the gateway cannot be reached or does not answer inside the
/// configured timeout. Keeping this as a domain exception (instead of leaking
/// HttpRequestException) lets the UI show a "try again" message without
/// knowing which vendor is behind the interface.
/// </summary>
public class PaymentGatewayTimeoutException : Exception
{
    public PaymentGatewayTimeoutException(string message)
        : base(message)
    {
    }

    public PaymentGatewayTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
