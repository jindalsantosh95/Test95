namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class Tracking
    {
        public object BookingReference { get; set; }
        public object EmailAddress { get; set; }
        public bool AcceptPost { get; set; }
        public bool AcceptTelephone { get; set; }
        public bool AcceptEmail { get; set; }
        public bool AcceptTextMessage { get; set; }
    }
}
