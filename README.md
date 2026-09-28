# LunchSplit

A restraurant bill-splitting domain logic built with Test-Driven Development.

 ## Project structure
LunchSplit/
├── LunchSplit.Core/                  # Core class library
│   ├── Modes/
│   │   ├── TipMode.cs
│   │   └── RoundingMode.cs
│   ├── Models/
│   │   ├── Bill.cs
│   │   ├── Attendee.cs
│   │   ├── Share.cs
│   │   └── ValidationResult.cs
│   ├── Engines/
│   │   ├── Splitter.cs
│   │   └── Rounder.cs
│   ├── Helpers/
│   │   └── BillValidator.cs
│   └── Output/
│       └── ReceiptFormatter.cs
└── LunchSplit.Tests/            # MSTest unit tests
    ├── SplitterTests.cs
    ├── RounderTests.cs
    ├── BillValidatorTests.cs
    ├── ReceiptFormatterTests.cs
    └── TddPlaceholderTests.cs

  ## Prerequisites
    - .NET 8 SDK

   ## Test Summary
- **Total tests**: 27
- **Passing (Green)**: 25
- **Failing (RED TDD placeholders)**: 2
  - `Format_CsvReceiptExporter_MatchesSchema` (CSV exporter - future F5)
  - `CalculateShares_PaymentRequest_ValidatesStripeMock` (Stripe gateway - future F6)

## Core Features
- Tip calculation (None / Percent / Fixed)
- Weighted bill splitting
- Penny reconciliation algorithm
- Multiple rounding modes (Bankers / RoundUp / RoundDown / None)
- Bill validation
- Receipt formatting

## Architecture
- **SRP**: Single Responsibility Principle applied across all classes
- **Immutability**: Models implemented as C# records
- **TDD**: Arrange-Act-Assert pattern in all tests
