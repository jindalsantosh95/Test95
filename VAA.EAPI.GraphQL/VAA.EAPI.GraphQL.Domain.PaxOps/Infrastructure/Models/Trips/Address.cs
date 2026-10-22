
namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips
{
    public class Address
    {
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string PreferredFlag { get; set; }
        public string Region { get; set; }

        public static explicit operator Address(PaymentMS.Customer.Address source)
        {
            if (source == null) return null;

            return new Address
            {
                AddressLine1 = source.AddressLine1,
                City = source.City,
                PostalCode = source.PostalCode,
                Country = source.Country,
                Region = source.Region,
            };
        }
    }
}
