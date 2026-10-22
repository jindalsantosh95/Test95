using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignAssign
{
    public class CampaignAssignEventResponseMapper
    {
        public static Response<string> CampaignAssignResponseMapper(string corelationid, string status)
        {
            return new Response<string>
            {
                Status = Status.OK,
                Value = Constants.Application.CampaignAssignInitiated
            };
        }
    }
}
