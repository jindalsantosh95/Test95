namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Models
{
    public class FindMyBookingPassenger
    {
        public string PassengerRef { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}