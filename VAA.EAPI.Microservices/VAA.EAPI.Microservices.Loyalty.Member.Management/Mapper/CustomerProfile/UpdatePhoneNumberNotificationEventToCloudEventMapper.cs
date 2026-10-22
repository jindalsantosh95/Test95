using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.CustomerProfile;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.CustomerProfile
{
    public class UpdatePhoneNumberNotificationEventToCloudEventMapper
    {
        public static CloudEvent CommandToEventMapper(UpdatePhoneNumberEventData updatePhoneNumberEventData)
        {
            ArgumentNullException.ThrowIfNull(updatePhoneNumberEventData);
           
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.ManageaccountUpdatePhoneSubscriberEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = updatePhoneNumberEventData,
                Source = Constants.Application.ManageAccountTopicName,
                Type = Constants.Application.ManageaccountUpdatePhoneSubscriberEventSubject,
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
