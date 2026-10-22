using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignAssign;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignAssignEventDataMapperTest
    {
        [Fact]
        public void CampaignAssignRequestToEventDataMapperTest_Success()
        {
            var request = TestDataProvider.GetTestData<CamapignAssignRequestData>("CampaignManagementMS\\CampaignAssignValidRequest");
            var campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            var eventData = CampaignAssignEventDataMapper.CampaignAssignRequestToEventDataMapper(request, campaignData);
            Assert.Equal("Registered", eventData.DocumentStatus);
            Assert.Equal("9999999998", eventData.MemberNumber);
        }

        [Fact]
        public void CampaignAssignRequestToEventDataMapperTest_Failure()
        {
            var request = TestDataProvider.GetTestData<CamapignAssignRequestData>("CampaignManagementMS\\CampaignAssignValidRequest");
            var campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            var eventData = CampaignAssignEventDataMapper.CampaignAssignRequestToEventDataMapper(request, campaignData);
            Assert.NotEqual("Targeted", eventData.DocumentStatus);
        }
    }
}
