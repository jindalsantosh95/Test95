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
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment.CoBrand;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using static VAA.EAPI.GraphQL.Domain.Book.Models.Common.Price;

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations.CoBrand
{
    [ExtendObjectType("Mutation")]
    public class SynchronyTokenMutation
    {
        protected readonly ILogger<SynchronyTokenMutation> _logger;
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IPaymentServiceProvider _paymentServiceProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IAnalyticsService _analyticsService;

        public SynchronyTokenMutation(IPaymentServiceProvider paymentServiceProvider, IOfferServiceProvider offerServiceProvider, IBasketServiceProvider basketServiceProvider, ILogger<SynchronyTokenMutation> logger, ITokenB2CProvider<string> tokenB2CProvider, ICustomerProvider customerProvider, IAnalyticsService analyticsService)
        {
            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(_paymentServiceProvider));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(_offerServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        }

        [GraphQLName("synchronyToken")]
        [GraphQLDescription("Generate Synchrony Token For the basket")]
        public async Task<SynchronyDataShareToken> GenerateSynchronyDataShareToken(string basketId)
        {
            var analyticsData = new Dictionary<string, object>();

            try
            {
                if (string.IsNullOrWhiteSpace(basketId))
                    throw new ArgumentException($"'{nameof(basketId)}' must be specified.");

                Models.Basket.Basket basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

                analyticsData["BasketId"] = basketId;
                analyticsData["Tier"] = basket.CustomerProfile.MembershipDetails.FQTTierCode;
                analyticsData["BookingType"] = basket.FlightsearchRequest.isAwardSearch == true ? "Award" : "Revenue";

                #region Get Offer

                var offerId = basket.OfferIds.First();

                if (string.IsNullOrWhiteSpace(offerId))
                    throw new ArgumentException($"'{nameof(offerId)}'  must be spefied.");
                OfferResponse offer = await _offerServiceProvider.OfferRetrieveAsync(offerId);
                CustomerDTO customerDTO = null;
                #endregion

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


                #region "Get Engagement Score"

                var engagementScore = await _paymentServiceProvider.GetEngagementScoreAsync(new() { Email = customerDTO?.Customer?.ContactDetails?.EmailDetails?.FirstOrDefault(e => e.PreferredFlag == "Y")?.emailId ?? basket.ContactDetails.EmailAddress });

                #endregion

                if (!string.IsNullOrEmpty(engagementScore?.AccountInfo))
                {
                    analyticsData["EngagementScore"] = engagementScore?.AccountInfo;
                }
                else
                    analyticsData["EngagementScore"] = "Not Received";

                #region "Create Synchrony Token Request"

                #region "Calculate InitialPurchaseAmount"

                var price = Convert.ToDouble(offer.Offers.First().SalePrice.Where(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).First().Amount.MonetaryAmount);
                var seatPrice = Convert.ToDouble(basket.FlightPassengerSeatMap?.Sum(seat => seat.Price.Amount));
                var quotePrice = basket.IsInsuranceAdded == true ? basket.InsuranceQuotes?.FirstOrDefault()?.Products.FirstOrDefault()?.TotalPrice ?? 0 : 0;
                var initialPurchaseAmount = price + seatPrice + quotePrice;

                #endregion

                var syncTokenRequest = SynchronyDataShareMapper.GetSynchronyTokenRequest(customerDTO, engagementScore.AccountInfo, basket, initialPurchaseAmount);

                #endregion

                #region "Call Synchrony Token Generate API endpoint"

                var responseToken = await _paymentServiceProvider.GetSynchronyDataShareTokenAsync(syncTokenRequest);

                #endregion
                analyticsData["TokenGenrationStatus"] = string.IsNullOrEmpty(responseToken.SynchronyToken) ? "Failed" : "Token Generated";

                if (!string.IsNullOrEmpty(responseToken.SynchronyToken))
                    await _basketServiceProvider.SetCobrandProgress(basketId, CoBrandStatusEnum.Initiated.ToString());

                return new() { Token = responseToken.SynchronyToken };
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                analyticsData["TokenGenrationStatus"] = "Apply Failed-SecurityTokenException";
                throw;
            }
            catch (RequestValidationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                analyticsData["TokenGenrationStatus"] = "Apply Failed-RequestValidationException";
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                analyticsData["TokenGenrationStatus"] = $"Apply Failed-Exception{ex.Message}";
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Unhandled Exception in Generating Synchrony Token.")
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
