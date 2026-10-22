
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS.Payment
{
    public class Contacts
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Countrycode { get; set; }
        public string PhoneAreaNumber { get; set; }
        public string PhoneType { get; set; }
        public string CountryDailingCode { get; set; }

        public static explicit operator Contacts(PaymentMS.Customer.Contacts source)
        {
            if (source == null) return null;

            return new()
            {
                Email = source.Email,
                Phone = source.Phone,
                Countrycode = source.Countrycode
            };
        }
    }
}