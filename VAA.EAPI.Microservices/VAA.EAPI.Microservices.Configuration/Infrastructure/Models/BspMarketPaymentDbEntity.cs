using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class BspMarketPaymentDbEntity : TableEntity
    {
        public bool Amex { get; set; }
        public string BSPMarket { get; set; }
        public string CountryCode { get; set; }
        public bool Diners { get; set; }
        public bool Discover { get; set; }
        public bool IsCreditCardAccepted { get; set; }
        public bool IsCurrencySetUpForCCAcquirer { get; set; }
        public bool Maestro { get; set; }
        public bool Mastercard { get; set; }
        public bool UATP { get; set; }
        public bool Visa { get; set; }
    }
}
