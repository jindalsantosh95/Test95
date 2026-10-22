using System;
using System.Linq;
using System.Runtime.Serialization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using static VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails.MemberStatus;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Helper
{
    public static class GetStatus
    {
        public static string Status(EnrolmentRequest memberEnrolmentRequest)
        {
            int age = DateTime.Now.Year - Convert.ToDateTime(memberEnrolmentRequest?.CustomerDetails?.DateOfBirth).Year;
            bool GuardianTerms = memberEnrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.GuardianTermsAndConditionsAccepted ?? false;
            string status = age < ConfigReader.Get<int>(Constants.Environment.EnrolmentAgeLimit) && GuardianTerms && memberEnrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.PowerAppChannelID))
                ? StatusEnum.Active.ToString()
                : age < ConfigReader.Get<int>(Constants.Environment.EnrolmentAgeLimit) && (memberEnrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.DeltaChannelID)) || memberEnrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.WebMyaChannelId))) ? StatusEnum.ActivePending.ToEnumMemberAttrValue() : StatusEnum.Active.ToEnumMemberAttrValue();
            return status;
        }
        public static string ToEnumMemberAttrValue(this Enum @enum)
        {
            EnumMemberAttribute attribute =
                @enum.GetType().GetMember(@enum.ToString()).FirstOrDefault()?.
                    GetCustomAttributes(false).OfType<EnumMemberAttribute>().
                    FirstOrDefault();
            return attribute == null ? @enum.ToString() : attribute.Value;
        }

        public static bool GetAgeAndParentConsentStatus(EnrolmentRequest enrolmentRequest)
        {
            int age = DateTime.Now.Year - Convert.ToDateTime(enrolmentRequest?.CustomerDetails?.DateOfBirth).Year;
            bool GuardianTerms = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.GuardianTermsAndConditionsAccepted ?? false;
            return age < 18 && GuardianTerms;

        }
    }

}
