namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class Tracking
    {
        public int BookingReference { get; set; }
        public string EmailAddress { get; set; }
        public bool AcceptPost { get; set; }
        public bool AcceptTelephone { get; set; }
        public bool AcceptEmail { get; set; }
        public bool AcceptTextMessage { get; set; }
    }
}
