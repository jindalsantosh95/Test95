using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class BannerDTO : CosmosDocument
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string DocType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public IEnumerable<Banners> Banners { get; set; }

        public static explicit operator BannerDTO(BannerDetail banner) => new()
        {
            MemberNumber = banner.MemberNumber,
            PartitionKey = banner.CustomerId,
            DocType = Constants.Application.BannerDocType,
            Id = banner.Id,
            CustomerId = banner.CustomerId,
            CreatedOn = banner.CreatedOn.ToUniversalTime(),
            CreatedBy = Constants.Application.BannerChannelType,
            LastUpdatedOn = banner.LastUpdatedOn.ToUniversalTime(),
            LastUpdatedBy = banner.LastUpdatedBy,
            Banners = banner.Banners.Select(b => new Banners()
            {
                LastShownDateTime = b.LastShownDateTime.IsNullOrEmpty() ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : b.LastShownDateTime,
                BannerCode = b.BannerCode
            })
        };
    }
}
