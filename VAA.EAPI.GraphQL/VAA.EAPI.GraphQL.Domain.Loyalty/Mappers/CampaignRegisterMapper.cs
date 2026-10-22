using System;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    public class CampaignRegisterMapper
    {
        public static CampaignRegisterRequest MapToCampaignRegister(CampaignRegisterMutationRequest campaignRegisterRequest, CustomerCustMS customer)
        {
            if (customer is null) throw new ArgumentNullException(nameof(customer));
            if (campaignRegisterRequest is null) throw new ArgumentNullException(nameof(campaignRegisterRequest));

            return new CampaignRegisterRequest()
            {
                CampaignCode = campaignRegisterRequest.CampaignCode,
                CampaignId = campaignRegisterRequest.CampaignId,
                CustomerId = string.IsNullOrEmpty(campaignRegisterRequest.CustomerId) ? customer.CustomerId : campaignRegisterRequest.CustomerId,
                MemberNumber = string.IsNullOrEmpty(campaignRegisterRequest.MemberNumber) ? customer.LoyaltyNumber : campaignRegisterRequest.MemberNumber,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                CreatedBy = campaignRegisterRequest.CreatedBy,
                DocumentId = campaignRegisterRequest.DocumentId,
                Title = customer.Title,
                Email = customer.ContactDetails?.EmailDetails?[0]?.emailId?.Trim(),
            };
        }
    }
}
