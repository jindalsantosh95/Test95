using VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Mapper
{
    public static class DeltaTripDetailsMapper
    {
        public static CarrierDetails MapOperatingCarrier(OperatingCarrier? source, string flightNum)
        {
            if (source == null)
                return null;

            return new CarrierDetails()
            {
                FlightNumber = flightNum,
                CarrierCode = source.Code,
                CarrierName = source.Name,
                OperationalSuffix = source.Code
            };
        }

        public static CarrierDetails MapToLegOperatingCarrier(Carrier? carrier, string? flightNum, string? operationalSuffix)
        {
            if (carrier == null)
                return null;

            return new CarrierDetails()
            {
                FlightNumber = flightNum,
                CarrierCode = carrier.Code,
                CarrierName = carrier.Name,
                OperationalSuffix = operationalSuffix
            };
        }
    }
}