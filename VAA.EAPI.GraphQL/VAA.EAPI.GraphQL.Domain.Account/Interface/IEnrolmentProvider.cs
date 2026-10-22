using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IEnrolmentProvider
    {
        Task<FlyingClubSignUpResponse> EnrolFcMember(CreateCustomerAccountRequest request);
        Task<FlyingClubSignUpResponse> EnrolYoungFlyerMember(CreateCustomerAccountRequest request);
        Task<EntraEnrolTokenResponse> EntraEnrolFcMember(CreateCustomerAccountRequest request);
    }
}
