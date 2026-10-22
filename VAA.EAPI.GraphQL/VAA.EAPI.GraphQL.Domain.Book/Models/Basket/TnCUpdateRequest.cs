namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class TnCUpdateRequest
    {
        public string BasketId { get; set; }
        public BasketTermsAndConditions TermsAndConditions { get; set; }
    }
}
