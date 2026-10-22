namespace VAA.EAPI.Web.Book.Domain.Basket.Requests
{
    public class UpdateContactDetailsRequest
    {
        public string BasketId { get; set; }
        public ContactDetails ContactDetails { get; set; }
    }
}
