using System;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister
{
    public class EmailTriggeringEventRequestMapper
    {
        public static EmailTriggeringEventRequest CreateEventGridSchemaData(CampaignRegisterEventData registerEventData)
        {
            return new EmailTriggeringEventRequest
            {
                Id = Guid.NewGuid().ToString(),
                EventType = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CampaignRegisterEmailEvent.ToString())),
                Subject = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignRegisterEmailEvent.ToString())),
                EventTime = DateTime.UtcNow,
                Data = new SendEmailRequest()
                {
                    Template = Constants.Application.EmailTemplate,
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
