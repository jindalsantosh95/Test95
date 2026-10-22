using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class BasketSectorDetails
    {
        public string SectorId { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public string AirlineName { get; set; }
        public string OperatingAirline { get; set; }
        public string OperatingAirlineName { get; set; }
        public BasketSectorLeg Leg { get; set; }
        public BasketAirportDetails Departing { get; set; }
        public BasketAirportDetails Arriving { get; set; }
        public LocalDateTime DepartingAt { get; set; }
        public LocalDateTime ArrivingAt { get; set; }
        public string CabinClass { get; set; }
        public string Pnr { get; set; }
        public string? AirCraftDetail { get; set; }
        public string? Duration { get; set; }
    }
}
