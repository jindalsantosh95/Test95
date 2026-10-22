using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;

namespace VAA.EAPI.GraphQL.Domain.Trips.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class ManangeLoyaltyMemberMutation
    {
        private readonly ILoyaltyMemberService _loyaltyMemberService;
        private readonly ILogger<ManangeLoyaltyMemberMutation> _logger;

        public ManangeLoyaltyMemberMutation(ILoyaltyMemberService loyaltyMemberService, ILogger<ManangeLoyaltyMemberMutation> logger)
        {
            _loyaltyMemberService = loyaltyMemberService ?? throw new ArgumentNullException(nameof(loyaltyMemberService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("manageLoyaltyMemberMutation")]
        [GraphQLDescription("Manange loyalty member details per passenger of a trip")]
        public async Task<LoyaltyMemberDetailsResponse> SelectLoyaltyMemberDetails(LoyaltyMemberDetailsRequest loyaltyMemberDetailsRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _loyaltyMemberService.ProcessLoyaltyMemberDetailsAsync(loyaltyMemberDetailsRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {loyaltyMemberDetailsRequest.TripReservationId} |Error message - {ex.Message}| Inner ex: {ex.InnerException?.Message}");

                throw new Exception(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode, ex);
            }
        }
    }
}