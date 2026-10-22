using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignRegisterEventResponseMapperTest
    {
        [Fact]
        public void CampaignRegisterEventResponseMapper_Success()
        {
            var response = CampaignRegisterEventResponseMapper.CampaignRegisterResponseMapper("a1234", "Ok");
            Assert.Equal(Status.OK, response.Status);
            Assert.Equal(Constants.Application.CampaignRegistrationInitiated, response.Value);
        }
    }
}
