namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class ContactUpdateRequest
    {
        public string BasketId { get; set; }
        public BasketContactInfo ContactDetails { get; set; }
        public bool? FcAutoMatched { get; set; }
    }
}
