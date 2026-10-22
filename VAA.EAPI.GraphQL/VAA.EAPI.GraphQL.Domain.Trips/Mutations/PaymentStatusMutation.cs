using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Trips.Mutations
{
    [ExtendObjectType("Mutation")]
    public class PaymentStatusMutation
    {
        private readonly IBasketProvider _basketServiceProvider;
        private readonly ILogger<PaymentStatusMutation> _logger;

        public PaymentStatusMutation(IBasketProvider basketServiceProvider, ILogger<PaymentStatusMutation> logger)
        {
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("paymentStatusMutation")]
        [GraphQLDescription("Update payment status")]
        public async Task<PaymentStatusResponse> PaymentStatus(PaymentStatusRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                ArgumentNullException.ThrowIfNull(request.TripReservationId);
                ArgumentNullException.ThrowIfNull(request.PaymentReferenceId);
                ArgumentNullException.ThrowIfNull(request.PaymentStatus);

                //get basketId and retrieve
                var basketId = await BookingIdHelper.GetBasketId(request.TripReservationId);
                var basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

                if (basket?.PaymentSessions?.Any(pri => pri.PaymentReferenceId == request.PaymentReferenceId) ?? false)
                {     
                    var updateResponse = await _basketServiceProvider.UpdatePaymentDetailsAsync((UpdateBasketPaymentRequest)(request, basketId, basket.PaymentSessions.First()));
                    
                    return new PaymentStatusResponse()
                    {
                        PaymentStatus = request.PaymentStatus
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");
                throw new Exception(Constants.ErrorCodes.SelectSeatErrorCode, ex);
            }
        }
    }
}