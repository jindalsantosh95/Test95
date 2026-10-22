using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Aircraft
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string SubfleetCode { get; set; }

        public static explicit operator Aircraft(TransportEquipment? source)
        {
            if (source == null) return null;
            return new Aircraft
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}
