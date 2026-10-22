using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations.Basket
{
    [ExtendObjectType("Mutation")]
    public class TnCMutation
    {
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<TnCMutation> _logger;
        private readonly ICustomerProvider _customerProvider;

        public TnCMutation(ILogger<TnCMutation> logger, IBasketServiceProvider basketServiceProvider, IOfferServiceProvider offerServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, IRefServiceProvider referenceServiceProvider, ICustomerProvider customerProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [GraphQLName("updateTnC")]
        [GraphQLDescription("Updates terms and conditions in basket")]
        public async Task<Models.Basket.Basket> UpdateTermsAndConditions(TnCUpdateRequest updateTnCRequest)
        {
            string basketId = string.Empty;
            try
            {
                if (updateTnCRequest == null)
                    throw new ArgumentNullException(nameof(updateTnCRequest));

                basketId = updateTnCRequest.BasketId;
                CustomerDTO customer = null;
                Models.Basket.Basket basket = await _basketServiceProvider.UpdateTnCAsync(updateTnCRequest);

                if (basket?.OfferIds?.Any() ?? false)
                {
                    IEnumerable<AirportRefData> refAirportData = null;
                    IEnumerable<AirlineRefData> refAirlineData = null;
                    IEnumerable<CabinRefData> refCabinData = null;
                    OfferResponse offer = null;

                    Parallel.Invoke(
                        new Action(() => refAirportData = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                        new Action(() => refAirlineData = _referenceServiceProvider.RetrieveAirlineDataAsync().GetAwaiter().GetResult()),
                        new Action(() => refCabinData = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                        new Action(() => offer = _offerServiceProvider.OfferRetrieveAsync(basket.OfferIds.First()).GetAwaiter().GetResult()));

                    if (TokenHelper.GetAuthorizationToken() != string.Empty)
                    {
                        var fQTNumber = TokenHelper.GetFQTNumber(_tokenB2CProvider).Result;
                        var customerFQTNumber = basket?.CustomerProfile?.MembershipDetails.FQTNumber;

                        if (!string.IsNullOrEmpty(customerFQTNumber) && !string.Equals(fQTNumber, customerFQTNumber))
                        {
                            throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                        }

                        customer = await _customerProvider.GetCustomer(string.Empty, fQTNumber);
                        if (customer != null)
                        {
                            var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                            var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customer.Customer.DateOfBirth.ToString()), departureTime);

                            if (passengerAgeonDepartureDate < 16)
                                throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                        }
                    }
                    else if (basket.FlightsearchRequest.isAwardSearch)
                        throw new SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                    basket.Summary = BasketSummaryMapper.MapFlightSummary(offer, basket, refAirportData, refAirlineData, null, refCabinData);
                    basket.PointOfSale = new() { CountryCode = offer.Pos.CountryCode, CityCode = offer.Pos.CityCode };
                    basket.IsCoBrandEligible = CoBrandHelper.IsMemberCoBrandEligible(basket, customer);
                }

                return basket;
            }
            catch (HttpRequestException httpex)
            {
                _logger.LogError(httpex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {httpex}");
                throw;
            }
            catch (RequestValidationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw;
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in  update TnC mutation.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}
