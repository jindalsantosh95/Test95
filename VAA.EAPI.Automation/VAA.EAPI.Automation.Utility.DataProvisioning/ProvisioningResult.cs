using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Automation.Utility.DataProvisioning;

[JsonConverter(typeof(StringEnumConverter))]
public enum ProvisioningStatus
{
    [EnumMember(Value = "created")]
    Created,

    [EnumMember(Value = "no-inventory")]
    NoInventory,

    [EnumMember(Value = "seasonal-unavailable")]
    SeasonalUnavailable,

    [EnumMember(Value = "failed")]
    Failed,
}

public sealed record ProvisioningResult(
    string DataSet,
    string Scenario,
    ProvisioningStatus Status,
    string? Pnr,
    string? LeadPassengerFirstName,
    string? LeadPassengerLastName,
    string? FailedStage,
    string? Error,
    IReadOnlyList<string> StorageTables);
