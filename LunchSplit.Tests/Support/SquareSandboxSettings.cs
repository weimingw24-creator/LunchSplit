using System;
using System.IO;
using System.Text.Json;

namespace LunchSplit.Tests.Support
{
    /// <summary>
    /// Loads Square Sandbox credentials without ever hardcoding them in C#.
    /// Resolution order:
    ///   1. Environment variables (preferred for CI / `dotnet test`).
    ///   2. appsettings.local.json (git-ignored, for local secrets).
    ///   3. appsettings.json (committed placeholder file).
    /// If nothing is configured the Tier 1 tests report Inconclusive instead of
    /// failing, so a fresh clone still runs the Tier 2 suite green.
    /// </summary>
    public static class SquareSandboxSettings
    {
        private static readonly JsonDocument? ConfigDocument = LoadConfig();

        /// <summary>Sandbox access token (SQUARE_SANDBOX_ACCESS_TOKEN).</summary>
        public static string? AccessToken =>
            FirstNonEmpty(
                Environment.GetEnvironmentVariable("SQUARE_SANDBOX_ACCESS_TOKEN"),
                ReadConfig("AccessToken"));

        /// <summary>Sandbox location id (SQUARE_SANDBOX_LOCATION_ID).</summary>
        public static string? LocationId =>
            FirstNonEmpty(
                Environment.GetEnvironmentVariable("SQUARE_SANDBOX_LOCATION_ID"),
                ReadConfig("LocationId"));

        /// <summary>Optional base URL override; defaults to the sandbox host.</summary>
        public static string BaseUrl =>
            FirstNonEmpty(
                Environment.GetEnvironmentVariable("SQUARE_SANDBOX_BASE_URL"),
                ReadConfig("BaseUrl"))
            ?? "https://connect.squareupsandbox.com";

        /// <summary>True when enough credentials exist to run live Tier 1 tests.</summary>
        public static bool IsConfigured =>
            !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(LocationId);

        private static string? ReadConfig(string key)
        {
            if (ConfigDocument is null)
            {
                return null;
            }

            if (ConfigDocument.RootElement.TryGetProperty("Square", out var square) &&
                square.TryGetProperty(key, out var value))
            {
                return value.GetString();
            }

            return null;
        }

        private static JsonDocument? LoadConfig()
        {
            var baseDirectory = AppContext.BaseDirectory;
            foreach (var fileName in new[] { "appsettings.local.json", "appsettings.json" })
            {
                var path = Path.Combine(baseDirectory, fileName);
                if (File.Exists(path))
                {
                    return JsonDocument.Parse(File.ReadAllText(path));
                }
            }

            return null;
        }

        private static string? FirstNonEmpty(params string?[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate.Trim();
                }
            }

            return null;
        }
    }
}
