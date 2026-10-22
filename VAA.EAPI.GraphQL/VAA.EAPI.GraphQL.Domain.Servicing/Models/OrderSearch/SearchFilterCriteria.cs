namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class SearchFilterCriteria
    {
        public BookingReferenceCriteria? bookingReferenceCriteria { get; set; }
        public AccountableDocumentFilterCriteria? accountableDocumentFilterCriteria { get; set; }
        public OrderReferenceCriteria? OrderReferenceCriteria { get; set; }
    }
}