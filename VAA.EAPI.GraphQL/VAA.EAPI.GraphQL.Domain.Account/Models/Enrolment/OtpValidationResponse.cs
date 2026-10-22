using Microsoft.Azure.WebJobs.Host;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class OtpValidationResponse
    {
        public string EnrolmentToken { get; set; }
        public bool IsEnrolledMember { get; set; }
    }
}
