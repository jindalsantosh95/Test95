using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class EmailTriggeringEventGridMapper
    {
        public static CloudEvent EventDataMapper(EmailTriggeringEventRequest eventdata)
        {
            if (eventdata == null) throw new ArgumentNullException(nameof(eventdata));

            return new CloudEvent
            {
                Id = eventdata.Id,
                Subject = eventdata.Subject,
                Type = eventdata.EventType,
                Data = eventdata.Data,
                Source = ConfigReader.Get(Constants.Environment.AzureEventTopicEndPoint),
                Specversion = Constants.Application.DataVersion
            };
        }

        public static CloudEvent EmailEventDataMapper(SendEmailRequest data)
        {

            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.Application.WelcomeEmailEventSubject,
                Data = data,
                Subject = Constants.Application.WelcomeEmailEventSubject,
                Source = Constants.Application.EnrolmentEventHandlerTopicName,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion

            };
        }
    }
}
