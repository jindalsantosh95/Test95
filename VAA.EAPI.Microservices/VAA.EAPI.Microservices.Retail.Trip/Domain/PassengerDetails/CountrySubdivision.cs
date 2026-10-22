using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class CountrySubdivision
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator CountrySubdivision(DeltaPaxInfo.CountrySubdivision source)
        {
            if (source is null) return null;

            return new CountrySubdivision()
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}