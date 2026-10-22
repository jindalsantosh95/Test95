namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class SearchFilterCriteria
    {
        public BookingReferenceCriteria bookingReferenceCriteria { get; set; }
        public AccountableDocumentFilterCriteria accountableDocumentFilterCriteria { get; set; }
        public OrderReferenceCriteria OrderReferenceCriteria { get; set; }
    }
}