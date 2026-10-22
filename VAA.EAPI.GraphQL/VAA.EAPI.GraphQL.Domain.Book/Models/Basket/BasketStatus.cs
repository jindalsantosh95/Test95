

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class BasketStatus
    {
        public Common.Enums.BasketStatusEnum CurrentBasketStatus { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
