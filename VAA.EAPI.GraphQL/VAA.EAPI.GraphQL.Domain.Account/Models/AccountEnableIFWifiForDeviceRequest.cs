namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount
{
    public class AccountEnableIFWifiForDeviceRequest
    {
        public string DeviceId { get; set; }
        public string SandboxId { get; set; }
        public DateTime? Expiry { get; set; }
    }
}