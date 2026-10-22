using VAA.EAPI.GraphQL.Domain.Account.Models.FlyingClubToken;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IFlyingClubTokenProvider
    {
        Task<FlyingClubTokenResponse> IssueAsync(FlyingClubTokenRequest request);
        Task<FlyingClubTokenValidationResponse> ValidateAsync(string token);
    }
}
