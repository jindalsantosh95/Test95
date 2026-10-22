namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class PassengerInformationRequest
    {
        public RequestOptionCodeType Using { get; set; }
        public string ConfirmationNum { get; set; }
        public string EncryptedConfirmationNum { get; set; }
        public string TicketNum { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
    }
}