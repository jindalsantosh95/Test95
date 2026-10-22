using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.GroupDoc;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseHoldAccountEmailTriggeringEventMapperTest
    {
        public HouseHoldAccountEmailTriggeringEventMapperTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CreateEventGridSchemaDataTest_Success()
        {
            HouseHoldAccountEmailTriggerCommand command = new HouseHoldAccountEmailTriggerCommand()
            {
                Id = Guid.NewGuid(),
                FirstName = "First",
                LastName = "Last",
                MemberNumber = "MemberId001",
                CustomerId = "CustomerId001",
                Email = "Customer001@mail.com",
                EmailTemplate = "YoungFlyerEnrolmentConfirmationEmailTemplate",
                ChildFirstName = "Child",
                ChildMemberNumber = "ChildMember001"
            };
            var emailEvent = HouseHoldAccountEmailTriggeringEventMapper.CreateEventGridSchemaData(command);
            Assert.Equal("Customer001@mail.com", emailEvent.Data.To);
            Assert.Equal("vaaLoyaltyU18Parent", emailEvent.Data.Template);
            var dataFields = Assert.IsType<VAA.EAPI.Domain.Entities.Email.DataFields>(emailEvent.Data.DataFields);
            Assert.Equal("Child", dataFields.Child_First_Name);
            Assert.Equal("ChildMember001", dataFields.Child_Membership_Number);
        }

        [Fact]
        public void CreateEventGridSchemaDataTest_Failure()
        {
            HouseHoldAccountEmailTriggerCommand command = new HouseHoldAccountEmailTriggerCommand()
            {
                Id = Guid.NewGuid(),
                FirstName = "First",
                LastName = "Last",
                MemberNumber = "MemberId001",
                CustomerId = "CustomerId001",
                Email = "Customer001@mail.com",
                EmailTemplate = "YoungFlyerEnrolmentConfirmationEmailTemplate",
                ChildFirstName = "Child",
                ChildMemberNumber = "ChildMember001"
            };
            var emailEvent = HouseHoldAccountEmailTriggeringEventMapper.CreateEventGridSchemaData(command);
            Assert.NotEqual("test1.email@fly.virgin.com", emailEvent.Data.To);
        }
    }
}
