using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.TargetMembershipDoc;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class TargetedMembershipDocCreateEventDataMapperTest
    {
        public TargetedMembershipDocCreateEventDataMapperTest()
        {
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void TargetedMembershipDocCreateEventDataMapperTest_Success()
        {
            var request = TestDataProvider.GetTestData<CampaignAssignEventData>("CampaignManagementMS\\CampaignAssignEventData");
            var campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            var eventData = TargetedMembershipDocCreateEventDataMapper.TargetMembershipDocCreateEventDataMapper(request, campaignData);
            Assert.Equal("Targeted", eventData.TargetedMembershipDocument.StatusCode);
            Assert.Equal("c220a7fb-8b85-4f4f-bac8-db94252a3e45", eventData.CustomerId);
        }

        [Fact]
        public void TargetedMembershipDocCreateEventDataMapperTest_Failure()
        {
            var request = TestDataProvider.GetTestData<CampaignAssignEventData>("CampaignManagementMS\\CampaignAssignEventData");
            var campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            var eventData = TargetedMembershipDocCreateEventDataMapper.TargetMembershipDocCreateEventDataMapper(request, campaignData);
            Assert.NotEqual("Regstered", eventData.TargetedMembershipDocument.StatusCode);
        }

        [Fact]
        public void CommandToTragetedMembershipEventMapperTest_Success()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDocCreateEventData>("CampaignManagementMS\\TargetedMembershipDocEventData");
            var cloudEvent = TargetedMembershipDocCreateEventDataMapper.CommandToTragetedMembershipEventMapper(request);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipCreateEvent.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CreateTargetedMembership.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }

        [Fact]
        public void CommandToTragetedMembershipEventMapperTest_Failure()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDocCreateEventData>("CampaignManagementMS\\TargetedMembershipDocEventData");
            var cloudEvent = TargetedMembershipDocCreateEventDataMapper.CommandToTragetedMembershipEventMapper(request);
            Assert.NotEqual("a4567787", cloudEvent.Id);
        }
    }
}
