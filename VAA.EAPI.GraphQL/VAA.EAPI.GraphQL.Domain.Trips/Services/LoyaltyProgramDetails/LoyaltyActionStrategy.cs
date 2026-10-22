using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services.LoyaltyProgramDetails
{
    public class LoyaltyActionStrategy : ILoyaltyActionStrategy
    {
        private readonly ILogger<LoyaltyActionStrategy> _logger;

        public LoyaltyActionStrategy(ILogger<LoyaltyActionStrategy> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LoyaltyActionType> DetermineAction(LoyaltyMemberDetailsRequest request, PassengerDetails passengerDetails)
        {
            if (request.LoyaltyMemberId.IsNullOrEmpty()) { return LoyaltyActionType.Delete; }

            var passenger = FindPassengerById(passengerDetails, request.PassengerId);

            var existingLoyaltyDetails = passenger?.LoyaltyProgramAccounts;

            if (existingLoyaltyDetails == null || !existingLoyaltyDetails.Any())
            {
                return LoyaltyActionType.Add;
            }

            var hasMatchingLoyalty = existingLoyaltyDetails.Any(loyalty =>
                IsMatchingLoyaltyProgram(loyalty, request.LoyaltyProgramCode, request.LoyaltyMemberId));

            return hasMatchingLoyalty ? LoyaltyActionType.NoChange : LoyaltyActionType.Update;
        }

        private static Passenger FindPassengerById(PassengerDetails passengerDetails, string passengerId)
        {
            return passengerDetails?.PassengerInformation?.Passengers
                ?.FirstOrDefault(p => p.PassengerResourceId == passengerId);
        }

        private static bool IsMatchingLoyaltyProgram(LoyaltyProgramAccount loyalty, string programCode, string memberId)
        {
            return loyalty?.LoyaltyProgram?.Owner?.FirstOrDefault()?.Code == programCode &&
                   loyalty?.Number == memberId;
        }
    }
}
