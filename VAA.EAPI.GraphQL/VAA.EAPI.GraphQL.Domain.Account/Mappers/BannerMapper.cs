using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public static class BannerMapper
    {
        public static BannerData CreateBanerResponseMapper(string customerId, string fcNumber, string bannerCode)
        {
            var bannerData = new BannerData
            {
                CustomerID = customerId,
                FcNumber = fcNumber,
                Banners = new List<BannerItem>
                            {
                                new BannerItem { BannerCode = bannerCode }
                            }
            };

            return bannerData;
        }

        public static Banner UpdateBanerResponseMapper(string bannerCode)
        {
            var bannerData = new Banner
            {
                BannerData = new BannerData
                {
                    Banners = new List<BannerItem>
                            {
                                new BannerItem { BannerCode = bannerCode }
                            }
                }
            };

            return bannerData;
        }
    }
}
