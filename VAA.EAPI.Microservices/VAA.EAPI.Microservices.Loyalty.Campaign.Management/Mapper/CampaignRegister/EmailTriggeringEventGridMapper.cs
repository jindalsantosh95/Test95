using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister
{
    public class EmailTriggeringEventGridMapper
    {
        public static CloudEvent EventDataMapper(EmailTriggeringEventRequest eventdata)
        {
            if (eventdata == null) throw new ArgumentNullException(nameof(eventdata));

            return new CloudEvent
            {
                Id = eventdata.Id,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignRegisterEmailEvent.ToString())),
                Data = eventdata.Data,
                Subject = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignRegisterEmailEvent.ToString())),
                Source = ConfigReader.Get(Constants.Environment.EmailTopicEndPoint),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
