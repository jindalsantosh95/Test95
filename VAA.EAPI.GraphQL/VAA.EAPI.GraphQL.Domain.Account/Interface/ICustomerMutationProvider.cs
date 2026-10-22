using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface ICustomerMutationProvider<T>
    {
        Task<T> GetTierResponse(string emailAddress);
        Task<string> UpgradeFlyingClub(EnrolmentRequest enrolmentRequest);
        Task<T> UpdateVaCustomerProfile(VirginAccountCustomerUpdate updateRequest, string fcNumber, string customerId);
    }
}
