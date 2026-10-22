using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BirthdayEmail
{
    public class BirthdayEmailTriggeringEventMapper
    {
        public static BirthdayEmailTriggeringEventRequest CreateEventGridSchemaData(MemberContactData registerEventData)
        {
            return new BirthdayEmailTriggeringEventRequest
            {
                Id = Guid.NewGuid().ToString(),
                EventType = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BirthdayEmailEvent.ToString())),
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BirthdayEmailEvent.ToString())),
                EventTime = DateTime.UtcNow,
                Data = new SendEmailRequest()
                {
                    Template = ConfigReader.Get(Constants.Environment.BirthdayEmailTemplate),
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
