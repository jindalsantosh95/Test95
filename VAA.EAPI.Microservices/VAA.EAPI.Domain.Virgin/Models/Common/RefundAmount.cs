using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class RefundAmount
    {
        public string MonetaryAmount { get; set; }
        public string BaseCurrency { get; set; }
        public string CurrencyId { get; set; }

        public static explicit operator RefundAmount(JObject source)
        {
            return new RefundAmount()
            {
                MonetaryAmount = source["MonetaryAmount"]?.ToString(),
                BaseCurrency = source["BaseCurrency"]?.ToString(),
                CurrencyId = source["CurrencyId"]?.ToString(),
            };
        }
    }
}
