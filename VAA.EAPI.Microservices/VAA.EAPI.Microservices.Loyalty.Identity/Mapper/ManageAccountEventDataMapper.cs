using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class ManageAccountEventDataMapper
    {
        public static CloudEvent EmailEventDataMapperB2C(string EmailAddress)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubject,
                Data = new EmailTriggerRequestManageAccount
                {
                    To = EmailAddress,
                    Template = ConfigReader.Get(Constants.Environment.EmailUpdateTemplate)
                },
                Source = Constants.Application.ManageAccountUpdateTopicName,
                Type = Constants.Application.UpdateCustomerEventSubject,
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
