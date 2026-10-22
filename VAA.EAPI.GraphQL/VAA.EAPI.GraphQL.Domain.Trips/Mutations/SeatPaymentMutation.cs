using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SeatPaymentMutation
    {
        private readonly IBasketProvider _basketServiceProvider;
        private readonly IPaymentServiceProvider _paymentServiceProvider;
        private readonly ILogger<SeatPaymentMutation> _logger;
        private readonly ISeatProvider _seatServiceProvider;
        private readonly IReferenceServiceProvider _referenceServiceProvider;

        public SeatPaymentMutation(IBasketProvider basketServiceProvider, IPaymentServiceProvider paymentServiceProvider, ILogger<SeatPaymentMutation> logger, ISeatProvider seatServiceProvider, IReferenceServiceProvider referenceServiceProvider)
        {
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(paymentServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _seatServiceProvider = seatServiceProvider ?? throw new ArgumentNullException(nameof(seatServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
        }

        [GraphQLName("seatPaymentMutation")]
        [GraphQLDescription("get total amount and PRI for selected seats across all segments")]
        public async Task<PaymentResponse> SeatPayment(PaymentRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                ArgumentNullException.ThrowIfNull(request.TripReservationId);
                request.IpAddress ??= HeaderHelper.GetIPAddress();
                ArgumentException.ThrowIfNullOrEmpty(request.IpAddress);

                Basket basket = null;
                var basketId = await BookingIdHelper.GetBasketId(request.TripReservationId);

                if (request.EmailAddress?.Trim()?.IsNullOrEmpty() == false)
                {
                    basket = await _basketServiceProvider.UpdateContactAsync(new ContactUpdateRequest
                    {
                        BasketId = basketId,
                        ContactDetails = new ContactDetails
                        {
                            EmailAddress = request.EmailAddress
                        }
                    });
                }
                else
                    basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

                ArgumentException.ThrowIfNullOrEmpty(basket.BookingDetails.Pos.CurrencyCode);

                IEnumerable<CabinRefData> cabinDetails = null;
                IEnumerable<CurrencyNumberRefData> currencyNumberData = null;

                Parallel.Invoke(
                     new Action(() => cabinDetails = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                    new Action(() => currencyNumberData = _referenceServiceProvider.RetrieveAir4CurrencyNumberAsync().GetAwaiter().GetResult())
                );

                ArgumentNullException.ThrowIfNull(currencyNumberData.FirstOrDefault(i => i.CurrencyCode == basket.BookingDetails.Pos.CurrencyCode));

                if (!basket.BookingDetails.IsNullOrDefault() && !basket.FlightPassengerSeatMap.IsNullOrDefault() && basket.FlightPassengerSeatMap.Any(seat => !string.IsNullOrEmpty(seat.SeatNumber) && seat.Price.Amount > 0.0m) && (basket.PaymentSessions.IsNullOrEmpty() || !basket.PaymentSessions.Any(s => s?.HppPaymentStatus == "SUCCESS")))
                {
                    var requestedSeats = basket.FlightPassengerSeatMap.Where(f => !f.SeatNumber.IsNullOrEmpty());
                    var totalAmount = requestedSeats.Sum(a => a.Price.Amount);

                    if (totalAmount > 0)
                    {
                        #region GetSeatsInformation

                        var seatOfferIds = requestedSeats.Select(x => x.SeatOfferId).Distinct();

                        ConcurrentBag<SeatResponse> seatMSResponse = new();

                        if (!seatOfferIds.IsNullOrEmpty())
                        {
                            await Parallel.ForEachAsync(seatOfferIds, async (id, CancellationToken) => seatMSResponse.Add(await _seatServiceProvider.RetrieveSeatOfferDetailsAsync(id)));
                        }
                        #endregion

                        //Create Payment Request for calling Payment to generate PRI
                        var paymentRequest = (PaymentRequestMs)(basket, request, seatMSResponse, cabinDetails, currencyNumberData);

                        var paymentRefResponse = await _paymentServiceProvider.PaymentReferenceId(paymentRequest);

                        //Update HPP Link and PaymentReferenceId in PaymentSession
                        basket = await _basketServiceProvider.UpdatePaymentDetailsAsync((UpdateBasketPaymentRequest)(null, basketId, (PaymentSession)(paymentRefResponse, paymentRequest)));

                        return (PaymentResponse)(basket, paymentRefResponse);
                    }
                }

                return (PaymentResponse)(basket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ErrorHelpers.AggregateException(ex)}");
                throw new Exception(Constants.ErrorCodes.SelectSeatErrorCode, ex);
            }
            
        }
    }
}