using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Models.Campaign;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class CampaignMapper
    {
        public static AssignCampaignRequest MapToCampaignAssign(RegisterCampaignRequest registerCampaignRequest, CustomerDTO customer)
        {
            return new AssignCampaignRequest
            {
                CampaignCode = ConfigReader.Get<string>(Common.Constants.Environment.AssignCampaignCode),
                CampaignId = ConfigReader.Get<string>(Common.Constants.Environment.AssignCampaignId),
                FirstName = customer.Customer.FirstName,
                LastName = customer.Customer.LastName,
                Title = customer.Customer.Title,
                Email = customer.Customer.ContactDetails?.EmailDetails?[0]?.emailId?.Trim(),
                MemberNumber = customer.Customer.LoyaltyNumber,
                CustomerId = customer.Customer.CustomerId,

                CreatedBy = registerCampaignRequest.CreatedBy,
            };
        }

        public static UpdateCampaignRequest MapToCampaignUpdate(string uniqueId, string memberNumber)
        {
            return new UpdateCampaignRequest
            {
                AmexUniqueId = uniqueId,
                FcNumber = memberNumber,
            };
        }
    }
}
