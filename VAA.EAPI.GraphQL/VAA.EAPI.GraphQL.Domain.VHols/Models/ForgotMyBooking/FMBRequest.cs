namespace VAA.EAPI.GraphQL.Domain.VHols.Models.ForgotMyBooking
{
    public class FMBRequest
    {
        public string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DepartureDate { get; set; }
    }
}
