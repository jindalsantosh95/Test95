using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignUnassign;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignUnassignCommandToEventMapperTest
    {
        public CampaignUnassignCommandToEventMapperTest()
        {
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CommandToEventMapper_Success()
        {
            var request = TestDataProvider.GetTestData<CampaignUnassignRequestData>("CampaignManagementMS\\CampaignUnassignValid");
            TargetedMembershipDocDeleteEventData eventData = new TargetedMembershipDocDeleteEventData()
            {
                CustomerId = request.CustomerId,
                TargetedMembershipDocument = new TargetedMembershipDeleteDocument()
                {
                    Id = request.DocumentId,
                    CustomerId = request.CustomerId
                }
            };
            var cloudEvent = CampaignUnassignCommandToEventMapper.CommandToEventMapper(eventData);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipDeleteEvent.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipDeleteEvent.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }

        [Fact]
        public void CommandToEventMapper_Faliure()
        {
            var request = TestDataProvider.GetTestData<CampaignUnassignRequestData>("CampaignManagementMS\\CampaignUpdateValid");
            TargetedMembershipDocDeleteEventData eventData = new TargetedMembershipDocDeleteEventData()
            {
                CustomerId = request.CustomerId,
                TargetedMembershipDocument = new TargetedMembershipDeleteDocument()
                {
                    Id = request.DocumentId,
                    CustomerId = request.CustomerId
                }
            };
            var cloudEvent = CampaignUnassignCommandToEventMapper.CommandToEventMapper(eventData);
            Assert.NotEqual("ab45677", cloudEvent.Id);
        }
    }
}