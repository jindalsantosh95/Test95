using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Book.Constants;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.CoBrand;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class CoBrandApplicationStatusQuery
    {
        private readonly ILogger<CoBrandApplicationStatusQuery> _logger;
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly IPaymentServiceProvider _paymentServiceProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IAnalyticsService _analyticsService;

        public CoBrandApplicationStatusQuery(ILogger<CoBrandApplicationStatusQuery> logger, IBasketServiceProvider basketServiceProvider, IRefServiceProvider referenceServiceProvider, IPaymentServiceProvider paymentServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, ICustomerProvider customerProvider, IOfferServiceProvider offerServiceProvider, IAnalyticsService _analyticsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(paymentServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            this._analyticsService = _analyticsService ?? throw new ArgumentNullException(nameof(_analyticsService));
        }

        [GraphQLName("retrieveCoBrandApplicationStatus")]
        [GraphQLDescription("Retrieves CoBrand Application status")]
        public async Task<ApplicationStatusResponse> RetrieveApplicationStatus(ApplicationStatusRequest request)
        {
            var analyticsData = new Dictionary<string, object>();

            try
            {
                if (string.IsNullOrEmpty(request.BasketId)) throw new ArgumentNullException(nameof(request.BasketId));

                if (string.IsNullOrEmpty(request.SynchronyToken)) throw new ArgumentNullException(nameof(request.SynchronyToken));

                var basket = await _basketServiceProvider.RetrieveBasketAsync(request.BasketId);

                CustomerDTO customerDTO = null;

                if (basket?.OfferIds?.Any() ?? false)
                {
                    IEnumerable<AirportRefData> refAirportData = Enumerable.Empty<AirportRefData>();
                    IEnumerable<AirlineRefData> refAirlineData = Enumerable.Empty<AirlineRefData>();
                    IEnumerable<CabinRefData> refCabinData = Enumerable.Empty<CabinRefData>();
                    OfferResponse offer = null;

                    Parallel.Invoke(
                        new Action(() => refAirportData = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                        new Action(() => refAirlineData = _referenceServiceProvider.RetrieveAirlineDataAsync().GetAwaiter().GetResult()),
                        new Action(() => refCabinData = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                        new Action(() => offer = _offerServiceProvider.OfferRetrieveAsync(basket.OfferIds.First()).GetAwaiter().GetResult()));

                    basket.Summary = BasketSummaryMapper.MapFlightSummary(offer, basket, refAirportData, refAirlineData, null, refCabinData);

                    #region "Token Validation"

                    if (basket is not null && basket.FlightsearchRequest.isAwardSearch && TokenHelper.GetAuthorizationToken() == string.Empty) throw new Microsoft.IdentityModel.Tokens.SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                    if (TokenHelper.GetAuthorizationToken() != string.Empty)
                    {

                        var fQTNumber = TokenHelper.GetFQTNumber(_tokenB2CProvider).Result;
                        var customerFQTNumber = basket?.CustomerProfile?.MembershipDetails.FQTNumber;

                        if (!string.IsNullOrEmpty(customerFQTNumber) && !string.Equals(fQTNumber, customerFQTNumber))
                        {
                            throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                        }

                        customerDTO = await _customerProvider.GetCustomer(string.Empty, fQTNumber);
                        if (customerDTO != null)
                        {
                            var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                            var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customerDTO.Customer.DateOfBirth.ToString()), departureTime);

                            if (passengerAgeonDepartureDate < 16)
                                throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                        }
                    }
                    else if (basket?.CustomerProfile?.MembershipDetails?.FQTNumber.IsNullOrEmpty() == false)
                        throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                    #endregion
                }

                analyticsData["BasketId"] = request.BasketId;
                analyticsData["Tier"] = basket.CustomerProfile.MembershipDetails.FQTTierCode;
                analyticsData["BookingType"] = basket.FlightsearchRequest.isAwardSearch == true ? "Award" : "Revenue";

                var applicationStatus = await _paymentServiceProvider.RetrieveApplicationStatusAsync(request.SynchronyToken);

                analyticsData["ApplyStatus"] = applicationStatus.ApplicationDecision;
                analyticsData["CreditLimit"] = applicationStatus.CreditLimit;

                var status = applicationStatus.ApplicationDecision switch
                {
                    "APPROVED" => Convert.ToDouble(applicationStatus.CreditLimit) < basket.Summary.PaymentAmount ? nameof(CoBrandStatusEnum.ApprovedInsufficientCredit) : nameof(CoBrandStatusEnum.Approved),
                    "DECLINED" => nameof(CoBrandStatusEnum.Rejected),
                    "REFERRED" => nameof(CoBrandStatusEnum.Pending),
                    "ERROR" => nameof(CoBrandStatusEnum.Error),
                    _ => nameof(CoBrandStatusEnum.Initiated)
                };

                var statusDescription = status switch
                {
                    "ApprovedInsufficientCredit" => "Insufficient Credit", // Actual messages to be updated later when shared by team
                    "Approved" => "Approved",
                    "Pending" => " Pending",
                    "Rejected" => "Rejected",
                    "Initiated" => "Initiated",
                    "Error" => "Error",
                    _ => ""
                };


                if (!string.IsNullOrEmpty(applicationStatus.ApplicationDecision))
                    await _basketServiceProvider.SetCobrandProgress(request.BasketId, applicationStatus.ApplicationDecision switch
                    {
                        "APPROVED" => Convert.ToDouble(applicationStatus.CreditLimit) < basket.Summary.PaymentAmount ? nameof(CoBrandStatusEnum.ApprovedInsufficientCredit) : nameof(CoBrandStatusEnum.Approved),
                        "DECLINED" => nameof(CoBrandStatusEnum.Rejected),
                        "REFERRED" => nameof(CoBrandStatusEnum.Pending),
                        "ERROR" => nameof(CoBrandStatusEnum.Error),
                        _ => nameof(CoBrandStatusEnum.Initiated)
                    });

                return new()
                {
                    ApplicationDecision = applicationStatus.ApplicationDecision,
                    CoBrandApplicationStatus = new() { Status = status, StatusDescription = statusDescription }
                };
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                analyticsData["ApplyStatus"] = "Apply Status Unknown-SecurityTokenException";

                throw;
            }
            catch (RequestValidationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                analyticsData["ApplyStatus"] = "Apply Status Unknown-RequestValidationException";

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                analyticsData["ApplyStatus"] = $"Apply Status Unknown-Exception{ex.Message}";

                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in retrieve co brand application status query.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
            finally
            {
                _ = _analyticsService.TrackAnalytics((TrackAnalyticsMessage)(Type: Common.Constants.Application.TrackingAnalyticsType, Subject: "CoBrandApply", Data: analyticsData));
            }
        }
    }
}