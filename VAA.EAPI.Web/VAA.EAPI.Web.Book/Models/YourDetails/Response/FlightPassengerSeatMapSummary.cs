using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class FlightPassengerSeatMapSummary
    {
        public string PassengerId { get; set; }
        public string PassengerFirstName { get; set; }
        public string PassengerLastName { get; set; }
        public IEnumerable<SeatDetails> SeatDetails { get; set; }
        public LinkedInfantDetails LinkedInfant { get; set; }
    }
}
