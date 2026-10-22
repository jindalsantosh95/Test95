using VAA.EAPI.Automation.Core.TestDataConfig;

namespace VAA.EAPI.Automation.Book.Provisioning.Pnr
{
    public sealed record CreatedPnr(
        string PnrCode,
        string BasketId,
        string ScenarioName,
        DateTime CreatedAtUtc,
        string LeadPassengerFirstName,
        string LeadPassengerLastName)
    {
        // Carries the populated TestConfig so sinks (same assembly) can hand it
        // to existing WriteToAzureStorage methods that still take TestConfig.
        // Promotion to public is a follow-up once sinks no longer need it.
        internal TestConfig Config { get; init; } = default!;
    }
}
