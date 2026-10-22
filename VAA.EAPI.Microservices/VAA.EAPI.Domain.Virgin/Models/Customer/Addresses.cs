using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Customer
{
    public class Address
    {
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string DepartureCity { get; set; }

        public static explicit operator Address(JObject source)
        {
            return new Address()
            {
                AddressType = source["AddressType"]?.ToString(),
                AddressLine1 = source["AddressLine1"]?.ToString(),
                AddressLine2 = source["AddressLine2"]?.ToString(),
                AddressLine3 = source["AddressLine3"]?.ToString(),
                City = source["City"]?.ToString(),
                PostalCode = source["PostalCode"]?.ToString(),
                Region = source["Region"]?.ToString(),
                Country = source["Country"]?.ToString(),
                DepartureCity = source["DepartureCity"]?.ToString()
            };
        }
    }
}
