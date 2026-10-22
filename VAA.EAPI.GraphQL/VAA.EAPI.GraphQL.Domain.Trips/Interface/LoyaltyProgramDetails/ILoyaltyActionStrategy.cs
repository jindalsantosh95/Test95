using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails
{
    public interface ILoyaltyActionStrategy
    {
        Task<LoyaltyActionType> DetermineAction(LoyaltyMemberDetailsRequest request, PassengerDetails passengerDetails);
    }
}