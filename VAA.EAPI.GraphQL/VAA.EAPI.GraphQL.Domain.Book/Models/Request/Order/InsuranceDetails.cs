namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    public class InsuranceDetails
    {
        public string BusinessUnit { get; set; }
        public string TransactionID { get; set; }
        public Purchase Purchase { get; set; }
    }
}