namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class CreateBasketRequest
    {
        public Guid BasketId { get; set; }
        public string OfferId { get; set; }
        public string Validation { get; set; }
        public IEnumerable<BasketPerson> People { get; set; }
        public BasketContactInfo ContactDetail { get; set; }
        public BasketTermsAndConditions TermsAndConditions { get; set; }
    }
}
