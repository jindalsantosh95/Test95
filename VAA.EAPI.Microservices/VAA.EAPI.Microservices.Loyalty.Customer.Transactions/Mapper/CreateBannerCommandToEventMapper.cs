using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class CreateBannerCommandToEventMapper
    {
        public static BannerDetail getBannerDocEventMapper(CreateBannerRequest request, string id)
        {
            return new BannerDetail()
            {
                Banners = request.Banners,
                Id = id,
                CustomerId = request.CustomerID,
                MemberNumber = request.FcNumber,
                CreatedBy = Constants.Application.BannerChannelType,
                LastUpdatedBy = Constants.Application.BannerChannelType,
                CreatedOn = DateTime.UtcNow,
                LastUpdatedOn = DateTime.UtcNow,
                PartitionKey = request.CustomerID,
                DocType = Constants.Application.BannerDocType
            };
        }


    }
}
