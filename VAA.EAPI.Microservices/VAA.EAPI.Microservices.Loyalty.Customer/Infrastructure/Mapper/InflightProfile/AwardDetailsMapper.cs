using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.InflightProfile
{
    public class AwardDetailsMapper
    {
        public static List<Award> MapAwards(Proxy.SCVPlusProfile.Customer customerDetails)
        {
            List<Award> awards = new List<Award>();

            if (customerDetails.LoyaltyMember.Awards?.Count > 0)
            {
                foreach (Proxy.SCVPlusProfile.Award award in customerDetails.LoyaltyMember.Awards)
                {
                    awards.Add(new Award()
                    {
                        AwardId = award.AwardId,
                        AwardNumber = award.AwardNumber,
                        AwardValue = award.AwardValue,
                        ProductId = award.ProductId,
                        ProductName = award.ProductName,
                        StartDate = award.StartDate,
                        ExpiryDate = award.ExpiryDate

                    });
                }

                return awards;

            }

            return null;
        }
    }
}
