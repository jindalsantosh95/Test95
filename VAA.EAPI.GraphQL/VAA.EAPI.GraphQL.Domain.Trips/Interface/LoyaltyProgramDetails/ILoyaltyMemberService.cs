using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails
{
    public interface ILoyaltyMemberService
    {
        Task<LoyaltyMemberDetailsResponse> ProcessLoyaltyMemberDetailsAsync(
            LoyaltyMemberDetailsRequest request,
            CancellationToken cancellationToken = default);
    }
}
