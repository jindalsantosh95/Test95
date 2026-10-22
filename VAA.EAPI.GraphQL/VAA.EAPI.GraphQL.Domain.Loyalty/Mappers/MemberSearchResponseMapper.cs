using System;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FlyingClubMember;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberSearchViaPhone;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    public class MemberSearchResponseMapper
    {
        public static MemberInfo MemberSearchResponse(CustomerSearch custSearchDoc, Customers customer, VRedResponse pointBalance, FlyingClubMember flyingClubMember)
        {
            var primaryAddress = customer.AddressDetails?.FirstOrDefault(a => a.Primary == "Y");
            var primaryContact = customer.ContactDetails?.FirstOrDefault(c => c.Primary == "Y" && c.ContactType == "Phone");

            return new MemberInfo
            {
                EnvelopeSalutation = customer?.EnvelopeSalutation,
                MemberNumber = custSearchDoc?.LoyaltyMemberNumber,
                BirthDate = !string.IsNullOrEmpty(customer.DateOfBirth) ? DateTime.Parse(customer.DateOfBirth): null,
                CurrentTier = flyingClubMember?.TierCode,
                EmailAddress = custSearchDoc?.EmailAddress,
                AddressLine1 = primaryAddress?.AddressLine1,
                AddressLine2 = primaryAddress?.AddressLine2,
                AddressLine3 = primaryAddress?.AddressLine3,
                City = primaryAddress?.City,
                Region = primaryAddress?.Province,
                Country = primaryAddress?.Country,
                PostCode = primaryAddress?.PostCode,
                PhoneType = primaryContact?.ContactSubType,
                PhoneNumber = primaryContact?.ContactValue,
                PhoneCountryCode = primaryContact?.CountryCode,
                EnrollmentDate = flyingClubMember?.EnrollmentDate,
                MemberPointBalance = pointBalance?.Current
            };
        }
    }
}

