using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class EmailTriggeringCloudEventMapper
    {
        public static CloudEvent EventDataMapper(EmailTriggeringEventRequest data)
        {

            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.Application.WelcomeEmailEventSubject,
                Data = data.Data,
                Subject = Constants.Application.WelcomeEmailEventSubject,
                Source = ConfigReader.Get(Environment.EmailTopicEndPoint),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion

            };
        }
    }
}
