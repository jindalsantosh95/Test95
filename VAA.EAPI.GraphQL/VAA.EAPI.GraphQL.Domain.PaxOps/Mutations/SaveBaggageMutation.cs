using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SaveBaggageMutation
    {
        private IBaggageServiceProvider _baggageServiceProvider;
        private ICheckInServiceProvider _checkInServiceProvider;
        private IBasketServiceProvider _basketServiceProvider;
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<SaveBaggageMutation> _logger;

        public SaveBaggageMutation(IBaggageServiceProvider baggageServiceProvider, ICheckInServiceProvider checkInServiceProvider, IBasketServiceProvider basketServiceProvider, ITelemetryService telemetryService, ILogger<SaveBaggageMutation> logger)
        {
            _baggageServiceProvider = baggageServiceProvider ?? throw new ArgumentNullException(nameof(baggageServiceProvider));
            _checkInServiceProvider = checkInServiceProvider ?? throw new ArgumentNullException(nameof(checkInServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("SaveBaggage")]
        [GraphQLDescription("Save passenger baggage inforamtion to Basket MS and to PaxOps MS.")]
        public async Task<SaveBaggageResponse> SaveBaggage(SaveBaggageRequest request, IResolverContext context)
        {

            if (request.CheckInId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(request.CheckInId)));
                return null;
            }
            foreach (var selectbaggage in request.SelectedBaggageOffer)
            {
                if (string.IsNullOrWhiteSpace(selectbaggage.BaggageOfferId))
                {
                    context.ReportError(string.Format(Application.BaggageOfferId));
                    return null;
                }
                else if (selectbaggage.TotalPrice < 0)
                {
                    context.ReportError(string.Format(Application.BaggageTotalPrice));
                    return null;
                }
            }
            request.CheckInId = await CheckInIdHelper.ExtendSessions(_checkInServiceProvider, request.CheckInId);

            bool? paidBaggageOffer = null;

            try
            {
                Basket basketResponse = null;

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.BAGGAGE_DETAILS_SUBMITTED, Data: await TelemetryHelper.BaggageDetailsSubmitted(request)));

                paidBaggageOffer = request?.SelectedBaggageOffer?.Any(price => price.TotalPrice > 0);

                var saveBaggageResponse = await _baggageServiceProvider.SaveBaggageAsync(await SaveBaggageServiceRequest.CreateRequestAsync(request));

                if (!saveBaggageResponse.TravelPartyId.IsNullOrEmpty())
                {
                    CheckInResponse checkInResponse = null;
                    Infrastructure.Models.BaggageMS.BaggageResponse getBaggageResponse = null;

                    if (paidBaggageOffer == true)
                    {
                        checkInResponse = await _checkInServiceProvider.GetCheckInResponseAsync(saveBaggageResponse.TravelPartyId);

                        getBaggageResponse = await _baggageServiceProvider.GetBaggageAsync(request.CheckInId, false);
                    }

                    basketResponse = await _basketServiceProvider.CreateBasketAsync((CreateBasketRequest)(request, checkInResponse, getBaggageResponse?.Passengers));
                }
                else
                {
                    _logger.LogError("SaveBaggage returned an empty TravelPartyId for pnr : {pnr}", await CheckInIdHelper.GetPnrLocator(request?.CheckInId));

                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.BAGGAGE_DETAILS_FAILED, Data: await TelemetryHelper.BaggageDetailsFailed(request?.CheckInId, paidBaggageOffer, LoggingFailureContext.SAVE_BAGGAGE)));

                    context.ReportError(string.Format(Application.BaggageCheckInFailDefaultMessage, Application.BaggageCheckInFailDefaultTiTle));
                    return null;
                }
                var checkInId = await CheckInIdHelper.CreateCheckInId(request.CheckInId, null, false, saveBaggageResponse?.TravelPartyId, true, basketId: basketResponse?.BasketId.ToString(), resetBasketIdExpiryTime: true);

                var baggageResponse = (SaveBaggageResponse)(checkInId);

                if (baggageResponse.Success != false)
                {
                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.BAGGAGE_DETAILS_SAVED, Data: await TelemetryHelper.BaggageDetailsSaved(checkInId, paidBaggageOffer)));
                    return baggageResponse;
                }
                else
                {
                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.BAGGAGE_DETAILS_FAILED, Data: await TelemetryHelper.BaggageDetailsFailed(checkInId, paidBaggageOffer, LoggingFailureContext.SAVE_BAGGAGE)));
                    context.ReportError(string.Format(Application.BaggageCheckInFailDefaultMessage, Application.BaggageCheckInFailDefaultTiTle));
                    return null;

                }
            }

            catch (Exception ex)
            {
                return await BaggageFailureHandler.HandleAsync<SaveBaggageResponse>(
                    context,
                    ex,
                    new BaggageFailureContext
                    {
                        CheckInId = request?.CheckInId,
                        IsPaidBaggage = paidBaggageOffer,
                        LoggingFailureContext = LoggingFailureContext.SAVE_BAGGAGE,
                        Operation = "save baggage"
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
