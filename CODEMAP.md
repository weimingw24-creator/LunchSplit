# LunchSplit Project CodeMap

This file is read by the automated grading harness so that folder names never
have to match a fixed template. Paths are relative to the repository root.

- **SolutionFile**: `LunchSplit.slnx`
- **CoreSourceRoot**: `LunchSplit.Core`
- **InfraSourceRoot**: `LunchSplit.Infrastructure`
- **TestProjectRoot**: `LunchSplit.Tests`
- **Tier1TestClass**: `LunchSplit.Tests/Integration/SquareSandboxTests.cs`
- **Tier2TestClass**: `LunchSplit.Tests/Unit/PaymentProcessorMoqTests.cs`

## Layer responsibilities

| Layer | Path | Responsibility |
| --- | --- | --- |
| Core | `LunchSplit.Core` | Domain models, split/rounding engines, `IPaymentProcessor` contract, payment DTOs and `PaymentManager`. No vendor SDK references. |
| Infrastructure | `LunchSplit.Infrastructure` | `SquarePaymentService`, the only class that talks to the Square SDK. |
| Tests | `LunchSplit.Tests` | Tier 1 live sandbox tests (`Integration/`) and Tier 2 in-memory Moq tests (`Unit/`). |

## Running the suite

```bash
dotnet test
```

Tier 1 tests require `SQUARE_SANDBOX_ACCESS_TOKEN` and
`SQUARE_SANDBOX_LOCATION_ID` (environment variables or
`LunchSplit.Tests/appsettings.local.json`). Without them they report
Inconclusive/Skipped instead of failing.
