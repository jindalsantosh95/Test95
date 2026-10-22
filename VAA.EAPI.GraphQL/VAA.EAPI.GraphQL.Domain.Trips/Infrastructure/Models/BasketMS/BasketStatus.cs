using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Enums;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    public class BasketStatus
    {
        public BasketStatusEnum CurrentBasketStatus { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
