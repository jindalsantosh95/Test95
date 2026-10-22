using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class Carrier
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Carrier(DeltaPaxInfo.Carrier source)
        {
            if (source is null) return null;

            return new Carrier()
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}