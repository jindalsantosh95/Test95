namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class ApplicationStatusResponse
    {
        public string DataShareToken { get; set; }
        public string ApplicationDecision { get; set; }
        public string CreditLimit { get; set; }
        public string TokenStatus { get; set; }
        public DateTime TokenCreationDate { get; set; }
    }
}