namespace VAA.EAPI.Automation.MyAccount.Models
{

    public class AzureB2CResponse
    {
        [Newtonsoft.Json.JsonProperty("@odata.context")]
        public string odatacontext { get; set; }

        [Newtonsoft.Json.JsonProperty("value")]
        public AzureB2CValue[] azureB2CValues { get; set; }
    }

    public class AzureB2CValue
    {
        public string givenName { get; set; }
        public string surname { get; set; }
        public string id { get; set; }
        public string passwordPolicies { get; set; }
        public string userPrincipalName { get; set; }
        public bool accountEnabled { get; set; }
        public AzureB2CIdentity[] identities { get; set; }
    }

    public class AzureB2CIdentity
    {
        public string signInType { get; set; }
        public string issuer { get; set; }
        public string issuerAssignedId { get; set; }
    }
}
