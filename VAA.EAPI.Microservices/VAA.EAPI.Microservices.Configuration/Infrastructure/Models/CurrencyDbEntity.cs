using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class CurrencyDbEntity : TableEntity
    {
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Currency { get; set; }
        public string LocalCurrency { get; set; }
        public string POS { get; set; }
        public string OverrideCurrency { get; set; }
        public string OverridePos { get; set; }
    }
}
