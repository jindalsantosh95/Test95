using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Payment;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class PaymentEligibilityCheckMutation
    {
        private IBaggageServiceProvider _baggageServiceProvider;
        private ICheckInServiceProvider _checkInServiceProvider;
        private IBasketServiceProvider _basketServiceProvider;
        private IPaymentServiceProvider _paymentServiceProvider;
        private IReferenceServiceProvider _referenceServiceProvider;
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<PaymentEligibilityCheckMutation> _logger;

        public PaymentEligibilityCheckMutation(IBaggageServiceProvider baggageServiceProvider, ICheckInServiceProvider checkInServiceProvider, IBasketServiceProvider basketServiceProvider, IPaymentServiceProvider paymentServiceProvider, IReferenceServiceProvider referenceServiceProvider, ITelemetryService telemetryService, ILogger<PaymentEligibilityCheckMutation> logger)
        {
            _baggageServiceProvider = baggageServiceProvider ?? throw new ArgumentNullException(nameof(baggageServiceProvider));
            _checkInServiceProvider = checkInServiceProvider ?? throw new ArgumentNullException(nameof(checkInServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(paymentServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("PaymentEligibilityCheck")]
        [GraphQLDescription("Baggage Payment Eligibility Check during mutation.")]
        public async Task<BaggagePaymentCheckResponse> PaymentCheck(PaymentCheckRequest request, IResolverContext context)
        {
            request.CheckInId = await CheckInIdHelper.ExtendSessions(_checkInServiceProvider, request.CheckInId);

            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.BAGGAGE_PAYMENT_ELIGIBILITY_INITIATED, Data: await TelemetryHelper.PaymentEligibilityInitiation(request.CheckInId)));

            bool? isPaymentRequired = null;

            try
            {
                var basketResponse = await _basketServiceProvider.RetrieveBasketAsync(await CheckInIdHelper.GetBasketId(request.CheckInId));
                if (basketResponse.CheckedBaggageDetails == null)
                {
                    context.ReportError(string.Format(Common.Constants.Application.MissingFieldMessage, nameof(basketResponse.CheckedBaggageDetails)));
                    return null;
                }

                var totalPendingAmount = basketResponse?.CheckedBaggageDetails?.PassengerBaggage.Sum(pb => pb?.TotalPrice);

                isPaymentRequired = totalPendingAmount > 0;

                var paymentCheckResponse = await _baggageServiceProvider.PaymentEligibilityCheckAsync(await PaymentCheckServiceRequest.CreateRequestAsync(request.CheckInId, isPaymentRequired));

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.PAYMENT_ELIGIBILITY_CHECKED, Data: await TelemetryHelper.PaymentEligibilityChecked(request, basketResponse, paymentCheckResponse.TravelPartyId, isPaymentRequired)));

                if (isPaymentRequired == false)
                    return await BaggagePaymentCheckResponse.PaymentCheckResponseAsync(paymentCheckResponse, request.CheckInId, isPaymentRequired, null, null, null);

                string currencyCode = basketResponse?.CheckedBaggageDetails?.PassengerBaggage?.FirstOrDefault()?.CurrencyCode;

                var currencyNumber = (await _referenceServiceProvider.GetAir4CurrencyNumberAsync()).FirstOrDefault(x => x.CurrencyCode.Is(currencyCode));

                var ipAddress = AsyncContext.Current.Context.GetForwardedForIPAddress();
                ArgumentException.ThrowIfNullOrEmpty(ipAddress);

                var paymnetResponse = await _paymentServiceProvider.PaymentReferenceAsync((BaggagePaymentRequest)(request, basketResponse, currencyNumber.CurrencyNumber, ipAddress, totalPendingAmount));

                var paymentEligibilityCheckResponse = await BaggagePaymentCheckResponse.PaymentCheckResponseAsync(paymentCheckResponse, request.CheckInId, isPaymentRequired, paymnetResponse, totalPendingAmount, currencyCode);

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.PAYMENT_SESSION_CREATED, Data: await TelemetryHelper.PaymentSessionCreated(paymentEligibilityCheckResponse, basketResponse)));

                return paymentEligibilityCheckResponse;
            }

            catch (Exception ex)
            {
                return await BaggageFailureHandler.HandleAsync<BaggagePaymentCheckResponse>(
                    context,
                    ex,
                    new BaggageFailureContext
                    {
                        CheckInId = request?.CheckInId,
                        IsPaidBaggage = isPaymentRequired,
                        LoggingFailureContext = LoggingFailureContext.PAYMENT_ELIGIBILITY_CHECK,
                        Operation = "check baggage payment eligibility"
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
