using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails
{
    public interface ILoyaltyActionHandler
    {
        Task<LoyaltyOperationResult> ExecuteLoyaltyActionAsync(LoyaltyMemberDetailsRequest request, LoyaltyActionType actionType, FQTProgramRefData programDetails, IEnumerable<Infrastructure.Models.PassengerDetails.LoyaltyProgramAccount> existingLoyaltyProgramAccounts, CancellationToken cancellationToken = default);
    }
}
