using VAA.EAPI.Automation.Core.Extensions;
using Xunit;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Support;

/// <summary>
/// Reads the Reward Flight Finder target and scenario values supplied by the local settings file or CI variables.
/// </summary>
internal sealed record RewardFlightFinderEnvironment(
    string BaseUrl,
    string ExpectedHost,
    string Origin,
    string Destination,
    string? Month,
    string? NoAvailabilityOrigin,
    string? NoAvailabilityDestination,
    string? NoAvailabilityMonth)
{
    public static RewardFlightFinderEnvironment ReadHappyPathOrSkip() => ReadBaseOrSkip();

    public static RewardFlightFinderEnvironment ReadNoAvailabilityOrSkip()
    {
        RewardFlightFinderEnvironment environment = ReadBaseOrSkip();
        return environment with
        {
            NoAvailabilityOrigin = ReadRequiredOrSkip("RFF_NO_AVAILABILITY_ORIGIN"),
            NoAvailabilityDestination = ReadRequiredOrSkip("RFF_NO_AVAILABILITY_DESTINATION"),
            NoAvailabilityMonth = Environment.GetEnvironmentVariable("RFF_NO_AVAILABILITY_MONTH")
        };
    }

    private static RewardFlightFinderEnvironment ReadBaseOrSkip()
    {
        string baseUrl = ReadRequiredOrSkip("SAB_BASE_URL");
        string expectedHost = ReadRequiredOrSkip("RFF_EXPECTED_HOST");
        Uri baseUri = new(baseUrl);
        Skip.If(
            !baseUri.Host.Is(expectedHost),
            $"Reward Flight Finder is configured for {expectedHost}, but SAB_BASE_URL targets {baseUri.Host}.");

        return new RewardFlightFinderEnvironment(
            baseUrl,
            expectedHost,
            ReadRequiredOrSkip("RFF_ORIGIN"),
            ReadRequiredOrSkip("RFF_DESTINATION"),
            Environment.GetEnvironmentVariable("RFF_MONTH"),
            null,
            null,
            null);
    }

    private static string ReadRequiredOrSkip(string variableName)
    {
        string? value = Environment.GetEnvironmentVariable(variableName);
        Skip.If(value.IsNullOrWhiteSpace(), $"Set {variableName} to run Reward Flight Finder journeys.");
        return value!;
    }
}
