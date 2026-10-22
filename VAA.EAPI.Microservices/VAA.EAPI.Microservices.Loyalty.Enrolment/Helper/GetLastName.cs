using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Helper
{
    public static class GetLastName
    {
        public static string PopulateLastName(EnrolmentRequest memberEnrolmentRequest)
        {
            return memberEnrolmentRequest?.CustomerDetails?.LoyaltyMember?.TestMember == true ? "VirginTest" : memberEnrolmentRequest?.CustomerDetails?.LastName;
        }
    }
}
