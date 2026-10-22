using System;
using System.Collections;
using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.InflightProfile
{
    class MemberProfileResponseMapper
    {
        public static Domain.InflightProfile.CustomerProfile MapMemberDetailsToCustomerProfile(Proxy.SCVPlusProfile.CustomerProfile scvCustomerProfile, string customerId, IEnumerable<CountryDetails> countryDetails)
        {
            if (scvCustomerProfile != null && scvCustomerProfile.CustomerDetails != null)
            {
                return new Domain.InflightProfile.CustomerProfile()
                {
                    Customer = new Domain.InflightProfile.CustomerProfileDetails()
                    {
                        AddressDetails = AddressDetailsMapper.MapAddresses(scvCustomerProfile.CustomerDetails, countryDetails),
                        ContactDetails = ContactDetailsMapper.MapContactDetails(scvCustomerProfile.CustomerDetails),
                        CustomerType = !string.IsNullOrEmpty(scvCustomerProfile.CustomerDetails.LoyaltyMember.FlyingClubCardNumber) ? Domain.InflightProfile.CustomerInformation.CustomerTypeEnum.Loyalty : Domain.InflightProfile.CustomerInformation.CustomerTypeEnum.Basic,
                        DateOfBirth = scvCustomerProfile.CustomerDetails.DateOfBirth?.ToString("yyyy-MM-dd"),
                        LoyaltyNumber = scvCustomerProfile.CustomerDetails.LoyaltyMember.FlyingClubCardNumber,
                        Nationality = null,
                        PreferredFirstName = scvCustomerProfile.CustomerDetails.FirstName,
                        PreferredGender = scvCustomerProfile.CustomerDetails.Gender,
                        PreferredLastName = scvCustomerProfile.CustomerDetails.LastName,
                        PreferredPronouns = null,
                        PreferredSalutation = scvCustomerProfile.CustomerDetails.PreferredSalutation,
                        CustomerId = customerId,
                        Status = EnumOperations.GetEnumDescription((StatusEnum)Enum.Parse(typeof(StatusEnum), scvCustomerProfile.CustomerDetails.LoyaltyMember.Status.ToString())),
                        HeadOfGroupMemberNumber = scvCustomerProfile.CustomerDetails.LoyaltyMember.HeadOfHouseholdMemberNumber,
                        MemberAccountType = Convert.ToString(scvCustomerProfile.CustomerDetails.LoyaltyMember.MemberType),
                        TermsAndConditionsAccepted = Convert.ToString(scvCustomerProfile.CustomerDetails.LoyaltyMember.TermsAndConditionsAccepted),
                        AwardDetails = AwardDetailsMapper.MapAwards(scvCustomerProfile.CustomerDetails),
                        SuppSilverHeadAccount = Convert.ToString(scvCustomerProfile.CustomerDetails.LoyaltyMember.ReferredBySSA),
                        MemberTier = Convert.ToString(scvCustomerProfile.CustomerDetails.LoyaltyMember.CurrentTier),
                        PNRName = scvCustomerProfile.CustomerDetails.PnrName
                    },
                    FlightProfile = null // Is not required as part of response
                };
            }
            else return null;
        }
    }
}
