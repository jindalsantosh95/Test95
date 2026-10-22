using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MillionMiler;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.MillionMiler
{
    public class MillionMilerEmailTriggeringEventMapper
    {
        public static MillionMilerEmailTriggeringEventRequest CreateEventSchemaData(MillionMilerEmailTriggerCommand registerEventData)
        {
            return new MillionMilerEmailTriggeringEventRequest
            {
                Id = Guid.NewGuid().ToString(),
                EventType = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BasicEmailEvent.ToString())),
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BasicEmailEvent.ToString())),
                EventTime = DateTime.UtcNow,
                Data = new SendEmailRequest()
                {
                    Template = ConfigReader.Get(registerEventData.EmailTemplate),
                    DataFields = new DataFields()
                    {
                        Customer_FirstName = registerEventData?.FirstName,
                        Customer_LastName = registerEventData?.LastName,
                        Membership_Number = registerEventData?.MemberNumber,
                        Member_First_Name = registerEventData?.FirstName,
                    },
                    To = registerEventData?.Email
                }
            };
        }
    }
}
