using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.PaymentDetails;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class ConfirmBaggageMutation
    {
        private ICheckInServiceProvider _checkInServiceProvider;
        private IPaymentServiceProvider _paymentServiceProvider;
        private IBaggageServiceProvider _baggageServiceProvider;
        private IBasketServiceProvider _basketServiceProvider;
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<ConfirmBaggageMutation> _logger;

        public ConfirmBaggageMutation(IBaggageServiceProvider baggageServiceProvider, ICheckInServiceProvider checkInServiceProvider, IBasketServiceProvider basketServiceProvider, IPaymentServiceProvider paymentServiceProvider, ITelemetryService telemetryService, ILogger<ConfirmBaggageMutation> logger)
        {
            _checkInServiceProvider = checkInServiceProvider ?? throw new ArgumentNullException(nameof(checkInServiceProvider));
            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(paymentServiceProvider));
            _baggageServiceProvider = baggageServiceProvider ?? throw new ArgumentNullException(nameof(baggageServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));    
        }

        [GraphQLName("ConfirmBaggage")]
        [GraphQLDescription("Confirming the Baggage payment details back to the caller")]
        public async Task<ConfirmBaggageResponse> ConfirmBaggage(ConfirmBaggageRequest confirmBaggageRequest, IResolverContext context)
        {
            string checkInId = confirmBaggageRequest.CheckInId;
            string basketId = null;

            if (checkInId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(checkInId)));
                return null;
            }

            if (confirmBaggageRequest.PaymentStatus.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(confirmBaggageRequest.PaymentStatus)));
                return null;
            }

            if (confirmBaggageRequest.PaymentReferenceId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(confirmBaggageRequest.PaymentReferenceId)));
                return null;
            }

            try
            {
                checkInId = await CheckInIdHelper.ExtendSessions(_checkInServiceProvider, checkInId);

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CONFIRM_BAGGAGE_PAYMENT_TRANSACTION_SUBMITTED, Data: await TelemetryHelper.PaymentTransactionDetails(confirmBaggageRequest, PaymentType.Card.ToString())));

                basketId = await CheckInIdHelper.GetBasketId(checkInId);

                if (confirmBaggageRequest.PaymentStatus.Is(BasketStatusEnum.FAILED.ToString()))
                {
                    if (basketId != null)
                    {
                        await _basketServiceProvider.UpdatePaymentDetailsAsync((UpdateBasketPaymentRequest)(basketId, confirmBaggageRequest.PaymentStatus.ToString()));
                    }

                    _logger.LogError("ConfirmBaggage payment failed for pnr : {pnr}, basketId: {basketId}, paymentReferenceId: {paymentReferenceId}, paymentStatus: {paymentStatus}",
                        await CheckInIdHelper.GetPnrLocator(checkInId),
                        basketId,
                        confirmBaggageRequest?.PaymentReferenceId,
                        confirmBaggageRequest?.PaymentStatus);

                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.BAGGAGE_DETAILS_FAILED, Data: await TelemetryHelper.BaggageDetailsFailed(checkInId, true, LoggingFailureContext.CONFIRM_BAGGAGE)));

                    context.ReportError(ErrorBuilder.New().SetMessage(Application.OnlieCheckInFailDefaultMessage).SetExtension("title", Application.OnlieCheckInFailDefaultTiTle).SetExtension("code", "500").Build());

                    return null;
                }

                PaymentTransactionDetails paymentTransactionDetails = await _paymentServiceProvider.PaymentDetailsAsync(confirmBaggageRequest?.PaymentReferenceId, PaymentType.Card.ToString());

                string pnrLocator = await CheckInIdHelper.GetPnrLocator(checkInId);

                var basket = basketId != null ? await _basketServiceProvider.RetrieveBasketAsync(basketId) : null;

                var travelPartyId = await CheckInIdHelper.GetTravelPartyId(checkInId);

                var reviewPayDetails = await _baggageServiceProvider.ReviewPaymentDetails((ReviewPayDetailsRequest)(paymentTransactionDetails, travelPartyId, basket?.ContactDetails?.EmailAddress, pnrLocator));

                var isAmountCaptured = decimal.TryParse(reviewPayDetails?.TotalPaidAmount?.ToString(), out var paidAmount) && paidAmount > 0;

                var finalStatus = isAmountCaptured ? BasketStatusEnum.COMPLETED.ToString() : BasketStatusEnum.FAILED.ToString();

                await _basketServiceProvider.UpdateBasketOrderStatusAsync(basketId, finalStatus);

                var newCheckInId = await CheckInIdHelper.CreateCheckInId(checkInId, null, false, travelPartyId, true);

                ConfirmBaggageResponse confirmBaggageResponse = (ConfirmBaggageResponse)(reviewPayDetails, newCheckInId, travelPartyId, basket);

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CONFIRM_BAGGAGE_PAYMENT_REVIEW_SUBMITTED, Data: await TelemetryHelper.BaggageReviewPayment(confirmBaggageResponse, basket, newCheckInId)));

                return confirmBaggageResponse;
            }

            catch (HttpRequestException ex)
            {
                if (basketId != null)
                {
                    await _basketServiceProvider.UpdateBasketOrderStatusAsync(basketId, BasketStatusEnum.FAILED.ToString());
                }

                return await BaggageFailureHandler.HandleAsync<ConfirmBaggageResponse>(
                    context,
                    ex,
                    new BaggageFailureContext
                    {
                        CheckInId = confirmBaggageRequest.CheckInId,
                        IsPaidBaggage = true,
                        LoggingFailureContext = LoggingFailureContext.CONFIRM_BAGGAGE,
                        Operation = "confirm baggage payment"
                    },
                    _telemetryService,
                    _logger);
            }

            catch (Exception ex)
            {
                return await BaggageFailureHandler.HandleAsync<ConfirmBaggageResponse>(
                    context,
                    ex,
                    new BaggageFailureContext
                    {
                        CheckInId = confirmBaggageRequest.CheckInId,
                        IsPaidBaggage = true,
                        LoggingFailureContext = LoggingFailureContext.CONFIRM_BAGGAGE,
                        Operation = "confirm baggage payment"
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
