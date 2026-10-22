using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class Email
    {
        public string Address { get; set; }

        public static explicit operator Email(DeltaPaxInfo.Email source)
        {
            if (source is null) return null;

            return new Email()
            {
                Address = source.Address
            };
        }
    }
}