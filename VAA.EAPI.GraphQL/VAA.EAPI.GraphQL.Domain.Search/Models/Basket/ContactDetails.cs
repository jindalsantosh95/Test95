namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public sealed class ContactDetails
    {
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string AlternativePhoneNumber { get; set; }
        public bool AcceptPost { get; set; }
        public bool AcceptTelephone { get; set; }
        public bool AcceptEmail { get; set; }
        public bool AcceptTextMessage { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
    }
}