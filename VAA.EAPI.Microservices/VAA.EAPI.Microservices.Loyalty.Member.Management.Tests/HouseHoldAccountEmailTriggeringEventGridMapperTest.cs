using System;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.HouseHoldLink;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.GroupDoc;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseHoldAccountEmailTriggeringEventGridMapperTest
    {
        public HouseHoldAccountEmailTriggeringEventGridMapperTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EventDataMapperDataTest_Success()
        {
            HouseHoldAccountEmailTriggerCommand command = new HouseHoldAccountEmailTriggerCommand()
            {
                Id = Guid.NewGuid(),
                FirstName = "First",
                LastName = "Last",
                MemberNumber = "MemberId001",
                CustomerId = "CustomerId001",
                Email = "Customer001@mail.com",
                EmailTemplate = "HoHLinkHeadSilverEmalTemplate"
            };

            HouseHoldAccountEmailTriggeringEventRequest triggerEvent = new HouseHoldAccountEmailTriggeringEventRequest()
            {
                Id = Guid.NewGuid().ToString(),
                EventType = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BasicEmailEvent.ToString())),
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BasicEmailEvent.ToString())),
                EventTime = DateTime.UtcNow,
                Data = new SendEmailRequest()
                {
                    Template = command?.EmailTemplate,
                    DataFields = new DataFields()
                    {
                        Customer_FirstName = command?.FirstName,
                        Customer_LastName = command?.LastName,
                        Membership_Number = command?.MemberNumber,
                        Member_First_Name = command?.FirstName,
                    },
                    To = command?.Email
                }
            };

            var cloudEvent = HouseHoldAccountEmailTriggeringEventGridMapper.EventDataMapper(triggerEvent);

            Assert.Equal(EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BasicEmailEvent.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BasicEmailEvent.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }
    }
}
