namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
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

    }
}