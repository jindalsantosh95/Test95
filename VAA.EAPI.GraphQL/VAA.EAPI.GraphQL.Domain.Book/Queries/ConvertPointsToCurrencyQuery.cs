using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ConvertPointsToCurrencyQuery
    {

        private readonly ILogger<SeatMapQuery> _logger;
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly ILoyaltySyncServiceProvider _loyaltySyncServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ICustomerProvider _customerProvider;

        public ConvertPointsToCurrencyQuery(ILogger<SeatMapQuery> logger, IBasketServiceProvider basketServiceProvider, IOfferServiceProvider offerServiceProvider, ILoyaltySyncServiceProvider loyaltySyncServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, ICustomerProvider customerProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _loyaltySyncServiceProvider = loyaltySyncServiceProvider ?? throw new ArgumentNullException(nameof(loyaltySyncServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [GraphQLName("PointsToCurrencyConverterQuery")]
        [GraphQLDescription("Retrieves points equivalent currency  details ")]
        public async Task<IEnumerable<PointsEquivalentCurrency>> PointsEquivalentCurrency(string basketId)
        {
            try
            {
                if (string.IsNullOrEmpty(basketId))
                {
                    throw new ArgumentNullException(nameof(basketId), "Basket ID must be provided.");
                }

                if (ConfigReader.Try<bool>(Constants.Environment.EnableMockPointsCurrencyConverter, false))
                {
                    return Json.Deserialize<IEnumerable<PointsEquivalentCurrency>>(File.ReadAllText($"{Common.Constants.Application.FunctionAppDirPath}/MockResponses/PointsEquivalentCurrency.json"));
                }

                var basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

                OfferResponse offer = null;

                if (basket?.OfferIds?.Any() ?? false)
                    offer = await _offerServiceProvider.OfferRetrieveAsync(basket.OfferIds.First());

                #region "Redemption Validation"
                if (basket is not null && basket.FlightsearchRequest.isAwardSearch && TokenHelper.GetAuthorizationToken() == string.Empty) throw new Microsoft.IdentityModel.Tokens.SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                string fcNumber = string.Empty;
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
                        var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                        var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customer.Customer.DateOfBirth.ToString()), departureTime);

                        if (passengerAgeonDepartureDate < 16)
                            throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                    }
                }
                else if (basket?.CustomerProfile?.MembershipDetails?.FQTNumber.IsNullOrEmpty() == false)
                    throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                #endregion


                IEnumerable<PointsEquivalentCurrency> pointsCurrencyConverterResponse = null;

                if (offer.Offers.First().PayWithPointsEligible)
                {
                    if (basket.People.Count() * 3000 > Int32.Parse(basket?.CustomerProfile?.MembershipDetails?.FQTPoints ?? "0"))
                    {
                        throw new NotSupportedException(Book.Constants.Application.PointsNotSufficientForPWPErrorCode);
                    }
                    var loyaltyServiceResponse = await _loyaltySyncServiceProvider.PointsCurrencyConverter(PointsToCurrencyMapper.PointsToCurrencyRQMapper(offer, basket));

                    var updateBasketResponse = await _basketServiceProvider.UpdatePointsCurrencyDetailsAsync(basketId, loyaltyServiceResponse.First().PtcWisePointsEquivalentCurrencyDetails);

                    pointsCurrencyConverterResponse = PointsToCurrencyMapper.PointsToCurrencyRSMapper(loyaltyServiceResponse, basket);
                }
                else
                {
                    throw new NotSupportedException(Book.Constants.Application.OfferNoteligibleForPWPErrorCode);
                }

                return pointsCurrencyConverterResponse;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw;
            }
            catch (RequestValidationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw new Exception(Constants.Application.PointsToCurrencyConverterNotAvailableErrorCode);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in PointsToCurrencyConverterQuery.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}