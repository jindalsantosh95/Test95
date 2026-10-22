using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class Contacts
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Countrycode { get; set; }
        public string PhoneAreaNumber { get; set; }
        public string PhoneType { get; set; }
        public string CountryDailingCode { get; set; }
        public bool ContactRefusedInd { get; set; }

        public static explicit operator Contacts(JObject source)
        {
            return new Contacts()
            {
                Email = source["Email"]?.ToString(),
                Phone = source["Phone"]?.ToString(),
                Countrycode = source["Countrycode"]?.ToString(),
                PhoneAreaNumber = source["PhoneAreaNumber"]?.ToString(),
                CountryDailingCode = source["CountryDailingCode"]?.ToString(),
                ContactRefusedInd = source.Value<bool?>("ContactRefusedInd") ?? false
            };
        }
    }
}
