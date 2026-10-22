using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class CurrencyAmount
    {
        public string MonetaryAmount { get; set; }
        public string BaseCurrency { get; set; }
        public string EquivalentMonetaryAmount { get; set; }
        public string EquivalentBaseCurrency { get; set; }
        public string CurrencyId { get; set; }
        public int DecimalPrecisionCount { get; set; }
        public decimal PriceDifference { get; set; }
        public string PriceDifferenceSign { get; set; }
        public string BalanceAmount { get; set; }

        public static explicit operator CurrencyAmount(JObject source)
        {
            return new CurrencyAmount()
            {
                MonetaryAmount = source["MonetaryAmount"]?.ToString(),
                BaseCurrency = source["BaseCurrency"]?.ToString(),
                BalanceAmount = source["BalanceAmount"]?.ToString(),
                EquivalentMonetaryAmount = source["EquivalentMonetaryAmount"]?.ToString(),
                EquivalentBaseCurrency = source["EquivalentBaseCurrency"]?.ToString(),
                CurrencyId = source["CurrencyId"]?.ToString(),
                DecimalPrecisionCount = source["DecimalPrecisionCount"] != null ? (int)source["DecimalPrecisionCount"] : 0,
                PriceDifference = source["PriceDifference"] != null ? (decimal)source["PriceDifference"] : 0,
                PriceDifferenceSign = source["PriceDifferenceSign"]?.ToString()
            };
        }
    }
}
