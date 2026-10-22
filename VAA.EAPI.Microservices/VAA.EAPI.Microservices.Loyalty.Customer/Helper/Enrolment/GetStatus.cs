using System;
using System.Linq;
using System.Runtime.Serialization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment
{
    public static class GetStatus
    {
        public static string Status(EnrolmentRequestCommand memberEnrolmentRequest)
        {
            int age = DateTime.Now.Year - Convert.ToDateTime(memberEnrolmentRequest?.Data?.CustomerDetails?.DateOfBirth).Year;
            bool GuardianTerms = memberEnrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.GuardianTermsAndConditionsAccepted ?? false;
            string status = age < ConfigReader.Get<int>(Constants.Environment.EnrolmentAgeLimit) && GuardianTerms && memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.PowerAppChannelID))
                ? StatusEnum.Active.ToString()
                : age < ConfigReader.Get<int>(Constants.Environment.EnrolmentAgeLimit) && (memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.DeltaChannelID)) || memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.WebMyaChannelId)) || memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.AppMyaChannelId))) ? StatusEnum.ActivePending.ToEnumMemberAttrValue() : StatusEnum.Active.ToEnumMemberAttrValue();
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

        public static bool GetAgeAndParentConsentStatus(EnrolmentRequestCommand enrolmentRequest)
        {
            int age = DateTime.Now.Year - Convert.ToDateTime(enrolmentRequest?.Data?.CustomerDetails?.DateOfBirth).Year;
            bool GuardianTerms = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.GuardianTermsAndConditionsAccepted ?? false;
            return age < 18 && GuardianTerms;

        }
    }

}
