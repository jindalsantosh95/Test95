using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;
using IdentityType = VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C.IdentityType;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Mapper
{
    public class IdentityRequestMapper
    {
        public static MemberEnrolmentGraphRequest MapIdentityrequest(IdentityRepoCommand b2CEnrolmentRepoQuery, bool isEnabled)
        {
            if (b2CEnrolmentRepoQuery == null) throw new ArgumentNullException(nameof(b2CEnrolmentRepoQuery));

            string emailaddress = b2CEnrolmentRepoQuery.CreateCustomerEntities?.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId;

            if (string.IsNullOrWhiteSpace(b2CEnrolmentRepoQuery.CreateCustomerEntities?.Password))
            {
                b2CEnrolmentRepoQuery.CreateCustomerEntities.Password = Hash.Compute(Guid.NewGuid().ToString());
            }

            return new MemberEnrolmentGraphRequest
            {
                AccountEnabled = isEnabled,
                DisplayName = $"{b2CEnrolmentRepoQuery.CreateCustomerEntities?.FirstName}  {b2CEnrolmentRepoQuery.CreateCustomerEntities?.LastName}",
                GivenName = b2CEnrolmentRepoQuery.CreateCustomerEntities?.FirstName,
                Surname = b2CEnrolmentRepoQuery.CreateCustomerEntities?.LastName,
                PasswordProfile = new PasswordProfile
                {

                    ForceChangePasswordNextSignIn = false,
                    Password = b2CEnrolmentRepoQuery.CreateCustomerEntities?.Password
                },
                MemberType = b2CEnrolmentRepoQuery.CreateCustomerEntities?.MemberType,
                MemberStatus = b2CEnrolmentRepoQuery.CreateCustomerEntities?.Status,
                SecurityQuestion1 = b2CEnrolmentRepoQuery.CreateCustomerEntities?.SecurityQuestions?.FirstOrDefault()?.Question,
                SecurityAnswer1 = Hash.Compute(b2CEnrolmentRepoQuery.CreateCustomerEntities?.SecurityQuestions?.FirstOrDefault()?.Answer),
                SecurityQuestion2 = b2CEnrolmentRepoQuery.CreateCustomerEntities?.SecurityQuestions?.LastOrDefault()?.Question,
                SecurityAnswer2 = Hash.Compute(b2CEnrolmentRepoQuery.CreateCustomerEntities?.SecurityQuestions?.LastOrDefault()?.Answer),
                CustomerId = b2CEnrolmentRepoQuery.CreateCustomerEntities?.CustomerId,
                UserPrincipalName = $"{b2CEnrolmentRepoQuery.CreateCustomerEntities?.FcNumber}@{ConfigReader.Get(Environment.B2CDomain)}",
                Identities = GetIdentities(emailaddress, b2CEnrolmentRepoQuery.CreateCustomerEntities?.FcNumber)
            };
        }
        public static MemberEnrolmentGraphRequest MapRegisterRequest(MemberRegisterRequest b2CRegisterRequest, bool accountEnabled = true)
        {
            if (b2CRegisterRequest == null) throw new ArgumentNullException(nameof(b2CRegisterRequest));

            string emailaddress = b2CRegisterRequest.EmailAddress;
            if (string.IsNullOrWhiteSpace(b2CRegisterRequest.Password))
            {
                b2CRegisterRequest.Password = Hash.Compute(Guid.NewGuid().ToString());
            }

            return new MemberEnrolmentGraphRequest
            {
                AccountEnabled = accountEnabled,
                DisplayName = b2CRegisterRequest?.FirstName,
                GivenName = b2CRegisterRequest?.FirstName,
                Surname = b2CRegisterRequest?.LastName,
                PasswordProfile = new PasswordProfile
                {

                    ForceChangePasswordNextSignIn = false,
                    Password = b2CRegisterRequest?.Password
                },
                UserPrincipalName = $"{b2CRegisterRequest?.MemberNumber}@{ConfigReader.Get(Environment.B2CDomain)}",
                Identities = GetIdentities(b2CRegisterRequest?.EmailAddress, b2CRegisterRequest?.MemberNumber),
                MemberStatus = Constants.Application.MemberStatus,
                MemberType = b2CRegisterRequest.MemberType,
                CustomerId = b2CRegisterRequest.CustomerId
            };
        }
        private static List<IdentityType> GetIdentities(string email, string member)
        {
            List<IdentityType> identityTypes = new();
            if (!string.IsNullOrEmpty(email))
            {
                identityTypes.Add(new IdentityType { SignInType = ConfigReader.Get(Environment.SignInEmailAddress), Issuer = ConfigReader.Get(Environment.B2CDomain), IssuerAssignedId = email });
            }
            if (!string.IsNullOrEmpty(member))
            {
                identityTypes.Add(new IdentityType { SignInType = ConfigReader.Get(Environment.SignInUserName), Issuer = ConfigReader.Get(Environment.B2CDomain), IssuerAssignedId = member });
            }

            return identityTypes;
        }
    }
}
