
using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Infrastructure.Entity
{
    public class ServiceResponseDatabaseEntity : TableEntity
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string ResponseData { get; set; }
        public string Type { get; set; }
        public string Error { get; set; }

    }

}
