using VAA.EAPI.Domain.Entities.Common;


namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister
{
    public class CampaignRegisterEventResponseMapper
    {
        public static Response<string> CampaignRegisterResponseMapper(string corelationid, string status)
        {
            return new Response<string>
            {
                Status = Status.OK,
                Value = Constants.Application.CampaignRegistrationInitiated
            };
        }
    }
}
