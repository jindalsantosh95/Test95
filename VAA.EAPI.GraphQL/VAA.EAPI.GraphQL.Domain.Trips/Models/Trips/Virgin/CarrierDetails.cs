
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class CarrierDetails
    {
        public string? FlightNumber { get; set; }
        public string? CarrierCode { get; set; }
        public string? CarrierName { get; set; }
        public string? OperationalSuffix { get; set; }

        public static explicit operator CarrierDetails((Infrastructure.Models.TripMS.CarrierDetails msCarrierDetail, IEnumerable<AirlineRefData> airlineRefData) source)
        {
            var (msCarrierDetail, airlineRefData) = source;
            if (msCarrierDetail == null) return null;

            return new CarrierDetails()
            {
                CarrierCode = msCarrierDetail.CarrierCode,
                CarrierName = !airlineRefData.IsNullOrEmpty() && airlineRefData.Any(a => a.CarrierCode == msCarrierDetail.CarrierCode) ? airlineRefData.FirstOrDefault(a => a.CarrierCode == msCarrierDetail.CarrierCode)?.CarrierName : msCarrierDetail.CarrierName,
                FlightNumber = msCarrierDetail.FlightNumber,
                OperationalSuffix = msCarrierDetail.OperationalSuffix
            };
        }

        public static explicit operator CarrierDetails(Infrastructure.Models.HolidaysMS.TripsCore.CarrierDetails source)
        {
            if (source == null)
                return null;

            return new CarrierDetails()
            {
                CarrierCode = source.CarrierCode,
                CarrierName = source.CarrierName,
                FlightNumber = source.FlightNumber,
                OperationalSuffix = source.OperationalSuffix
            };
        }

        public static explicit operator CarrierDetails((Infrastructure.Models.HolidaysMS.TripsCore.CarrierDetails carrier, IEnumerable<AirlineRefData> airlineRefData) source)
        {
            var (carrier, airlineRefData) = source;

            if (carrier == null)
                return null;

            return new CarrierDetails()
            {
                CarrierCode = carrier.CarrierCode,
                CarrierName = !airlineRefData.IsNullOrEmpty()
                    ? airlineRefData.FirstOrDefault(a => a.CarrierCode.Is(carrier.CarrierCode))?.CarrierName ?? carrier.CarrierName
                    : carrier.CarrierName,
                FlightNumber = carrier.FlightNumber,
                OperationalSuffix = carrier.OperationalSuffix
            };
        }
    }
}
