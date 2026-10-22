using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Enum;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount
{
    public class VacManageAccountMapper
    {
        public static CloudEvent EmailEvent(Customers request, string correlationId, string template)
        {
            return new CloudEvent()
            {
                Id = correlationId,
                Subject = Constants.Application.UpdateCustomerEventSubject,
                Time = DateTime.UtcNow.ToString(),
                Data = new EmailTriggerRequestVacManageAccount
                {
                    To = request.ContactDetails.FirstOrDefault(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y")?.ContactValue,
                    Template = template,
                    FirstName = request.FirstName,
                    TierCode = Constants.Application.VacCode
                },
                Source = Constants.Application.ManagaeAccountUpdateTopicName,
                Type = Constants.Application.UpdateCustomerEventSubject,
                Specversion = Constants.Application.EventSpecversion
            };
        }

        public static string EmailTemplate(VacManageAccountUpdateAttributes updateAttribute)
        {
            return updateAttribute switch
            {
                VacManageAccountUpdateAttributes.Name => Constants.Application.NameUpdateTemplate,

                VacManageAccountUpdateAttributes.PhoneNumber => Constants.Application.PhoneUpdateTemplate,

                VacManageAccountUpdateAttributes.Email => Constants.Application.EmailUpdateTemplate,

                _ => Constants.Application.DateOfBirthUpdateTemplate,
            };
        }
    }
}
