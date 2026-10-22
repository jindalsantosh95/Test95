namespace VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels
{
    public class FlightSector
    {
        public string SectorId { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public string AirlineName { get; set; }
        public string OperatingAirline { get; set; }
        public string OperatingAirlineName { get; set; }
        public FlightSectorLeg Leg { get; set; }
        public Departing Departing { get; set; }
        public Arriving Arriving { get; set; }
        public DateTime DepartingAt { get; set; }
        public DateTime ArrivingAt { get; set; }
        public string CabinClass { get; set; }
        public string Pnr { get; set; }
        public string AirCraftDetail { get; set; }
        public string Duration { get; set; }
    }
}
