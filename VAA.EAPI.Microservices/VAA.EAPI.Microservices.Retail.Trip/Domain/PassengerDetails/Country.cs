using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class Country
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Country(DeltaPaxInfo.Country source)
        {
            if (source is null) return null;

            return new Country()
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}