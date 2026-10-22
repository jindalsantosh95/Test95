namespace VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels
{
    public class PaymentInformation
    {
        public string PaymentStatus { get; set; }
        public string ObfusticatedCard { get; set; }
        public string CardType { get; set; }
        public double PaymentAmount { get; set; }
    }
}
