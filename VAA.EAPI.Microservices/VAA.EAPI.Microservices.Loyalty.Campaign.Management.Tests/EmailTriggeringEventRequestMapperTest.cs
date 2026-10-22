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
    public class EmailTriggeringEventRequestMapperTest
    {
        [Fact]
        public void CreateEventGridSchemaDataTest_Success()
        {
            var request = TestDataProvider.GetTestData<EmailEventRequestProcessCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            var registerEventData = Json.Deserialize<CampaignRegisterEventData>(Convert.ToString(request.Data));
            var emailEvent = EmailTriggeringEventRequestMapper.CreateEventGridSchemaData(registerEventData);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CampaignRegisterEmailEvent.ToString())), emailEvent.EventType);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignRegisterEmailEvent.ToString())), emailEvent.Subject);
            Assert.Equal("test.email@fly.virgin.com", emailEvent.Data.To);
        }
        [Fact]
        public void CreateEventGridSchemaDataTest_Failure()
        {
            var request = TestDataProvider.GetTestData<EmailEventRequestProcessCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            var registerEventData = Json.Deserialize<CampaignRegisterEventData>(Convert.ToString(request.Data));
            var emailEvent = EmailTriggeringEventRequestMapper.CreateEventGridSchemaData(registerEventData);
            Assert.NotEqual("test1.email@fly.virgin.com", emailEvent.Data.To);
        }
    }
}