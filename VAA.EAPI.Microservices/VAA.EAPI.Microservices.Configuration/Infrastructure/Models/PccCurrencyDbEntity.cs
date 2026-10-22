using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class PccCurrencyDbEntity : TableEntity
    {
        public string CurrencyCode { get; set; }
        public string PCC { get; set; }
    }
}
