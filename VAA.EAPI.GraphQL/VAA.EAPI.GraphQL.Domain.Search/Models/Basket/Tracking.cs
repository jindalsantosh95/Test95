namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class Tracking
    {
        public string BookingReference { get; set; }
        public string EmailAddress { get; set; }
        public bool AcceptPost { get; set; }
        public bool AcceptTelephone { get; set; }
        public bool AcceptEmail { get; set; }
        public bool AcceptTextMessage { get; set; }
    }
}