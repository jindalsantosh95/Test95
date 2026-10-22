using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Mapper
{
    public class AccountCreationEmailTriggerEventMapper
    {
        public static CloudEvent EmailTriggerEventMapper(CreateCustomerEntities request, string correlationId)
        {
            return new CloudEvent()
            {
                Id = correlationId,
                Type = Constants.Application.VaAccEmailEvent,
                Subject = Constants.Application.VaAccEmailSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecVersion,
                Source = Constants.Application.VaAccEmailSource,
                Data = new CRMTriggerRequest
                {
                    FirstName = request.FirstName,
                    LastName= request.LastName,
                    EmailAddress = request.ContactDetails.EmailDetails.FirstOrDefault().EmailId.ToLower(),
                    TierCode=request.CurrentTier,
                    B2CPasswordResetLink = ConfigReader.Get(Constants.Environment.VaAccEmailB2CLink),
                    VACEnrolEmailTemplate = ConfigReader.Get(Constants.Environment.VaAccEmailTemplate)
                }

            };
        }
            
    }
}
