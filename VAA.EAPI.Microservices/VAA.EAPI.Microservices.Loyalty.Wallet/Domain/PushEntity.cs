using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain
{
    public class PushEntity : TableEntity
    {        
        public string SerialNumber { get; set; }
       
    }
}
