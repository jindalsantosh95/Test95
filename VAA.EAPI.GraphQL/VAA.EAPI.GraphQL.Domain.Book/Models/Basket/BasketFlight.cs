using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class BasketFlight
    {
        public string AirlineName { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public BasketAirportDetails Departing { get; set; }
        public BasketAirportDetails Arriving { get; set; }
        public LocalDateTime DepartingAt { get; set; }
        public LocalDateTime ArrivingAt { get; set; }
        public string CabinClass { get; set; }
        public IEnumerable<BasketSectorDetails> Sectors { get; set; }
    }
}
