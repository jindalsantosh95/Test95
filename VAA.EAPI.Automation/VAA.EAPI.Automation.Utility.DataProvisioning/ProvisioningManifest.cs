using VAA.EAPI.Automation.Book.Provisioning.Pnr;
using VAA.EAPI.Automation.Book.Provisioning.Pnr.Sinks;

namespace VAA.EAPI.Automation.Utility.DataProvisioning;

public sealed record ProvisioningManifest
{
    public int MaxConcurrency { get; init; } = 2;

    public CardPaymentAutomationMode CardPaymentMode { get; init; } = CardPaymentAutomationMode.Api;

    public IReadOnlyList<PnrScenario> Scenarios { get; init; } = Array.Empty<PnrScenario>();

    public IReadOnlyList<ProvisioningDataSet> DataSets { get; init; } = Array.Empty<ProvisioningDataSet>();

    public bool ShouldSerializeScenarios() => Scenarios.Count > 0;

    public bool ShouldSerializeDataSets() => DataSets.Count > 0;
}

public sealed record ProvisioningDataSet
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<PnrScenario> Scenarios { get; init; } = Array.Empty<PnrScenario>();

    public IReadOnlyList<TableOutput> TableOutputs { get; init; } = Array.Empty<TableOutput>();
}

public sealed record TableOutput
{
    public PnrTableSchema? Schema { get; init; }

    public string TableName { get; init; } = string.Empty;
}
