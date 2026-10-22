
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class Carrier
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Carrier(Domain.PassengerDetails.Carrier source)
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