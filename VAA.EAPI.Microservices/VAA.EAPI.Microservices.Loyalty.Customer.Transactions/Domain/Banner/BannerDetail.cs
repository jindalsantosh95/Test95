using System.Collections.Generic;
using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner
{
    public class BannerDetail : CustomerTransaction
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string PartitionKey { get; set; }
        public string DocType { get; set; }

        public IEnumerable<Banners> Banners { get; set; }
        
        public static explicit operator BannerDetail(BannerDTO source) => new()
        {
            Id = source.Id,
            PartitionKey= source.PartitionKey,
            DocType= source.DocType,
            CustomerId = source.CustomerId,
            CreatedOn = source.CreatedOn,
            CreatedBy = source.CreatedBy,
            LastUpdatedOn = source.LastUpdatedOn,
            LastUpdatedBy = source.LastUpdatedBy,
            Banners = source.Banners,
            MemberNumber = source.MemberNumber
        };
    }
}
