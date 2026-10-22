namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SecureFlightInfo;

public sealed class UpdateSecureFlightInfoGraphQLRequest
{
    public string query { get; set; } = string.Empty;

    public UpdateSecureFlightInfoVariables variables { get; set; } = new();
}
