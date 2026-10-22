using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightOrderNotification
{
    public class Airport
    {
        public string TerminalName { get; set; }
        public string Code { get; set; }
        public string AirportName { get; set; }

        public static explicit operator Airport(string airportCode)
        {
            return new Airport { Code = airportCode };
        }
    }
}
