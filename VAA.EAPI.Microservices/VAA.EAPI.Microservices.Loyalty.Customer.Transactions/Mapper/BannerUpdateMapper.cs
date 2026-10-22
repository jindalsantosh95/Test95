using System;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class BannerUpdateMapper
    {
        public static BannerDetail BannerMapper (BannerResponse response, BannerCommandBase eventData)
        {

            var bannersList = response.Banners.ToList();

            foreach (Banners banner in bannersList) 
            {
                if (eventData.Banner.Banners.Any(x => x.BannerCode.Is(banner.BannerCode)))
                    banner.LastShownDateTime = DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);  
            }

            foreach (Banners banner in eventData.Banner.Banners)
            {
                if (!bannersList.Any(b => b.BannerCode.Is(banner.BannerCode)))
                {
                    bannersList.Add(new Banners
                    {
                        BannerCode = banner.BannerCode,
                        LastShownDateTime = DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat)
                    });
                }
            }

            return new BannerDetail
            {
                CustomerId = response.CustomerId,
                MemberNumber = response.MemberNumber,
                DocType = response.DocType,
                CreatedBy = response.CreatedBy,
                CreatedOn = response.CreatedOn,
                LastUpdatedBy = Constants.Application.Channel,
                LastUpdatedOn = DateTime.UtcNow,
                Banners = bannersList,
                Id = response.Id,
                PartitionKey = response.PartitionKey,
            };
        }
    }
}
