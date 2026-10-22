namespace VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF
{
    public class UpdateContact
    {
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string AlternativePhoneNumber { get; set; }
        public bool AcceptPost { get; set; }
        public bool AcceptTelephone { get; set; }
        public bool AcceptEmail { get; set; }
        public bool AcceptTextMessage { get; set; }
        public string CountryCode { get; set; }
        public string PeopleId { get; set; }

    }
}
