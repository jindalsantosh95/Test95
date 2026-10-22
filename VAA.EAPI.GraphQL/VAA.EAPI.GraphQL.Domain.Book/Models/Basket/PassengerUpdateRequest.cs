namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class PassengerUpdateRequest
    {
        public string BasketId { get; set; }
        public BasketPerson People { get; set; }
    }
}
