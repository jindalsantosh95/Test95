using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class AuthorizationMapper
    {
        public static Authorization[] MapEAPIAuthorizationToSCVAuthorization(CreateCustomerEntities createCustomerEntities, string authCustomerId)
        {
            if (createCustomerEntities == null) throw new ArgumentNullException(nameof(createCustomerEntities));

            if (!string.IsNullOrEmpty(createCustomerEntities.DateOfBirth) && (DateTime.Now - Convert.ToDateTime(createCustomerEntities.DateOfBirth)).TotalDays / 365 < 18)
            {
                string authGuid = Guid.NewGuid().ToString();
                //string authCustId = authCustomerId;
                return new Authorization[]
                {
                    new()
                    {
                        AuthorizerCustomerId  = authCustomerId,
                        Consents = ConsentMapper.MapConsentToSCVConsent(createCustomerEntities),
                        CountryCode = createCustomerEntities.GuardianDetails?.PhoneDetails?.CountryCode,
                        AuthorizerDateOfBirth = DateTimeHelper.ConvertDateTimeFormat(createCustomerEntities.GuardianDetails ?.DateOfBirth,Constants.Application.EAPIEnrolmentDOBFormat,Constants.Application.EAPIEnrolmentDOBFormat).ToString(),
                        Email = createCustomerEntities.GuardianDetails?.EmailDetails?.EmailId,
                        Name = createCustomerEntities.GuardianDetails?.Name,
                        Phone = createCustomerEntities.GuardianDetails?.PhoneDetails?.PhoneNumber,
                        RelationType = string.IsNullOrEmpty(createCustomerEntities.DateOfBirth)? null:(DateTime.Now - Convert.ToDateTime(createCustomerEntities.DateOfBirth)).TotalDays / 365 < 18 ? ConsentFor.FlyingClubGuardianship.ToString() : ConsentFor.FlyingClubAuthorizedUserAccount.ToString(),
                        AuthorizerFCMemberNumber=createCustomerEntities.GuardianDetails?.CardNumber,
                        AuthorizationId = authGuid,
                        SourceIdentifier = string.Empty,
                        SourceSystem = string.Empty
                    },
                    new()
                    {
                        AuthorizerCustomerId  = authCustomerId,
                        RelationType = Convert.ToString(ConsentFor.FlyingClubAuthorizedUserAccount),
                        Name = createCustomerEntities.GuardianDetails?.Name,
                        AuthorizerDateOfBirth = string.Empty,
                        Email =string.Empty,
                        Phone = string.Empty,
                        CountryCode = string.Empty,
                        AuthorizerFCMemberNumber=string.Empty,
                        Consents = ConsentMapper.MapConsentToMinorAge(createCustomerEntities),
                        AuthorizationId = authGuid,
                        SourceIdentifier = string.Empty,
                        SourceSystem = string.Empty
                    }
                };
            }
            else
            {

                return new Authorization[]
                {
                    new()
                    {
                        AuthorizerCustomerId  = authCustomerId,
                        Consents = ConsentMapper.MapConsentToSCVConsent(createCustomerEntities),
                        CountryCode = createCustomerEntities.GuardianDetails ?.PhoneDetails ?.CountryCode,
                        AuthorizerDateOfBirth = DateTimeHelper.ConvertDateTimeFormat(createCustomerEntities.GuardianDetails ?.DateOfBirth,Constants.Application.EAPIEnrolmentDOBFormat,Constants.Application.EAPIEnrolmentDOBFormat)?.ToString(Constants.Application.EAPIEnrolmentDOBFormat),
                        Email = createCustomerEntities.GuardianDetails ?.EmailDetails ?.EmailId,
                        Name = createCustomerEntities.GuardianDetails ?.Name,
                        Phone = createCustomerEntities.GuardianDetails ?.PhoneDetails ?.PhoneNumber,
                        RelationType = string.IsNullOrEmpty(createCustomerEntities.DateOfBirth)? null : (DateTime.Now - Convert.ToDateTime(createCustomerEntities?.DateOfBirth)).TotalDays / 365 < 18 ? ConsentFor.FlyingClubGuardianship.ToString() : ConsentFor.FlyingClubAuthorizedUserAccount.ToString(),
                        AuthorizerFCMemberNumber=createCustomerEntities.GuardianDetails?.CardNumber,
                        AuthorizationId = Guid.NewGuid().ToString(),
                        SourceIdentifier = string.Empty,
                        SourceSystem = string.Empty
                    }
                };
            }
        }
    }
}