using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment
{
    public static class GetLastName
    {
        public static string PopulateLastName(EnrolmentRequestCommand memberEnrolmentRequest)
        {
            return memberEnrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.TestMember == true
                ? "VirginTest"
                : memberEnrolmentRequest?.Data?.CustomerDetails?.LastName;
        }
    }
}
