using VAA.EAPI.GraphQL.Domain.Book.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat
{
    public class SeatDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SeatNumber { get; set; }
        public SeatPrice Price { get; set; }
        public string Pos { get; set; }
        public InfantDetails LinkedInfant { get; set; }
    }
}