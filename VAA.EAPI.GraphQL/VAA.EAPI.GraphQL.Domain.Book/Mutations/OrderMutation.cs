using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;
using VAA.EAPI.GraphQL.Domain.Book.Models.CustomerEnrollment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Seats;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations
{
    [ExtendObjectType("Mutation")]
    public class OrderMutation
    {

        protected readonly ILogger<OrderMutation> _logger;
        private readonly IOrderMutationProvider _orderCreateProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IPaymentServiceProvider _paymentServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IDistributedCache _distributedCache;
        private readonly ISeatServiceProvider _seatServiceProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly ICustomerServiceProvider _customerEnrolmentProvider;
        private readonly IAnalyticsService _analyticsService;
        private readonly IVoucherRedemptionProvider _voucherRedemptionProvider;


        public OrderMutation(IOrderMutationProvider orderCreateProvider, IOfferServiceProvider offerServiceProvider,
            IBasketServiceProvider basketServiceProvider, IPaymentServiceProvider paymentServiceProvider,
            ILogger<OrderMutation> logger, IRefServiceProvider referenceServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, IDistributedCache distributedCache, ISeatServiceProvider seatServiceProvider, ICustomerProvider customerProvider, ICustomerServiceProvider customerEnrolmentProvider, IAnalyticsService analyticsService, IVoucherRedemptionProvider voucherRedemptionProvider)
        {
            _orderCreateProvider = orderCreateProvider ?? throw new ArgumentNullException(nameof(orderCreateProvider));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(paymentServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _seatServiceProvider = seatServiceProvider ?? throw new ArgumentNullException(nameof(seatServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerEnrolmentProvider = customerEnrolmentProvider ?? throw new ArgumentNullException(nameof(customerEnrolmentProvider));
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _voucherRedemptionProvider = voucherRedemptionProvider ?? throw new ArgumentNullException(nameof(voucherRedemptionProvider));
        }

        [GraphQLName("bookCreateOrder")]
        [GraphQLDescription("Create Order.")]
        public async Task<OrderCreateGraphQLResponse> CreateOrder(OrderCreateGraphQLRequest request)
        {
            bool orderCreated = false;
            bool orderCaptured = false;
            bool orderVoid = false;
            bool captureFailed = false;
            bool createOrderSubmitted = false;
            string basketId = string.Empty;
            string paymentReferenceIdInSession = string.Empty;
            string fcNumber = string.Empty;
            VoucherRedemptionContext voucherRedemption = null;
            // A callback can fail after VRed has accepted the redemption.  Keep the
            // accepted ids in a case-insensitive set so the compensating rollback is
            // attempted once for every accepted voucher.
            var redeemedVoucherIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var analyticsData = new Dictionary<string, object>();
            var voucherRedemptionEnabled = ConfigReader.Try<bool>(Constants.Application.VoucherRedemptionFeatureFlag, false);

            try
            {

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                basketId = request.BasketId;

                if (string.IsNullOrWhiteSpace(request.BasketId) || string.IsNullOrWhiteSpace(request.PaymentStatus))
                    throw new ArgumentException($"'{nameof(request.BasketId)}' And {nameof(request.PaymentStatus)} must be specified.");

                if (request.IpAddress.IsNullOrWhiteSpace())
                    request.IpAddress = AsyncContext.Current.Context.GetForwardedForIPAddress();

                #region "Read Cache for OrderData"
                //Return Order Data
                var orderData = await RefreshOrder(basketId);
                if (orderData.Key && orderData.Value == null)
                {
                    throw new GraphQLException(ErrorBuilder.New()
                               .SetMessage("Error Occured while getting your order details.")
                               .SetExtension("Exception", "Error Occured while getting your order details")
                               .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                               .Build());
                }
                else if (orderData.Key && orderData.Value != null)
                {
                    analyticsData["BasketId"] = basketId;
                    analyticsData["Refresh"] = true;
                    return orderData.Value;
                }
                #endregion

                Models.Basket.Basket basket = await _basketServiceProvider.RetrieveBasketAsync(request.BasketId);
                var summary = GetFlightSummary(basket);
                VoucherRedemptionFeatureHelper.HydrateBasketFromPersistedSummary(basket, summary);

                #region UnMask AutoMatch FcNumber

                if (basket?.FcAutoMatched == true && basket?.ContactDetails?.FcAutoMatchStamping == true)
                {
                    var profiles = await _customerEnrolmentProvider.GetCustomerDetailsByEmail(new() { Email = basket?.ContactDetails?.EmailAddress });
                    if (profiles != null && basket.People.Any())
                    {
                        var firstPerson = basket?.People?.First(x => x.Id == "0");
                        if (firstPerson?.MembershipDetails != null)
                        {
                            firstPerson.MembershipDetails.FQTNumber = profiles?.First().LoyaltyMemberNumber;
                            firstPerson.MembershipDetails.FQTProgramme = Constants.Application.VSFQTProgramme;
                        }
                    }
                }
                #endregion

                analyticsData["BasketId"] = basketId;
                analyticsData["Tier"] = basket.CustomerProfile?.MembershipDetails?.FQTTierCode;
                analyticsData["BookingType"] = basket.FlightsearchRequest.isAwardSearch == true ? "Award" : "Revenue";

                #region Search Offer & Reference data

                var offerId = basket.OfferIds.First();

                if (string.IsNullOrWhiteSpace(offerId))
                    throw new ArgumentException($"'{nameof(offerId)}'  must be spefied.");

                IEnumerable<AirportRefData> refAirportData = null;
                IEnumerable<AirlineRefData> refAirlineData = null;
                IEnumerable<CabinRefData> refCabinData = null;
                IEnumerable<AircraftRefData> refAircraftData = null;
                OfferResponse offer = null;

                Parallel.Invoke(
                    new Action(() => refAirportData = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                    new Action(() => refAirlineData = _referenceServiceProvider.RetrieveAirlineDataAsync().GetAwaiter().GetResult()),
                    new Action(() => refCabinData = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                    new Action(() => refAircraftData = _referenceServiceProvider.RetrieveAircraftDataAsync().GetAwaiter().GetResult()),
                    new Action(() => offer = _offerServiceProvider.OfferRetrieveAsync(offerId).GetAwaiter().GetResult()));

                #endregion

                #region "Redemption Validation"
                if (basket is not null && basket.FlightsearchRequest.isAwardSearch && TokenHelper.GetAuthorizationToken() == string.Empty) throw new Microsoft.IdentityModel.Tokens.SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                if (TokenHelper.GetAuthorizationToken() != string.Empty)
                {
                    fcNumber = await TokenHelper.GetFQTNumber(_tokenB2CProvider);
                    if (fcNumber != string.Empty)
                    {
                        if (basket.CustomerProfile != null && !basket.CustomerProfile.MembershipDetails.FQTNumber.Equals(fcNumber)) throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                    }

                    var customer = await _customerProvider.GetCustomer(string.Empty, fcNumber);
                    if (customer != null)
                    {
                        HydrateBasketCustomerProfile(basket, customer.Customer, fcNumber);

                        var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                        var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customer.Customer.DateOfBirth.ToString()), departureTime);

                        if (passengerAgeonDepartureDate < 16)
                            throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                    }
                }
                else if (basket?.CustomerProfile?.MembershipDetails?.FQTNumber.IsNullOrEmpty() == false)
                    throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                #endregion

                try
                {
                    voucherRedemption = voucherRedemptionEnabled && IsVoucherApplied(basket, summary)
                        ? GetVoucherRedemptionContext(basket, summary)
                        : null;
                }
                catch (ArgumentException ex)
                {
                    throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage(ex.Message)
                        .SetCode(((int)HttpStatusCode.BadRequest).ToString())
                        .Build());
                }

                #region GetSeatsInformation

                var seatOfferIds = basket.FlightPassengerSeatMap?.Where(flightpassengerSeatMap => !string.IsNullOrEmpty(flightpassengerSeatMap?.SeatNumber)).Select(x => x.SeatOfferId).Distinct();

                ConcurrentBag<SeatMSResponse> seatMSResponse = new();

                if (!seatOfferIds.IsNullOrEmpty())
                {
                    await Parallel.ForEachAsync(seatOfferIds, async (id, CancellationToken) => seatMSResponse.Add(await _seatServiceProvider.RetrieveSeatOfferDetailsAsync(id)));
                    analyticsData["SeatsIncluded"] = "Yes";

                }
                #endregion

                OrderResponseMS response = null;
                OrderCreateRequestMS createOrder = null;
                PaymentResponse payment = null;
                string paymentReferenceId = null;
                OrderCreateGraphQLResponse orderCreateResponse = null;

                #region "Get Payment Details"

                var basketPaymentSession = basket.PaymentSessions?.First();
                paymentReferenceIdInSession = basketPaymentSession.PaymentReferenceId;
                if (!string.IsNullOrEmpty(paymentReferenceIdInSession))
                {
                    payment = await _paymentServiceProvider.PaymentDetailsAsync(basketPaymentSession.PaymentReferenceId, PaymentType.APM.ToDisplayName());
                    paymentReferenceId = basketPaymentSession.PaymentReferenceId;
                }
                else
                {
                    string deltaHPPPaymentLink = basketPaymentSession?.PaymentLink;
                    if (!deltaHPPPaymentLink.IsNullOrEmpty())
                    {
                        Uri deltaHPPPaymentUri = new Uri(deltaHPPPaymentLink);
                        paymentReferenceId = HttpUtility.ParseQueryString(deltaHPPPaymentUri.Query).Get("paymentReferenceId");

                        payment = await _paymentServiceProvider.PaymentDetailsAsync(paymentReferenceId, PaymentType.Card.ToDisplayName());

                    }
                }
                analyticsData["PaymentReferenceId"] = paymentReferenceId;
                #endregion


                #region "Create Order"

                createOrder = CreateOrderMapper.GetOrderCreateRequest(offer, basket, payment, request, paymentReferenceId, seatMSResponse, refAirportData);

                analyticsData["PaymentMethod"] = createOrder.PaymentDetails.FirstOrDefault().FormOfPayments.ToString();
                analyticsData["PaymentAmount"] = createOrder.PaymentDetails?.FirstOrDefault()?.Amount;
                analyticsData["InsuranceIncluded"] = createOrder.IsInsuranceAdded;
                analyticsData["POS"] = createOrder.Pos;
                analyticsData["PaxCount"] = createOrder.CustomerDetails.Count();
                analyticsData["AdultCount"] = createOrder.CustomerDetails.Count(a => a.Ptc == Constants.Application.DeltaPtcAdult);
                analyticsData["TeenCount"] = createOrder.CustomerDetails.Count(a => a.Ptc == Constants.Application.DeltaPtcTeen);
                analyticsData["ChildCount"] = createOrder.CustomerDetails.Count(a => a.Ptc == Constants.Application.DeltaPtcChild);
                analyticsData["InfantCount"] = createOrder.CustomerDetails.Count(a => a.Ptc == Constants.Application.DeltaPtcInfant);
                analyticsData["SelectedOffers"] = createOrder.SelectedOffers;

                basket = await _basketServiceProvider.UpdateBasketOrderStatusAsync(request.BasketId, BasketStatusEnum.PROCESSING.ToDisplayName());

                if (voucherRedemption is not null)
                {
                    foreach (var voucher in voucherRedemption.Vouchers)
                    {
                        await _voucherRedemptionProvider.RedeemAsync(CreateRedemptionRequest(voucherRedemption, voucher));
                        redeemedVoucherIds.Add(voucher.VoucherId);
                    }

                    analyticsData["VoucherRedemption"] = "Redeemed";
                }

                createOrderSubmitted = true;
                response = await _orderCreateProvider.CreateOrder(createOrder);
                #endregion

                orderCreated = !response.IsNullOrDefault();

                if (!response.IsNullOrDefault())
                {
                    analyticsData["OrderCreated"] = "Yes";

                    orderCreateResponse = CreateOrderMapper.GetOrderCreateResponse(response, offer, basket, refAirportData, refAirlineData, refCabinData, seatMSResponse ?? null, refAircraftData);

                    if (voucherRedemption is not null)
                    {
                        try
                        {
                            await _voucherRedemptionProvider.UpdatePnrAsync(new VoucherPnrUpdateRequest
                            {
                                CustomerId = voucherRedemption.CustomerId,
                                MemberNumber = voucherRedemption.MemberNumber,
                                InitiatedBy = voucherRedemption.InitiatedBy,
                                VoucherNumbers = voucherRedemption.Vouchers.Select(voucher => voucher.VoucherId).ToArray(),
                                Pnr = response.BookingReference?.RLOC
                            });
                            analyticsData["VoucherPnrUpdate"] = "Accepted";
                        }
                        catch (Exception ex)
                        {
                            analyticsData["VoucherPnrUpdate"] = "Failed";
                            _logger.LogError(ex, "Correlation Id - {CorrelationId} :: Voucher PNR update could not be accepted after order creation.", AsyncContext.Current.CorrelationId);
                        }
                    }

                    if (orderCreated && !string.IsNullOrEmpty(paymentReferenceIdInSession))
                    {
                        var captureResponse = await _paymentServiceProvider.PaymentCaptureAsync(new Models.Payment.PaymentCaputreRequest()
                        {
                            PaymentReferenceId = basketPaymentSession.PaymentReferenceId,
                            VendorId = ConfigReader.Get(Constants.Environment.PaymentCaptureMSVendorId),
                            PaymentType = Models.Common.Enums.PaymentType.APM,
                            RLOC = response?.BookingReference?.RLOC,
                            TicketNumber = response?.OrderItems.First().AccountableDocumentRefs.First().DocumentNumber,
                        });

                        if (!captureResponse.IsNullOrDefault())            // IMLEMENT captureResponse error handling
                        {
                            orderCaptured = true;
                        }
                        else
                        {
                            captureFailed = true;
                        }
                    }
                }
                else
                {
                    await RollbackVouchersAsync(voucherRedemption, redeemedVoucherIds, orderCreated, analyticsData);
                    if (!string.IsNullOrEmpty(paymentReferenceIdInSession))
                    {
                        await _paymentServiceProvider.PaymentVoidAsync(new Models.Payment.OrderPaymentVoidRequest() { PaymentReferenceId = basketPaymentSession.PaymentReferenceId });
                        orderVoid = true;
                    }
                    await _basketServiceProvider.DeleteBasketAsync(request.BasketId);
                    return CreateOrderMapper.OrderCreateErrorMapper(Constants.Application.APMVoidErrorCode, string.Empty);
                }

                if (captureFailed)
                {
                    await _basketServiceProvider.UpdateBasketOrderStatusAsync(request.BasketId, BasketStatusEnum.RETRY.ToDisplayName());
                    return CreateOrderMapper.OrderCreateErrorMapper(Constants.Application.APMCaptureErrorCode, string.Empty);
                }

                if (orderCreated)
                {

                    if (ConfigReader.Caching.IsCacheEnabled("orderResponse", out string key, out TimeSpan expiry, out bool isSliding, basket.BasketId.ToString()))
                    {
                        DistributedCacheEntryOptions options = isSliding ? new() { SlidingExpiration = expiry } : new() { AbsoluteExpirationRelativeToNow = expiry };

                        _distributedCache?.SetString(key, Convert.ToBase64String(Encoding.UTF8.GetBytes(Json.Serialize(orderCreateResponse))), options);
                    }

                    await _basketServiceProvider.DeleteBasketAsync(request.BasketId);
                }

                analyticsData["BookingReference"] = response.BookingReference?.RLOC;
                return orderCreateResponse;
            }
            catch (HttpRequestException ex)
            {
                if (ex is VoucherRedemptionAcceptedException acceptedRedemption &&
                    !acceptedRedemption.VoucherId.IsNullOrEmpty())
                {
                    redeemedVoucherIds.Add(acceptedRedemption.VoucherId);
                }

                if (voucherRedemptionEnabled && (!createOrderSubmitted || IsConfirmedOrderRejection(ex)))
                {
                    await RollbackVouchersAsync(voucherRedemption, redeemedVoucherIds, orderCreated, analyticsData);
                }
                if (!orderCreated && (redeemedVoucherIds.Count > 0 || createOrderSubmitted))
                    await _basketServiceProvider.UpdateBasketOrderStatusAsync(request.BasketId, BasketStatusEnum.RETRY.ToDisplayName());
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");

                if (ex.InnerException is HttpResponseException responseException && responseException.Response?.RequestMessage?.RequestUri?.OriginalString?.Contains("order/response") == true)
                {
                    var apiErrorResponse = Json.Deserialize<OrderErrorResponse>(await ex.ReadResponseContentAsStringAsync());
                    var errorDetails = Json.Deserialize<IEnumerable<ErrorDetails>>(apiErrorResponse.Error);

                    if (!errorDetails.IsNullOrEmpty() && !orderCreated && errorDetails.FirstOrDefault().Code == Constants.Application.DeltaErrorCode)
                    {
                        await _basketServiceProvider.UpdateBasketOrderStatusAsync(request.BasketId, BasketStatusEnum.RETRY.ToDisplayName());

                        var deltaError = Json.Deserialize<ErrorDetails>(errorDetails.FirstOrDefault().Message);
                        if (deltaError != null && deltaError.Code == Constants.Application.FraudCheckDeltaErrorCode)
                        {
                            analyticsData["OrderConfirmationStatus"] = "Confirmation Failed-HttpRequestException-FraudCheck";
                            return CreateOrderMapper.OrderCreateErrorMapper(Constants.Application.FraudCheckErrorCode, Constants.Application.FraudCheckErrorMessage);
                        }

                    }
                }

                if (!string.IsNullOrEmpty(paymentReferenceIdInSession))
                {
                    if (!orderCreated)
                    {
                        try
                        {
                            var voidResponse = await _paymentServiceProvider.PaymentVoidAsync(new Models.Payment.OrderPaymentVoidRequest() { PaymentReferenceId = paymentReferenceIdInSession });
                            orderVoid = true;
                            analyticsData["OrderConfirmationStatus"] = "Confirmation Failed-HttpRequestException-VoidTransaction";

                            return CreateOrderMapper.OrderCreateErrorMapper(Constants.Application.APMVoidErrorCode, string.Empty);
                        }
                        catch (Exception voidEx)
                        {
                            analyticsData["OrderConfirmationStatus"] = "Confirmation Failed-HttpRequestException-VoidTransactionFailed";
                            _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {voidEx}");
                            throw;
                        }
                    }
                    if (!orderCaptured)
                    {
                        analyticsData["OrderConfirmationStatus"] = "Confirmation Failed-HttpRequestException-CaptureTransactionFailed";
                        return CreateOrderMapper.OrderCreateErrorMapper(Constants.Application.APMCaptureErrorCode, string.Empty);
                    }
                }

                analyticsData["OrderConfirmationStatus"] = "Confirmation Failed-HttpRequestException";
                throw;
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
            {
                await RollbackVouchersAsync(voucherRedemption, redeemedVoucherIds, orderCreated, analyticsData);
                analyticsData["OrderConfirmationStatus"] = "Confirmation Failed-SecurityTokenException";
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                await RollbackVouchersAsync(voucherRedemption, redeemedVoucherIds, orderCreated, analyticsData);
                if (!orderCreated && redeemedVoucherIds.Count > 0)
                    await _basketServiceProvider.UpdateBasketOrderStatusAsync(request.BasketId, BasketStatusEnum.RETRY.ToDisplayName());
                analyticsData["OrderConfirmationStatus"] = "Confirmation Failed-Exception";
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw;
            }
            finally
            {
                _ = _analyticsService.TrackAnalytics((TrackAnalyticsMessage)(Type: Common.Constants.Application.TrackingAnalyticsType, Subject: "OrderConfirmation", Data: analyticsData));
            }
        }

        private async Task RollbackVouchersAsync(VoucherRedemptionContext voucherRedemption, IReadOnlyCollection<string> redeemedVoucherIds, bool orderCreated, Dictionary<string, object> analyticsData)
        {
            if (voucherRedemption is null || orderCreated || redeemedVoucherIds.Count == 0)
                return;

            var rollbackFailures = new List<string>();
            foreach (var voucherId in redeemedVoucherIds)
            {
                try
                {
                    await _voucherRedemptionProvider.RollbackAsync(new VoucherRollbackRequest
                    {
                        CustomerId = voucherRedemption.CustomerId,
                        MemberNumber = voucherRedemption.MemberNumber,
                        RollbackInitiatedBy = voucherRedemption.InitiatedBy,
                        VoucherNumber = voucherId,
                        RollbackSource = "WEBSITE"
                    });
                }
                catch (Exception rollbackException)
                {
                    rollbackFailures.Add(voucherId);
                    _logger.LogError(rollbackException, "Correlation Id - {CorrelationId} :: Voucher redemption rollback failed for voucher {VoucherId}.", AsyncContext.Current.CorrelationId, voucherId);
                }
            }

            analyticsData["VoucherRollback"] = rollbackFailures.Count == 0 ? "Succeeded" : $"Failed: {string.Join(',', rollbackFailures)}";
        }

        private static void HydrateBasketCustomerProfile(Models.Basket.Basket basket, VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS.CustomerCustMS customer, string fcNumber)
        {
            if (basket is null || customer is null)
                return;

            basket.CustomerProfile ??= new();
            basket.CustomerProfile.MembershipDetails ??= new();

            if (basket.CustomerProfile.FirstName.IsNullOrWhiteSpace())
                basket.CustomerProfile.FirstName = customer.FirstName;

            if (basket.CustomerProfile.LastName.IsNullOrWhiteSpace())
                basket.CustomerProfile.LastName = customer.LastName;

            if (basket.CustomerProfile.CustomerId.IsNullOrWhiteSpace())
                basket.CustomerProfile.CustomerId = customer.CustomerId;

            if (basket.CustomerProfile.MembershipDetails.FQTNumber.IsNullOrWhiteSpace())
                basket.CustomerProfile.MembershipDetails.FQTNumber = fcNumber;

            if (basket.CustomerProfile.MembershipDetails.FQTProgramme.IsNullOrWhiteSpace())
                basket.CustomerProfile.MembershipDetails.FQTProgramme = Constants.Application.VSFQTProgramme;
        }

        private static bool IsConfirmedOrderRejection(HttpRequestException exception)
        {
            if (exception?.InnerException is not HttpResponseException responseException)
                return false;

            var response = responseException.Response;
            var statusCode = response?.StatusCode;
            return response?.RequestMessage?.RequestUri?.OriginalString?.Contains("order/response", StringComparison.OrdinalIgnoreCase) == true &&
                   statusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError &&
                   statusCode is not HttpStatusCode.RequestTimeout and not HttpStatusCode.TooManyRequests;
        }

        private static VoucherRedemptionContext GetVoucherRedemptionContext(
            Models.Basket.Basket basket,
            Models.FlightSummary.Summary? summary = null)
        {
            summary ??= GetFlightSummary(basket);
            if (!IsVoucherApplied(basket, summary))
                return null;

            // New baskets persist the explicit choice at the Basket root.  Read the
            // serialized summary as a backwards-compatible fallback for baskets
            // created before that field was propagated from Search.
            var selection = basket?.VoucherRedemptionSelection
                ?? summary?.VoucherRedemptionSelection
                ?? CreateSelectionFromBestOption(basket?.VoucherDetails ?? summary?.VoucherDetails);
            if (selection?.SelectedVoucherIds?.Any() != true)
                return null;

            var selectedIds = selection.SelectedVoucherIds.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (selectedIds.Length > 2)
                throw new ArgumentException("At most two vouchers can be redeemed for a booking.");

            var voucherDetails = basket.VoucherDetails ?? summary?.VoucherDetails;
            var option = voucherDetails?.RedemptionOptions?.FirstOrDefault(candidate =>
                candidate.OptionId == selection.SelectedOptionId &&
                (candidate.AppliedVouchers ?? Enumerable.Empty<Models.FlightSummary.AppliedVoucher>())
                    .Select(voucher => voucher.VoucherId)
                    .Where(voucherId => !voucherId.IsNullOrEmpty())
                    .Cast<string>()
                    .ToHashSet(StringComparer.OrdinalIgnoreCase)
                    .SetEquals(selectedIds));

            if (option is null || string.IsNullOrWhiteSpace(basket.CustomerProfile?.CustomerId) || string.IsNullOrWhiteSpace(basket.CustomerProfile?.MembershipDetails?.FQTNumber))
                throw new ArgumentException("The voucher redemption selection is not valid for this basket.");

            var vouchers = selectedIds
                .Select(selectedId => option.AppliedVouchers.FirstOrDefault(voucher => string.Equals(voucher.VoucherId, selectedId, StringComparison.OrdinalIgnoreCase)))
                .Where(voucher => voucher is not null)
                .Select(voucher => new RedeemedVoucher(voucher.VoucherId, voucher.PointsUsed))
                .ToArray();
            if (vouchers.Length != selectedIds.Length || vouchers.Any(voucher => voucher.PointsUsed <= 0))
                throw new ArgumentException("Voucher redemption points must be greater than zero.");

            var segment = summary?.Flights?.SelectMany(flight => flight.Segments ?? Enumerable.Empty<Models.FlightSummary.Segment>()).FirstOrDefault();
            if (segment is null)
                throw new ArgumentException("Flight details are required for voucher redemption.");

            return new VoucherRedemptionContext(basket.CustomerProfile.CustomerId, basket.CustomerProfile.MembershipDetails.FQTNumber, option.RedemptionType, basket.People?.Count() ?? 0, segment, vouchers);
        }

        private static Models.FlightSummary.Summary? GetFlightSummary(Models.Basket.Basket? basket)
        {
            return string.IsNullOrWhiteSpace(basket?.FlightSummary)
                ? null
                : Json.Deserialize<Models.FlightSummary.Summary>(basket.FlightSummary);
        }

        private static bool IsVoucherApplied(Models.Basket.Basket? basket, Models.FlightSummary.Summary? summary)
        {
            return basket?.VoucherApplied == true || summary?.VoucherApplied == true;
        }

        private static Models.FlightSummary.VoucherRedemptionSelection? CreateSelectionFromBestOption(
            Models.FlightSummary.VoucherRedemptionDetails? voucherDetails)
        {
            var option = voucherDetails?.RedemptionOptions?.FirstOrDefault(candidate =>
                candidate.OptionId == voucherDetails.BestOption?.OptionId);
            var voucherIds = option?.AppliedVouchers?
                .Select(voucher => voucher.VoucherId)
                .Where(voucherId => !voucherId.IsNullOrEmpty())
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return voucherIds is { Length: > 0 }
                ? new Models.FlightSummary.VoucherRedemptionSelection
                {
                    MaxVouchersToUse = voucherIds.Length,
                    SelectedVoucherIds = voucherIds,
                    SelectedOptionId = option!.OptionId
                }
                : null;
        }

        private static VoucherRedemptionRequest CreateRedemptionRequest(VoucherRedemptionContext context, RedeemedVoucher voucher) => new()
        {
            CustomerId = context.CustomerId,
            MemberNumber = context.MemberNumber,
            InitiatedBy = context.InitiatedBy,
            RedemptionType = context.RedemptionType.Equals("UPGRADE", StringComparison.OrdinalIgnoreCase)
                ? "Reward voucher redemption - Upgrade"
                : "Reward voucher redemption - Companion",
            FlightDetails = new VoucherFlightDetails
            {
                ActivityDate = context.Segment.Departure?.Date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
                    ?? throw new ArgumentException("A flight departure date is required for voucher redemption."),
                BookingClass = context.Segment.BookingClass,
                OriginAirport = context.Segment.Origin?.Code,
                DestinationAirport = context.Segment.Destination?.Code,
                OperatingFlightNumber = context.Segment.OperatingFlightNumber,
                NumberOfSegments = context.Segment.StopCount + 1,
                NumberOfPassengers = context.PassengerCount,
                MarketingFlightNumber = context.Segment.FlightNumber,
                OperatingCarrierCode = context.Segment.OperatingAirline?.Code,
                MarketingCarrierCode = context.Segment.Airline?.Code
            },
            VoucherDetails = new VoucherDetails { VoucherNumber = voucher.VoucherId, PointsToAward = voucher.PointsUsed }
        };

        private sealed record RedeemedVoucher(string VoucherId, long PointsUsed);
        private sealed record VoucherRedemptionContext(string CustomerId, string MemberNumber, string RedemptionType, int PassengerCount, Models.FlightSummary.Segment Segment, IReadOnlyCollection<RedeemedVoucher> Vouchers)
        {
            public string InitiatedBy => "EAPI";
        }



        private async Task<KeyValuePair<bool, OrderCreateGraphQLResponse>> RefreshOrder(string basketId)
        {
            var IsRefreshCall = true;
            int retryCounter = 1;
            bool responseProcessed = false;
            int responseDelayTimeInMs = ConfigReader.Get<int>(Constants.Environment.DelayTimeInMS);

            do
            {
                var orderFromCache = await GetOrderFromCache(basketId);
                if (orderFromCache != null)
                    return new KeyValuePair<bool, OrderCreateGraphQLResponse>(IsRefreshCall, orderFromCache);

                #region "No Cache Data -> Get Basket"

                Models.Basket.Basket basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);
                var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMs * retryCounter);

                if (basket.Status == null)
                    return new KeyValuePair<bool, OrderCreateGraphQLResponse>(false, null);

                //Check Processiong status in basket
                switch (basket.Status.CurrentBasketStatus)
                {
                    case BasketStatusEnum.PROCESSING:
                        await Task.Delay(timespan);
                        IsRefreshCall = true;
                        break;

                    case BasketStatusEnum.FAILED:
                        IsRefreshCall = true;
                        responseProcessed = true;
                        break;

                    case BasketStatusEnum.RETRY:
                        IsRefreshCall = false;
                        responseProcessed = true;
                        break;

                    default:
                        IsRefreshCall = false;
                        break;
                }
                retryCounter++;

                #endregion
            }
            while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.PollingCount));

            return new KeyValuePair<bool, OrderCreateGraphQLResponse>(IsRefreshCall, null); ;

        }

        private async Task<OrderCreateGraphQLResponse> GetOrderFromCache(string basketId)
        {
            if (ConfigReader.Caching.IsCacheEnabled("orderResponse", out string key, out TimeSpan expiry, out bool isSliding, basketId))
            {
                var cachedValue = await _distributedCache?.GetStringAsync(key);

                if (!cachedValue.IsNullOrEmpty())
                {
                    var orderCreateResponse = Encoding.UTF8.GetString(Convert.FromBase64String(cachedValue));

                    var response = Json.Deserialize<OrderCreateGraphQLResponse>(orderCreateResponse);

                    return response;

                }
            }
            return null;
        }

        private async Task<CustomerProfile> EnrolCustomer(Models.Basket.Basket basket, OfferResponse offer)
        {
            var request = (CustomerEnrolmentRequest)basket;

            if (ValidateCustomerEnrolment.ValidateCustomerForEnrolment(request, offer))
            {
                var response = await _customerEnrolmentProvider.EnrolVAAccount(request);

                return new() { FirstName = request.CustomerDetails.FirstName, LastName = request.CustomerDetails.LastName };
            }

            return null;
        }

    }
}
