namespace VAA.EAPI.Automation.Book.Models.OrderCreateMS
{
    public class CustomerDetails
    {
        public string custId { get; set; }
        public string ptc { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string dateOfBirth { get; set; }
        public Contacts Contacts { get; set; }
    }
}