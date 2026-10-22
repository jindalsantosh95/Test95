using System;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class EmailTriggeringEventGridMapperTest
    {
        public EmailTriggeringEventGridMapperTest()
        {
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CreateEventGridSchemaDataTest_Success()
        {
            var request = TestDataProvider.GetTestData<EmailEventRequestProcessCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            var registerEventData = Json.Deserialize<CampaignRegisterEventData>(Convert.ToString(request.Data));
            var emailEvent = EmailTriggeringEventRequestMapper.CreateEventGridSchemaData(registerEventData);
            var cloudEvent = EmailTriggeringEventGridMapper.EventDataMapper(emailEvent);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignRegisterEmailEvent.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignRegisterEmailEvent.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }
        [Fact]
        public void CreateEventGridSchemaDataTest_Failure()
        {
            var request = TestDataProvider.GetTestData<EmailEventRequestProcessCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            var registerEventData = Json.Deserialize<CampaignRegisterEventData>(Convert.ToString(request.Data));
            var emailEvent = EmailTriggeringEventRequestMapper.CreateEventGridSchemaData(registerEventData);
            var cloudEvent = EmailTriggeringEventGridMapper.EventDataMapper(emailEvent);
            Assert.NotEqual("2.0", cloudEvent.Specversion);
        }
    }
}
