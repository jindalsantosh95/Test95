using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignAssign;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignAssignEventMapperTest
    {
        public CampaignAssignEventMapperTest()
        {
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CommandToAssignEventMapperTest_Success()
        {
            var request = TestDataProvider.GetTestData<CampaignAssignEventData>("CampaignManagementMS\\CampaignAssignEventData");
            var cloudEvent = CampaignAssignEventMapper.CommandToAssignEventMapper(request);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignAssignInitiate.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CampaignAssignEvent.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }

        [Fact]
        public void CommandToAssignEventMapperTest_Failure()
        {
            var request = TestDataProvider.GetTestData<CampaignAssignEventData>("CampaignManagementMS\\CampaignAssignEventData");
            var cloudEvent = CampaignAssignEventMapper.CommandToAssignEventMapper(request);
            Assert.NotEqual("a4567788", cloudEvent.Id);
        }
    }
}
