using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class RateInfo
    {
        public decimal? Price { get; set; }
        public decimal? PriceWithTax { get; set; }
        public decimal? Tax { get; set; }
        public decimal? BasePrice { get; set; }
        public string? CurrencyInContract { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal? TotalTaxBase { get; set; }

        public static explicit operator RateInfo(JToken source)
        {
            if (source == null)
                return null;

            return new()
            {
                BasePrice = source["basePrice"]?.Value<decimal>(),
                CurrencyInContract = source["currencyInContract"]?.Value<string>(),
                TotalTax = source["totalTax"]?.Value<decimal>(),
                TotalTaxBase = source["totalTaxBase"]?.Value<decimal>(),
                Price = source["price"]?.Value<decimal>(),
                PriceWithTax = source["priceWithTax"]?.Value<decimal>(),
                Tax = source["tax"]?.Value<decimal>(),
            };
        }
    }
}
