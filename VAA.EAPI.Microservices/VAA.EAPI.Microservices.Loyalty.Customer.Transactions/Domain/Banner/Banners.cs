using System.Collections.Generic;
using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner
{
    public class Banners
    {
        public string BannerCode { get; set; }
        public string LastShownDateTime { get; set; }
    }
}
