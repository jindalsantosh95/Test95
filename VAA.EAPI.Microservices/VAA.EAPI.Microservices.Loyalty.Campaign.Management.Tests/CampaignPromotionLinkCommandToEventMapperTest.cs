using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionLinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignPromotionLinking;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignPromotionLinkCommandToEventMapperTest
    {
        public CampaignPromotionLinkCommandToEventMapperTest()
        {
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CommandToEventMapper_Success()
        {
            var request = TestDataProvider.GetTestData<CampaignPromotionLinkRequestData>("CampaignManagementMS\\CampaignPromotionLinkingValid");
            var cloudEvent = CampaignPromotionLinkCommandToEventMapper.CommandToEventMapper(request);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.LinkCampaignPromotion.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.LinkCampaignPromotion.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }
        [Fact]
        public void CommandToEventMapper_Faliure()
        {
            var request = TestDataProvider.GetTestData<CampaignPromotionLinkRequestData>("CampaignManagementMS\\CampaignPromotionLinkingValid");
            var cloudEvent = CampaignPromotionLinkCommandToEventMapper.CommandToEventMapper(request);
            Assert.NotEqual("ab45677", cloudEvent.Id);
        }
    }
}