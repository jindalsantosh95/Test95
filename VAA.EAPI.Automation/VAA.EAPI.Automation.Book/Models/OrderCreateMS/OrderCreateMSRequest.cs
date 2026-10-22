namespace VAA.EAPI.Automation.Book.Models.OrderCreateMS
{
    public class OrderCreateMSRequest
    {
        public IEnumerable<CustomerDetails> customerDetails { get; set; }
        public Pos pos { get; set; }
        public Parties parties { get; set; }
        public IEnumerable<Selectedoffer> selectedoffers { get; set; }
        public IEnumerable<Paymentdetail> paymentdetails { get; set; }
    }
}
