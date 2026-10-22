using System;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

public class CampaignAssignMapper
{
    public static CampaignAssignRequest MapToCampaignAssign(CampaignAssignMutationRequest campaignAssignRequest, CustomerCustMS customer)
    {
        if (customer is null) throw new ArgumentNullException(nameof(customer));
        if (campaignAssignRequest is null) throw new ArgumentNullException(nameof(campaignAssignRequest));

        return new CampaignAssignRequest()
        {
            CampaignCode = campaignAssignRequest.CampaignCode,
            CampaignId = campaignAssignRequest.CampaignId,
            CustomerId = string.IsNullOrEmpty(campaignAssignRequest.CustomerId) ? customer.CustomerId : campaignAssignRequest.CustomerId,
            MemberNumber = string.IsNullOrEmpty(campaignAssignRequest.MemberNumber) ? customer.LoyaltyNumber : campaignAssignRequest.MemberNumber,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            CreatedBy = campaignAssignRequest.CreatedBy,
            Title = customer.Title,
            Email = customer.ContactDetails?.EmailDetails?[0]?.emailId?.Trim(),
        };
    }
}
