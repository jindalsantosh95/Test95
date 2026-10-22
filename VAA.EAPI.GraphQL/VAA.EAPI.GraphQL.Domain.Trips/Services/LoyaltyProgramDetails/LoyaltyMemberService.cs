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
    public class LoyaltyMemberService : ILoyaltyMemberService
    {
        private readonly IPassengerDetailsProvider _passengerDetailsProvider;
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly ILoyaltyActionHandler _loyaltyActionHandler;
        private readonly ILoyaltyActionStrategy _loyaltyActionStrategy;
        private readonly ILoyaltyRequestValidator _requestValidator;
        private readonly ILogger<LoyaltyMemberService> _logger;

        public LoyaltyMemberService(IPassengerDetailsProvider passengerDetailsProvider, IReferenceServiceProvider referenceServiceProvider, ILoyaltyActionHandler loyaltyActionHandler, ILoyaltyActionStrategy loyaltyActionStrategy, ILoyaltyRequestValidator requestValidator, ILogger<LoyaltyMemberService> logger)
        {
            _passengerDetailsProvider = passengerDetailsProvider ?? throw new ArgumentNullException(nameof(passengerDetailsProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _loyaltyActionHandler = loyaltyActionHandler ?? throw new ArgumentNullException(nameof(loyaltyActionHandler));
            _loyaltyActionStrategy = loyaltyActionStrategy ?? throw new ArgumentNullException(nameof(loyaltyActionStrategy));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LoyaltyMemberDetailsResponse> ProcessLoyaltyMemberDetailsAsync(LoyaltyMemberDetailsRequest request, CancellationToken cancellationToken = default)
        {
            var correlationId = AsyncContext.Current.CorrelationId.ToString();

            var validationResult = _requestValidator.ValidateRequest(request);
            if (!validationResult.IsValid)
            {
                throw new Exception(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode, new(validationResult.ErrorMessage));
            }

            var passengerDetails = await GetPassengerDetailsAsync(request.TripReservationId, cancellationToken);
            var requestedPasengerDetails = passengerDetails?.PassengerInformation?.Passengers
            ?.FirstOrDefault(p => p.PassengerResourceId == request.PassengerId);

            if (requestedPasengerDetails == null)
            {
                throw new Exception(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode);
            }

            var actionType = await _loyaltyActionStrategy.DetermineAction(request, passengerDetails);

            if (actionType == LoyaltyActionType.NoChange)
            {
                throw new Exception(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode);
            }

            var programDetails = await GetLoyaltyProgramDetailsAsync(request.LoyaltyProgramCode, cancellationToken);
            if (programDetails == null)
            {
                throw new Exception(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode);
            }

            var operationResult = await _loyaltyActionHandler.ExecuteLoyaltyActionAsync(request, actionType, programDetails, requestedPasengerDetails.LoyaltyProgramAccounts, cancellationToken);

            return operationResult.IsSuccess
                ? CreateSuccessResponse(operationResult.Status)
                : throw new Exception(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode);

        }

        private async Task<PassengerDetails> GetPassengerDetailsAsync(string tripReservationId, CancellationToken cancellationToken)
        {
            try
            {
                var msBookingId = await BookingIdHelper.GetMSBookingId(tripReservationId);
                return await _passengerDetailsProvider.GetPassengerDetails(msBookingId);
            }
            catch (Exception ex)
            {
                var msError = await (ex.InnerException as HttpResponseException)?.Response?.Content?.ReadAsStringAsync(cancellationToken);
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {tripReservationId} |Error message - {msError}");

                throw new Exception(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode, ex);
            }
        }

        private async Task<FQTProgramRefData> GetLoyaltyProgramDetailsAsync(string loyaltyProgramCode, CancellationToken cancellationToken)
        {
            try
            {
                var loyaltyPrograms = await _referenceServiceProvider.RetrieveLoyaltyProgramsDataAsync();
                return loyaltyPrograms?.FirstOrDefault(lp => lp.ProgramCode == loyaltyProgramCode);
            }
            catch (Exception ex)
            {
                var msError = await (ex.InnerException as HttpResponseException)?.Response?.Content?.ReadAsStringAsync(cancellationToken);
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} |Error message - {msError}");

                throw new Exception(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode, ex);
            }
        }

        private static LoyaltyMemberDetailsResponse CreateSuccessResponse(string message) =>
            new() { Message = message };
    }
}
