
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class Email
    {
        public string Address { get; set; }

        public static explicit operator Email(Domain.PassengerDetails.Email source)
        {
            if (source is null) return null;

            return new Email()
            {
                Address = source.Address
            };
        }
    }
}