using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class ConsentMapper
    {
        public static Consent[] MapConsentToSCVConsent(CreateCustomerEntities createCustomerEntities)
        {
            return new Consent[]
            {
                new()
                {
                    ConsentFor = ConsentFor.FlyingClubGuardianship.ToString(),
                    TermsAndConditionsAccepted = createCustomerEntities?.GuardianDetails?.GuardianTermsAndConditionsAccepted == true ? "Y":"N",
                    ConsentProcessedDate = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    EffectiveFrom = string.Empty,
                    EffectiveTo = string.Empty,
                    CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    CreatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel
                }
            };
        }

        public static Consent[] MapConsentToMinorAge(CreateCustomerEntities createCustomerEntities)
        {
            return new Consent[]
            {
                new()
                {
                    ConsentFor = ConsentFor.FlyingClubGuardianship.ToString(),
                    TermsAndConditionsAccepted = string.Empty,
                    ConsentProcessedDate = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    EffectiveFrom = string.Empty,
                    EffectiveTo = string.Empty,
                    CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    CreatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel
                }
            };
        }
    }
}