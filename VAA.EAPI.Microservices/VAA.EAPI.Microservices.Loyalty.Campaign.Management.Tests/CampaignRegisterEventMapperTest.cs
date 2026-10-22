using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.MemberProfileUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.MemberProfileUpdate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignRegisterEventMapperTest
    {
        public CampaignRegisterEventMapperTest()
        {
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CommandToTragetedMembershipEventMapperTest_Success()
        {
            var request = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            var cloudEvent = CampaignRegisterEventMapper.CommandToTragetedMembershipEventMapper(request);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipUpdateEventInitiate.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateTargetedMembership.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }

        [Fact]
        public void CommandToTragetedMembershipEventMapperTest_Faliure()
        {
            var request = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            var cloudEvent = CampaignRegisterEventMapper.CommandToTragetedMembershipEventMapper(request);
            Assert.NotEqual("ab45677", cloudEvent.Id);
        }

        [Fact]
        public void CommandToRegisterEventMapper_Success()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\CampaignRegisterEventData");
            var cloudEvent = CampaignRegisterEventMapper.CommandToCampaignRegisterEventMapper(request);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignRegisterInitiate.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CampaignRegisterEvent.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }

        [Fact]
        public void CommandToRegisterEventMapper_Faliure()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\CampaignRegisterEventData");
            var cloudEvent = CampaignRegisterEventMapper.CommandToCampaignRegisterEventMapper(request);
            Assert.NotEqual("ab45677", cloudEvent.Id);
        }

        [Fact]
        public void CommandToMemberProfileUpdateEventMapper_Success()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\ProfileUpdateEventDataS2SFTG");
            MemberProfileUpdateEventData memberProfileUpdateData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(request);
            var cloudEvent = CampaignRegisterEventMapper.CommandToMemberProfileUpdateEventMapper(memberProfileUpdateData);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.MemberProfileUpdateEventInitiate.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateFlyingClubMember.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }

        [Fact]
        public void CommandToMemberProfileUpdateEventMapper_Failure()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\ProfileUpdateEventDataInstantSilver");
            MemberProfileUpdateEventData memberProfileUpdateData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(request);
            var cloudEvent = CampaignRegisterEventMapper.CommandToMemberProfileUpdateEventMapper(memberProfileUpdateData);
            Assert.NotEqual("ab45677", cloudEvent.Id);
        }
    }
}