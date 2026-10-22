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
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations.Basket
{
    [ExtendObjectType("Mutation")]
    public class PassengerMutations
    {
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<PassengerMutations> _logger;
        private readonly IBasketEarnPointsCalculatorService _basketEarnPointsCalculatorService;

        public PassengerMutations(ILogger<PassengerMutations> logger, IBasketServiceProvider basketServiceProvider, IOfferServiceProvider offerServiceProvider, IRefServiceProvider referenceServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, ICustomerProvider customerProvider, IBasketEarnPointsCalculatorService basketEarnPointsCalculator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _basketEarnPointsCalculatorService = basketEarnPointsCalculator ?? throw new ArgumentNullException(nameof(basketEarnPointsCalculator));
        }

        [GraphQLName("updatePeople")]
        [GraphQLDescription("Updates people details in basket")]
        public async Task<Models.Basket.Basket> UpdatePeople(PassengerUpdateRequest updatePeopleRequest)
        {
            string basketId = string.Empty;
            try
            {
                if (updatePeopleRequest == null)
                    throw new ArgumentNullException(nameof(updatePeopleRequest));

                basketId = updatePeopleRequest.BasketId;
                CustomerDTO customer = null;

                if (updatePeopleRequest.People.Id != "0")
                {
                    updatePeopleRequest.People.ProfilePassenger = false;
                }

                Models.Basket.Basket basket = await _basketServiceProvider.RetrieveBasketAsync(updatePeopleRequest.BasketId);

                if (basket != null)
                {
                    OfferResponse offer = await _offerServiceProvider.OfferRetrieveAsync(basket.OfferIds.First());

                    if (basket?.OfferIds?.Any() ?? false)
                    {
                        var token = TokenHelper.GetAuthorizationToken();
                        if (token != string.Empty)
                        {

                            var fqtNumber = TokenHelper.GetFQTNumber(_tokenB2CProvider).Result;

                            var customerFQTNumber = basket?.CustomerProfile?.MembershipDetails.FQTNumber;

                            if (!string.IsNullOrEmpty(customerFQTNumber) && !string.Equals(fqtNumber, customerFQTNumber))
                            {
                                throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                            }

                            //default lead passenger with customer profile details in basket
                            customer = await _customerProvider.GetCustomer(string.Empty, fqtNumber);

                            if (customer != null)
                            {
                                var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                                var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customer.Customer.DateOfBirth.ToString()), departureTime);

                                if (passengerAgeonDepartureDate < 16)
                                    throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                            }

                            if (updatePeopleRequest.People.Id == "0" && updatePeopleRequest.People.ProfilePassenger.GetValueOrDefault(false))
                            {

                                BasketPerson basketPerson = basket.People.First(p => p.Id == "0");
                                basketPerson.FirstName = customer.Customer.FirstName;
                                basketPerson.LastName = customer.Customer.LastName;
                                basketPerson.DateOfBirth = customer.Customer.DateOfBirth;
                                basketPerson.Gender = customer.Customer.MemberTier != Constants.Application.VACTier ? customer.Customer.Gender switch
                                {
                                    "M" => EnumGenderCode.Male.ToString().ToUpper(),
                                    "F" => EnumGenderCode.Female.ToString().ToUpper(),
                                    "U" => EnumGenderCode.Undisclosed.ToString().ToUpper(),
                                    "X" => EnumGenderCode.Unspecified.ToString().ToUpper(),
                                    _ => throw new ArgumentOutOfRangeException(nameof(customer.Customer.Gender))
                                } : updatePeopleRequest.People.Gender;
                                basketPerson.ProfilePassenger = true;
                                basketPerson.MembershipDetails = customer.Customer.MemberTier != Constants.Application.VACTier ? new() { FQTNumber = fqtNumber, FQTProgramme = "VS", FQTValidityStatus = Constants.FQTValidationStatus.Valid } : new();

                                updatePeopleRequest = new PassengerUpdateRequest() { BasketId = basketId, People = basketPerson };
                            }

                        }
                        else if (basket.FlightsearchRequest.isAwardSearch)
                            throw new SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                        if (!updatePeopleRequest.People.FirstName.IsNullOrEmpty())
                            await ValidateBasket.ValidatePassenger(updatePeopleRequest, offer, basket, _customerProvider);

                        IEnumerable<AirportRefData> refAirportData = null;
                        IEnumerable<AirlineRefData> refAirlineData = null;
                        IEnumerable<CabinRefData> refCabinData = null;

                        Parallel.Invoke(
                            new Action(() => refAirportData = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                            new Action(() => refAirlineData = _referenceServiceProvider.RetrieveAirlineDataAsync().GetAwaiter().GetResult()),
                            new Action(() => refCabinData = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                            new Action(() => basket = _basketServiceProvider.UpdatePeopleAsync(updatePeopleRequest).GetAwaiter().GetResult()));

                        
                        #region "Calculate FC Earn Points"
                        try
                        {
                            basket.EarnPoints = await _basketEarnPointsCalculatorService.Calculate(basket, offer, customer);
                        }
                        catch (Exception exEarnPoints)
                        {
                            //Do Not Break the journey even if points are not re-calculated.
                            _logger.LogError(exEarnPoints, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {exEarnPoints}");
                        }
                        #endregion

                        basket.Summary = BasketSummaryMapper.MapFlightSummary(offer, basket, refAirportData, refAirlineData, null, refCabinData);
                        basket.PointOfSale = new() { CountryCode = offer.Pos.CountryCode, CityCode = offer.Pos.CityCode };
                        basket.IsCoBrandEligible = CoBrandHelper.IsMemberCoBrandEligible(basket, customer);
                    }
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in update people mutation.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}
