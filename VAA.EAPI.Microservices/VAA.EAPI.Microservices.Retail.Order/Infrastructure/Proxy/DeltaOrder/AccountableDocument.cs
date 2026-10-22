namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class AccountableDocument
    {
        public string AccountableDocumentNum { get; set; }
        public string IssueLocalDate { get; set; }
        public string DocumentCategoryCode { get; set; }
        public Passenger Passenger { get; set; }
    }
}
