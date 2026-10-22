using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class EmailTriggeringEventMapper
    {
        public static CloudEvent EventDataMapper(string eventId, string email, string OTP, string template)
        {

            return new CloudEvent
            {
                Id = eventId,
                Type = Constants.Application.OTPEmailEventSubject,
                Data = new SendEmailRequest
                {
                    Template = template,
                    DataFields = new DataFields
                    {
                        OTP = OTP,
                    },
                    To = ConfigReader.Get(Environment.AllowTestEmail) == "Y"
                        ? ConfigReader.Get(Environment.TestEmailId)
                        : email
                },
                Subject = Constants.Application.OTPEmailEventSubject,
                Source = ConfigReader.Get(Environment.EmailTopicEndPoint),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecVersion
            };
        }        
    }
}


