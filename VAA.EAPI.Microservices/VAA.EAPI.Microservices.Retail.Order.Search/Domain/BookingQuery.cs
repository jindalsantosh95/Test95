namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class BookingQuery
    {
        public string FCNumber { get; set; }
        public string PhoneNo { get; set; }
        public string Page { get; set; }
        public string PageSize { get; set; }

        public BookingQuery(string fcNumber, string phoneNumber, string page = "1", string pageSize = "10")
        {
            FCNumber = fcNumber;
            PhoneNo = phoneNumber;
            Page = page;
            PageSize = pageSize;
        }

        public BookingQuery(string fcNumber, string phoneNumber)
        {
            FCNumber = fcNumber;
            PhoneNo = phoneNumber;
        }
    }
}
