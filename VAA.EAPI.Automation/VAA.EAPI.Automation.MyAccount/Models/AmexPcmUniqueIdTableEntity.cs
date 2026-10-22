using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Automation.MyAccount.Models
{
    public class AmexPcmUniqueIdTableEntity : TableEntity
    {

        public string UniqueId { get; set; }
        public string? FCNumber { get; set; }
        public string? UsedTimestamp { get; set; }

    }
}
