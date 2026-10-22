using Newtonsoft.Json.Linq;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Domain.Virgin.Models.Payment
{
    public class FinancialInstitution
    {
        public string AccountHolderName { get; set; }
        public string ProviderId { get; set; }
        public string ReturnUrl { get; set; }
        public Address Address { get; set; }

        public static explicit operator FinancialInstitution(JObject source)
        {
            return new FinancialInstitution()
            {
                AccountHolderName = source["AccountHolderName"]?.ToString(),
                ProviderId = source["ProviderId"]?.ToString(),
                ReturnUrl = source["ReturnUrl"]?.ToString(),
                Address = source["Address"] != null ? (Address)source["Address"] : null
            };
        }
    }
}
