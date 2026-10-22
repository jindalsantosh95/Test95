namespace VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.ContactDetailsBasket
{
    public class ContactDetails
    {
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string alternativePhoneNumber { get; set; }
        public bool acceptPost { get; set; }
        public bool acceptTelephone { get; set; }
        public bool acceptEmail { get; set; }
        public bool acceptTextMessage { get; set; }
        public string validation { get; set; }
        public string countryCode { get; set; }
        public string peopleId { get; set; }
    }
}
