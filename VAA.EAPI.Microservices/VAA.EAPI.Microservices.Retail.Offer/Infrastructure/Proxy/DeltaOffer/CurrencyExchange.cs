using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class CurrencyExchange
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ApplicableToChangeTextEnum
        {
            [EnumMember(Value = "FARES")]
            FARES = 1,
            [EnumMember(Value = "TAXES")]
            TAXES = 2,
            [EnumMember(Value = "BOTH")]
            BOTH = 3
        }
        public ApplicableToChangeTextEnum ApplicableToChangeText { get; set; }
        public double CurrencyExchangeRate { get; set; }
        public Currency FromCurrency { get; set; }
        public bool HistoricalRate { get; set; }
        public Currency ToCurrency { get; set; }
        public string CurrencyExchangeTypeCode { get; set; }
    }
}
