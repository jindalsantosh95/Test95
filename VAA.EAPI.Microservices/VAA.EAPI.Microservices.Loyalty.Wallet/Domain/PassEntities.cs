using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain
{
    public class PassEntities : TableEntity
    {
        public string PassTypeId { get; set; }
        public string SerialNumber { get; set; }
        public string PushToken { get; set; }
        public string LastUpdated { get; set; }
        public string DeviceId { get; set; }

    }
}
