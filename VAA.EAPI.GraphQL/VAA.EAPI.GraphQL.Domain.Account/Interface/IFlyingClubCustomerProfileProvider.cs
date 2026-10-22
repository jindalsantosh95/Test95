using VAA.EAPI.GraphQL.Domain.Account.Models;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IFlyingClubCustomerProfileProvider
    {
        Task<FlyingClubCustomerProfileResponse> GetAsync(string token);
    }
}
