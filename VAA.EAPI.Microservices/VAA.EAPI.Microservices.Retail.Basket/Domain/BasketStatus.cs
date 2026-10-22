using System;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class BasketStatus
    {
        public BasketStatusEnum CurrentBasketStatus { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
