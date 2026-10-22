using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class CarrierDetails
    {
        public string FlightNumber { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }


        public static explicit operator CarrierDetails((Carrier? Carrier, string? FlightNum) source)
        {
            var (carrier, FlightNum) = source;

            if (carrier == null && FlightNum == null) return null;

            return new CarrierDetails
            {
                FlightNumber = FlightNum,
                CarrierCode = carrier?.Code,
                CarrierName = carrier?.Name
            };
        }

        public static explicit operator CarrierDetails(DatedMarketingSegment? source)
        {
            if (source == null) return null;

            return new CarrierDetails
            {
                FlightNumber = source.FlightNum,
                CarrierCode = source.Carrier?.FirstOrDefault()?.Code,
                CarrierName = source.Carrier?.FirstOrDefault()?.Name
            };
        }

        public static explicit operator CarrierDetails((IEnumerable<Carrier> Carrier, string FlightNum) source)
        {
            var (carrier, FlightNum) = source;
            if (carrier == null && FlightNum == null) return null;

            return new CarrierDetails
            {
                FlightNumber = FlightNum,
                CarrierCode = carrier?.FirstOrDefault()?.Code,
                CarrierName = carrier?.FirstOrDefault()?.Name
            };
        }

        public static explicit operator CarrierDetails((IEnumerable<Infrastructure.Proxy.DeltaUpsellOffers.Carrier> Carrier, string FlightNum) source)
        {
            var (carrier, FlightNum) = source;
            if (carrier == null && FlightNum == null) return null;

            return new CarrierDetails
            {
                FlightNumber = FlightNum,
                CarrierCode = carrier?.FirstOrDefault()?.Code,
                CarrierName = carrier?.FirstOrDefault()?.Name
            };
        }

        public static explicit operator CarrierDetails((Infrastructure.Proxy.DeltaUpsellOffers.Carrier? Carrier, string? FlightNum) source)
        {
            var (carrier, FlightNum) = source;
            if (carrier == null && FlightNum == null) return null;

            return new CarrierDetails
            {
                FlightNumber = FlightNum,
                CarrierCode = carrier?.Code,
                CarrierName = carrier?.Name
            };
        }
    }
}
