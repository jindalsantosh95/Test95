using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services.LoyaltyProgramDetails
{
    public class LoyaltyActionHandler : ILoyaltyActionHandler
    {
        private readonly IPassengerDetailsProvider _passengerDetailsProvider;
        private readonly ILogger<LoyaltyActionHandler> _logger;

        public LoyaltyActionHandler(IPassengerDetailsProvider passengerDetailsProvider, ILogger<LoyaltyActionHandler> logger)
        {
            _passengerDetailsProvider = passengerDetailsProvider ?? throw new ArgumentNullException(nameof(passengerDetailsProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LoyaltyOperationResult> ExecuteLoyaltyActionAsync(LoyaltyMemberDetailsRequest request, LoyaltyActionType actionType, FQTProgramRefData programDetails, IEnumerable<LoyaltyProgramAccount> existingLoyaltyProgramAccounts, CancellationToken cancellationToken = default)
        {
            var correlationId = AsyncContext.Current.CorrelationId.ToString();

            try
            {
                return actionType switch
                {
                    LoyaltyActionType.Add => await ExecuteAddActionAsync(request, programDetails, cancellationToken),
                    LoyaltyActionType.Update => await ExecuteUpdateActionAsync(request, programDetails, existingLoyaltyProgramAccounts, cancellationToken),
                    LoyaltyActionType.Delete => await ExecuteDeleteActionAsync(request, programDetails, existingLoyaltyProgramAccounts, cancellationToken),
                    _ => LoyaltyOperationResult.Failure($"Unsupported action type: {actionType}", correlationId)
                };
            }
            catch (Exception ex)
            {
                var msError = await (ex.InnerException as HttpResponseException)?.Response?.Content?.ReadAsStringAsync();
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {request.TripReservationId} |Error message - {msError}");

                throw new Exception(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode, ex);
            }
        }

        private async Task<LoyaltyOperationResult> ExecuteAddActionAsync(LoyaltyMemberDetailsRequest request, FQTProgramRefData programDetails, CancellationToken cancellationToken)
        {
            var msBookingId = await BookingIdHelper.GetMSBookingId(request.TripReservationId);
            var response = await _passengerDetailsProvider.ManagePassengerLoyaltyMemberDetails((PassengerDetails)(request, ActionCodeType.ADD, programDetails, msBookingId, null));

            return response != null
                ? LoyaltyOperationResult.Success("Success", AsyncContext.Current.CorrelationId.ToString())
                : LoyaltyOperationResult.Failure("Failed to add loyalty member details", AsyncContext.Current.CorrelationId.ToString());
        }

        private async Task<LoyaltyOperationResult> ExecuteUpdateActionAsync(LoyaltyMemberDetailsRequest request, FQTProgramRefData programDetails, IEnumerable<LoyaltyProgramAccount> existingLoyaltyProgramAccounts, CancellationToken cancellationToken)
        {
            var msBookingId = await BookingIdHelper.GetMSBookingId(request.TripReservationId);
            var deleteResponse = await _passengerDetailsProvider.ManagePassengerLoyaltyMemberDetails((PassengerDetails)(request, ActionCodeType.DELETE, programDetails, msBookingId, existingLoyaltyProgramAccounts));

            if (deleteResponse == null)
            {
                return LoyaltyOperationResult.Failure("Failed to delete existing loyalty member details", AsyncContext.Current.CorrelationId.ToString());
            }

            var addResponse = await _passengerDetailsProvider.ManagePassengerLoyaltyMemberDetails((PassengerDetails)(request, ActionCodeType.ADD, programDetails, msBookingId, null));

            return addResponse != null
                ? LoyaltyOperationResult.Success("Success", AsyncContext.Current.CorrelationId.ToString())
                : LoyaltyOperationResult.Failure("Failed to add new loyalty member details", AsyncContext.Current.CorrelationId.ToString());
        }

        private async Task<LoyaltyOperationResult> ExecuteDeleteActionAsync(LoyaltyMemberDetailsRequest request, FQTProgramRefData programDetails, IEnumerable<LoyaltyProgramAccount> existingLoyaltyProgramAccounts, CancellationToken cancellationToken)
        {
            var msBookingId = await BookingIdHelper.GetMSBookingId(request.TripReservationId);
            var response = await _passengerDetailsProvider.ManagePassengerLoyaltyMemberDetails((PassengerDetails)(request, ActionCodeType.DELETE, programDetails, msBookingId, existingLoyaltyProgramAccounts));

            return response != null
                ? LoyaltyOperationResult.Success("Success", AsyncContext.Current.CorrelationId.ToString())
                : LoyaltyOperationResult.Failure("Failed to delete loyalty member details", AsyncContext.Current.CorrelationId.ToString());
        }
    }
}
