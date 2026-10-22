using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations.Basket
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class UpdateInsuranceDecisionMutation
    {
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly ISeatServiceProvider _seatServiceProvider;
        private readonly ILOPAServiceProvider _lopaServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<UpdateInsuranceDecisionMutation> _logger;
        private readonly ICustomerProvider _customerProvider;

        public UpdateInsuranceDecisionMutation(ILogger<UpdateInsuranceDecisionMutation> logger, IBasketServiceProvider basketServiceProvider, IOfferServiceProvider offerServiceProvider, IRefServiceProvider referenceServiceProvider, ISeatServiceProvider seatServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, ILOPAServiceProvider lopaServiceProvider, ICustomerProvider customerProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _seatServiceProvider = seatServiceProvider ?? throw new ArgumentNullException(nameof(seatServiceProvider));
            _lopaServiceProvider = lopaServiceProvider ?? throw new ArgumentNullException(nameof(lopaServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [GraphQLName("insuranceDecision")]
        [GraphQLDescription("Update insurance decision for a basket")]
        public async Task<Models.Basket.Basket> UpdateInsuranceDecision(UpdateInsuranceDecisionRequest request)
        {
            try
            {

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.BasketId))
                    throw new ArgumentException($"'{nameof(request.BasketId)}' must be specified.");

                var basket = await _basketServiceProvider.RetrieveBasketAsync(request.BasketId);

                if (basket is not null && basket.FlightsearchRequest.isAwardSearch && TokenHelper.GetAuthorizationToken() == string.Empty) throw new SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                var offerId = basket.OfferIds.First();

                if (string.IsNullOrWhiteSpace(offerId))
                    throw new ArgumentException($"'{nameof(offerId)}'  must be spefied.");

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

                        var customer = await _customerProvider.GetCustomer(string.Empty, fQTNumber);
                        if (customer != null)
                        {
                            var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                            var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customer.Customer.DateOfBirth.ToString()), departureTime);

                            if (passengerAgeonDepartureDate < 16)
                                throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                        }
                    }
                    else if (basket?.CustomerProfile?.MembershipDetails.FQTNumber.IsNullOrEmpty() == false)
                        throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));

                    basket = await _basketServiceProvider.UpdateInsuranceDecisionAsync(request.BasketId, request.InsuranceSelected);

                    basket.Summary = BasketSummaryMapper.MapFlightSummary(offer, basket, refAirportData, refAirlineData, null, refCabinData);

                }

                return basket;

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (RequestValidationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in insurance decision mutation.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}
