using Newtonsoft.Json.Linq;
using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class CruiseDetails
    {
        public string Name { get; set; }
        public bool VipBooking { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ShipDetails Ship { get; set; }
        public DestinationDetails CruiseDestination { get; set; }
        public DeparturePortDetails DeparturePort { get; set; }
        public ArrivalPortDeatils ArrivalPort { get; set; }
        public ItineraryDetails Itinerary { get; set; }
        public CruiseLineDetails CruiseLine { get; set; }

        public static explicit operator CruiseDetails(JToken source)
        {
            if (source == null) return null;

            return new CruiseDetails()
            {
                Name = $"{source["summary"]?["name"]?.Value<string>()} - {source["summary"]?["highlight"]?.Value<string>()}",
                VipBooking = source["summary"]?["vipBooking"]?.Value<bool>() ?? false,
                StartDate = source["summary"]?["startTime"]?.Value<DateTime>(),
                EndDate = source["summary"]?["endTime"]?.Value<DateTime>(),
                Itinerary = (ItineraryDetails)source["detail"]?["itinerary"],
                ArrivalPort = (ArrivalPortDeatils)source["detail"]?["arrivalPort"],
                DeparturePort = (DeparturePortDetails)source["detail"],
                CruiseDestination = (DestinationDetails)source["detail"]?["cruiseDestination"],
                CruiseLine = (CruiseLineDetails)source["detail"]?["cruiseLine"]
            };
        }
    }
}