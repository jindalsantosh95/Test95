namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SecureFlightInfo;

public sealed class SecureFlightInfoRequestInput
{
    public string pnrLocator { get; set; } = string.Empty;

    public string givenNames { get; set; } = string.Empty;

    public string surname { get; set; } = string.Empty;

    public string passengerFirstNin { get; set; } = string.Empty;

    public string passengerLastNin { get; set; } = string.Empty;
}
