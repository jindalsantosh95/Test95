namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SecureFlightInfo;

public sealed class GetSecureFlightInfoGraphQLRequest
{
    public string query { get; set; } = string.Empty;

    public SecureFlightInfoVariables variables { get; set; } = new();
}
