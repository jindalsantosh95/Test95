using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class AuditUpdatePreferenceRequestMapper
    {
        public static CloudEvent EventDataMapper(string eventId, CustomerProfileCommand updatePreferenceCustomerProfile, CustomerProfileDTO customerProfileDocResponse)
        {
            if (updatePreferenceCustomerProfile == null) throw new ArgumentNullException(nameof(updatePreferenceCustomerProfile));

            return new CloudEvent
            {
                Id = eventId,
                Type = Constants.Application.LoyaltyAuditPreferenceEventType,
                Data = LoyaltyAuditPreferenceMapper.MaployaltyAuditPreferenceMapper(updatePreferenceCustomerProfile, customerProfileDocResponse),
                Subject = Constants.Application.LoyaltyAuditPreferenceSubject,
                Source = Constants.Application.loyaltyAuditPreferenceSource,
                Time = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                DataContentType = Constants.Application.ContenTypeapplicationjson,
                Specversion = Constants.Application.SpecVersion
            };
        }

        public static CloudEvent EmailEventDataMapper(string eventId, CustomerProfileCommand request)
        {
            return new CloudEvent
            {
                Id = eventId,
                Subject = Constants.Application.UpdateCustomerEventSubject,
                Data = new ManageAccountEmailMsRequest
                {
                    To = request?.EmailAddress,
                    Template = Constants.Application.UpdateMarketingPreferenceTemplate,
                    FirstName = request?.FirstName,
                    TierCode = request?.TierCode

                },
                Source = Constants.Application.ManagaeAccountUpdateTopicName,
                Type = Constants.Application.UpdateCustomerEventSubject,
                Specversion = Constants.Application.SpecVersion
            };
        }
    }
}
