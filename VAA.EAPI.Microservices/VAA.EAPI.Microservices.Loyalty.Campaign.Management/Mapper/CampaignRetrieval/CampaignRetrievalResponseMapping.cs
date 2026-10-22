using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRetrieval
{
    public class CampaignRetrievalResponseMapping
    {
        public static MemberCampaignResponse ResponseMapping(IEnumerable<TargetedMembership> targatedMembers, List<CampaignDetails> refMemberCampaignDetails)
        {
            MemberCampaignResponse responses = new MemberCampaignResponse();
            List<MemberCampaign> memberCampaigns = new List<MemberCampaign>();
            foreach (var item in targatedMembers)
            {
                MemberCampaign campaignResponse = new MemberCampaign();
                campaignResponse.CustomerId = item.CustomerId;
                campaignResponse.AssignmentDate = item.AssignmentDate;
                campaignResponse.Comments = item.Comments;
                campaignResponse.CompletedDate = item.CompletedDate;
                campaignResponse.ContactedDate = item.ContactedDate;
                campaignResponse.CampaignId = item.ProfilingCampaignSourceId;
                campaignResponse.DocumentStatusCode = item.StatusCode;
                campaignResponse.Id = item.Id;
                campaignResponse.CreatedBy = item.CreatedBy;
                campaignResponse.CreatedOn = item.CreatedOn;
                campaignResponse.LastUpdatedBy = item.LastUpdatedBy;
                campaignResponse.LastUpdatedOn = item.LastUpdatedOn;
                campaignResponse.CampaignName = refMemberCampaignDetails.FirstOrDefault(x => x.CampaignId == item.ProfilingCampaignSourceId)?.CampaignName;
                campaignResponse.CampaignCode = refMemberCampaignDetails.FirstOrDefault(x => x.CampaignId == item.ProfilingCampaignSourceId)?.CampaignCode;
                campaignResponse.StartDate = refMemberCampaignDetails.FirstOrDefault(x => x.CampaignId == item.ProfilingCampaignSourceId)?.CampaignStartDate;
                campaignResponse.EndDate = refMemberCampaignDetails.FirstOrDefault(x => x.CampaignId == item.ProfilingCampaignSourceId)?.CampaignEndDate;
                campaignResponse.Status = refMemberCampaignDetails.FirstOrDefault(x => x.CampaignId == item.ProfilingCampaignSourceId)?.CampaignStatus;
                campaignResponse.Type = refMemberCampaignDetails.FirstOrDefault(x => x.CampaignId == item.ProfilingCampaignSourceId)?.CampaignType;
                campaignResponse.Description = refMemberCampaignDetails.FirstOrDefault(x => x.CampaignId == item.ProfilingCampaignSourceId)?.CampaignDescription;

                memberCampaigns.Add(campaignResponse);
                responses.MemberCampaigns = memberCampaigns;
            }
            return responses;
        }
    }
}
