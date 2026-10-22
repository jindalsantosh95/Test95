namespace VAA.EAPI.Web.Book.Domain.Basket.Requests
{
    public class UpdatePeopleRequest
    {
        public string BasketId { get; set; }
        public People People { get; set; }
    }
}
