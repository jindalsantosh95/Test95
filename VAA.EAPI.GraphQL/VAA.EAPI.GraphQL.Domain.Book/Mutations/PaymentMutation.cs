using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Web;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Seats;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations
{
    [ExtendObjectType("Mutation")]
    public class PaymentMutation
    {
        protected readonly ILogger<PaymentMutation> _logger;
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly ISeatServiceProvider _seatServiceProvider;
        private readonly IPaymentServiceProvider _paymentServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly IDistributedCache _distributedCache;
        private readonly ICustomerProvider _customerProvider;
        private readonly IAnalyticsService _analyticsService;

        public PaymentMutation(IPaymentServiceProvider paymentServiceProvider, IOfferServiceProvider offerServiceProvider, IBasketServiceProvider basketServiceProvider, ISeatServiceProvider seatServiceProvider, ILogger<PaymentMutation> logger, IRefServiceProvider referenceServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, IDistributedCache distributedCache, ICustomerProvider customerProvider, IAnalyticsService analyticsService)
        {

            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(_paymentServiceProvider));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(_offerServiceProvider));
            _seatServiceProvider = seatServiceProvider ?? throw new ArgumentNullException(nameof(seatServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        }

        [GraphQLName("orderPaymentReferenceId")]
        [GraphQLDescription("Order Payment.")]
        public async Task<PaymentOrderResponse> OrderPayment(OrderPaymentRequest request)
        {
            string basketId = string.Empty;
            var analyticsData = new Dictionary<string, object>();

            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.BasketId))
                    throw new ArgumentException($"'{nameof(request.BasketId)}' must be specified.");

                if (request.IpAddress.IsNullOrWhiteSpace())
                    request.IpAddress = AsyncContext.Current.Context.GetForwardedForIPAddress();

                basketId = request.BasketId;

                Models.Basket.Basket basket = await _basketServiceProvider.RetrieveBasketAsync(request.BasketId);

                analyticsData["BasketId"] = basketId;
                analyticsData["BookingType"] = basket.FlightsearchRequest.isAwardSearch == true ? "Award" : "Revenue";

                #region Search Offer

                var offerId = basket.OfferIds.First();

                if (string.IsNullOrWhiteSpace(offerId))
                    throw new ArgumentException($"'{nameof(offerId)}'  must be spefied.");

                IEnumerable<AirportRefData> refAirportData = null;
                IEnumerable<AirlineRefData> refAirlineData = null;
                IEnumerable<CabinRefData> refCabinData = null;
                IEnumerable<CurrencyNumberRefData> refCurrencyNumberData = null;
                OfferResponse offer = null;

                Parallel.Invoke(
                    new Action(() => refAirportData = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                    new Action(() => refAirlineData = _referenceServiceProvider.RetrieveAirlineDataAsync().GetAwaiter().GetResult()),
                    new Action(() => refCabinData = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                    new Action(() => refCurrencyNumberData = _referenceServiceProvider.RetrieveAir4CurrencyNumberAsync().GetAwaiter().GetResult()),
                    new Action(() => offer = _offerServiceProvider.OfferRetrieveAsync(offerId).GetAwaiter().GetResult())
                );

                #endregion

                #region "Redemption & Pay With Points Validations"
                if (basket is not null && basket.FlightsearchRequest.isAwardSearch && TokenHelper.GetAuthorizationToken() == string.Empty) throw new Microsoft.IdentityModel.Tokens.SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                if (TokenHelper.GetAuthorizationToken() != string.Empty)
                {

                    var fQTNumber = TokenHelper.GetFQTNumber(_tokenB2CProvider).Result;
                    var customerFQTNumber = basket?.CustomerProfile?.MembershipDetails.FQTNumber;

                    if (!string.IsNullOrEmpty(customerFQTNumber) && !string.Equals(fQTNumber, customerFQTNumber))
                    {
                        throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                    }

                    var customer = await _customerProvider.GetCustomer(string.Empty, fQTNumber);
                    if (customer != null)
                    {
                        var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                        var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customer.Customer.DateOfBirth.ToString()), departureTime);

                        if (passengerAgeonDepartureDate < 16)
                            throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                    }
                }
                else if (basket?.CustomerProfile?.MembershipDetails?.FQTNumber.IsNullOrEmpty() == false)
                    throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                #endregion

                #region GetSeatsInformation

                var seatOfferIds = basket.FlightPassengerSeatMap?.Where(flightpassengerSeatMap => !string.IsNullOrEmpty(flightpassengerSeatMap?.SeatNumber)).Select(x => x.SeatOfferId).Distinct();

                ConcurrentBag<SeatMSResponse> seatMSResponse = new();


                if (!seatOfferIds.IsNullOrEmpty())
                {
                    await Parallel.ForEachAsync(seatOfferIds, async (id, CancellationToken) => seatMSResponse.Add(await _seatServiceProvider.RetrieveSeatOfferDetailsAsync(id)));
                    analyticsData["SeatsIncluded"] = "Yes";
                }
                #endregion

                #region Get payment details

                decimal offerAmount = Convert.ToDecimal(offer.Offers.First().SalePrice.FirstOrDefault(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount);

                if (request.AmountToPay == offerAmount)
                {
                    return OrderPaymentMapper.GetOrderPaymentResponse(offerAmount, offer, basket, refAirportData, refAirlineData, refCabinData, basket.PaymentSessions?.First());
                }

                #region "GetLoyaltyPointsToCurrencyData"
                LoyaltyPointsEquivalentCurrency selectedPayWithPointsOption = null;
                PaymentOrderResponse paymentResponse = null;
                PaymentRefIdResponse paymentMSResponse = null;
                OrderPaymentRequestMs orderPaymentRequest = null;
                bool payWithPoints = false;

                if (!request.PayWithPointsValue.IsNullOrEmpty())
                {
                    var savedPayWithPointOptions = basket.PtcWisePointsEquivalentCurrencyDetails?.FirstOrDefault(v => int.Parse(v.TotalPointsEquivalentCurrency.LoyaltyPoints) == int.Parse(request.PayWithPointsValue));

                    if (savedPayWithPointOptions != null)
                    {
                        payWithPoints = true;
                        selectedPayWithPointsOption = savedPayWithPointOptions.TotalPointsEquivalentCurrency;
                    }
                    else { payWithPoints = false; selectedPayWithPointsOption = null; }
                }
                else { payWithPoints = false; selectedPayWithPointsOption = null; }

                #endregion
                bool isInsuranceAdded = basket.IsInsuranceAdded ?? false;
                var insurancePrice = isInsuranceAdded ? basket.InsuranceQuotes.First().Products.First().TotalPrice : 0;
                bool hasPaidSeat = basket.FlightPassengerSeatMap?.Any(seat => !seat.SeatNumber.IsNullOrEmpty() && seat?.Price?.Amount != 0) ?? false;
                analyticsData["IsPaidSeatIncluded"] = hasPaidSeat ? "Yes" : "No";
                analyticsData["IsInsuranceAdded"] = isInsuranceAdded ? "Yes" : "No";

                if (!payWithPoints)
                {
                    orderPaymentRequest = OrderPaymentMapper.GetOrderPaymentRequest(request, offer, basket, refCurrencyNumberData, seatMSResponse, refCabinData, payWithPoints, selectedPayWithPointsOption, insurancePrice);
                    paymentMSResponse = await _paymentServiceProvider.OrderPaymentReferenceId(orderPaymentRequest);
                }
                else
                {
                    if (hasPaidSeat)
                    {
                        orderPaymentRequest = OrderPaymentMapper.GetOrderPaymentRequest(request, offer, basket, refCurrencyNumberData, seatMSResponse, refCabinData, payWithPoints, selectedPayWithPointsOption, insurancePrice);
                        paymentMSResponse = await _paymentServiceProvider.OrderPaymentReferenceId(orderPaymentRequest);
                    }
                    else
                    {
                        if (isInsuranceAdded)
                        {
                            orderPaymentRequest = OrderPaymentMapper.GetOrderPaymentRequest(request, offer, basket, refCurrencyNumberData, seatMSResponse, refCabinData, payWithPoints, selectedPayWithPointsOption, insurancePrice);
                            paymentMSResponse = await _paymentServiceProvider.OrderPaymentReferenceId(orderPaymentRequest);
                        }
                        else
                        {
                            var pointsForSelectedOffer = basket.PtcWisePointsEquivalentCurrencyDetails?.FirstOrDefault(x => Decimal.Parse(x.TotalPointsEquivalentCurrency?.Amount?.MonetaryAmount) == offerAmount)?.TotalPointsEquivalentCurrency?.LoyaltyPoints ?? "0";

                            if ((request?.PayWithPointsValue ?? "0").Parse<int>() != pointsForSelectedOffer.Parse<int>())
                            {
                                orderPaymentRequest = OrderPaymentMapper.GetOrderPaymentRequest(request, offer, basket, refCurrencyNumberData, seatMSResponse, refCabinData, payWithPoints, selectedPayWithPointsOption, insurancePrice);
                                paymentMSResponse = await _paymentServiceProvider.OrderPaymentReferenceId(orderPaymentRequest);
                            }
                        }
                    }
                }

                analyticsData["Selectedoffers"] = orderPaymentRequest?.Selectedoffers;
                analyticsData["PaymentAmount"] = orderPaymentRequest?.paymentAmt.CurrencyEquivalentPrice.Amount.MonetaryAmount;
                analyticsData["Currency"] = orderPaymentRequest?.paymentAmt.CurrencyEquivalentPrice.Amount.Currency;
                analyticsData["PayWithPointsValue"] = request.PayWithPointsValue;


                #region Basket update with payment details

                Models.Basket.Basket basketUpdate = new();

                Uri deltaHPPPaymentUri = paymentMSResponse != null ? new(paymentMSResponse.HPPLink) : null;
                string parentPaymentReferenceId = deltaHPPPaymentUri == null ? null : HttpUtility.ParseQueryString(deltaHPPPaymentUri.Query).Get("paymentReferenceId");
                analyticsData["PaymentReferenceId"] = parentPaymentReferenceId;
                var basketPaymentRequest = new BasketPaymentRequest
                {
                    BasketId = request.BasketId,
                    PaymentSessionsBasketMS = OrderPaymentMapper.UpdateBasketPaymentSessionRequest(new PaymentSession()
                    {
                        AmountToPay = offerAmount - Convert.ToDecimal(selectedPayWithPointsOption?.Amount.MonetaryAmount ?? "0"),
                        PaymentLink = paymentMSResponse?.HPPLink,
                        ParentPaymentReferenceId = parentPaymentReferenceId,
                        SelectedPayWithPointsOption = selectedPayWithPointsOption
                    })
                };

                basketUpdate = await _basketServiceProvider.UpdatePaymentDetailsAsync(basketPaymentRequest);

                if (basketUpdate?.PaymentSessions?.Any() ?? false)
                {
                    paymentResponse = OrderPaymentMapper.GetOrderPaymentResponse(offerAmount, offer, basketUpdate, refAirportData, refAirlineData, refCabinData, new PaymentSession() { PaymentLink = paymentMSResponse?.HPPLink });

                    if (ConfigReader.Caching.IsCacheEnabled("paymentDetailsResponse", out string key, out TimeSpan expiry, out bool isSliding, basket.BasketId.ToString()))
                    {
                        DistributedCacheEntryOptions options = isSliding ? new() { SlidingExpiration = expiry } : new() { AbsoluteExpirationRelativeToNow = expiry };
                        _distributedCache?.SetString(key, Convert.ToBase64String(Encoding.UTF8.GetBytes(Json.Serialize(paymentResponse))), new DistributedCacheEntryOptions() { SlidingExpiration = expiry });
                    }

                    return paymentResponse;
                }
                else
                {
                    throw new NotImplementedException();
                }

                #endregion
                #endregion
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                analyticsData["PaymentStatus"] = "Payment Failed-SecurityTokenException";
                throw;
            }
            catch (RequestValidationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                analyticsData["PaymentStatus"] = "Payment Failed-RequestValidationException";
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                analyticsData["PaymentStatus"] = "Payment Failed-Exception";
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Unhandled Exception in create PRI from payment mutation.")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                            .Build());
            }
            finally
            {
                //To DO - Analytics
                //_ = _analyticsServiceProvider.LogAnalyticsData((TrackAnalyticsMessage)(Type: Common.Constants.Application.TrackingAnalyticsType, Subject: "PaymentDetails", Data: analyticsData));
            }
        }
    }
}
