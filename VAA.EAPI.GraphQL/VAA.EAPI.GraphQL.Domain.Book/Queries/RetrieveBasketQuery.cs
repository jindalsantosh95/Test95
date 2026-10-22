using HotChocolate;
using HotChocolate.Types;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;
using VAA.EAPI.GraphQL.Domain.Book.Models.InRuleEngine;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class RetrieveBasketQuery
    {
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly ILogger<RetrieveBasketQuery> _logger;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly IVRedServiceProvider _vRedProvider;
        private readonly IInRuleServiceProvider _inRuleProvider;
        private readonly IBasketEarnPointsCalculatorService _basketEarnPointsCalculatorService;

        public RetrieveBasketQuery(ILogger<RetrieveBasketQuery> logger, IBasketServiceProvider basketServiceProvider, IOfferServiceProvider offerServiceProvider,
            IRefServiceProvider referenceServiceProvider, ITokenB2CProvider<string> tokenB2CProvider,
            ICustomerProvider customerProvider, IVRedServiceProvider vRedProvider, IInRuleServiceProvider inRuleProvider, IBasketEarnPointsCalculatorService basketEarnPointsCalculatorService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _vRedProvider = vRedProvider ?? throw new ArgumentNullException(nameof(vRedProvider));
            _inRuleProvider = inRuleProvider ?? throw new ArgumentNullException(nameof(inRuleProvider)); ;
            _basketEarnPointsCalculatorService = basketEarnPointsCalculatorService ?? throw new ArgumentNullException(nameof(basketEarnPointsCalculatorService));
        }

        [GraphQLName("retrieveBasket")]
        [GraphQLDescription("Retrieves basket details with flight summary")]
        public async Task<Models.Basket.Basket> RetrieveBasket(string basketId)
        {
            try
            {
                if (string.IsNullOrEmpty(basketId)) throw new ArgumentNullException(nameof(basketId));
                CustomerDTO customer = null;
                var basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

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

                    #region "Redemption Validations & ProfileSet"

                    var token = TokenHelper.GetAuthorizationToken();

                    if (token != string.Empty)
                    {
                        var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(token);
                        var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);
                        var fqtNumber = claim?.FcNumber;
                        var customerFQTNumber = basket?.CustomerProfile?.MembershipDetails.FQTNumber;

                        if (!string.IsNullOrEmpty(customerFQTNumber) && !string.Equals(fqtNumber, customerFQTNumber))
                        {
                            throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                        }

                        customer = await _customerProvider.GetCustomer(string.Empty, fqtNumber);
                        if (customer != null)
                        {
                            var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                            var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customer.Customer.DateOfBirth.ToString()), departureTime);

                            if (passengerAgeonDepartureDate < 16)
                                throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                        }

                        //Get VRed Points
                        var vRedResponse = customer.Customer.MemberTier != Constants.Application.VACTier ? await _vRedProvider.GetVirginPoints(fqtNumber) : null;

                        //update customer profile in Basket and points
                        if (basket.CustomerProfile is null || basket.CustomerProfile.FirstName is null)
                        {
                            var updateBasketResponse = await _basketServiceProvider.UpdateCustomerProfileAsync(basketId, new Models.Basket.BasketLoyaltyMemberDetails()
                            {
                                FirstName = customer.Customer.FirstName,
                                LastName = customer.Customer.LastName,
                                MembershipDetails = new()
                                {
                                    FQTNumber = fqtNumber,
                                    FQTProgramme = "VS",
                                    FQTTierCode = customer.Customer.MemberTier,
                                    FQTPoints = vRedResponse?.Current ?? "0"
                                }
                            });

                            BasketPerson basketPerson = updateBasketResponse.People.First(p => p.Id == "0");
                            basketPerson.FirstName = customer.Customer.FirstName;
                            basketPerson.LastName = customer.Customer.LastName;
                            basketPerson.DateOfBirth = customer.Customer.DateOfBirth;
                            basketPerson.Gender = customer.Customer.Gender switch
                            {
                                "M" => EnumGenderCode.Male.ToString().ToUpper(),
                                "F" => EnumGenderCode.Female.ToString().ToUpper(),
                                "U" => EnumGenderCode.Undisclosed.ToString().ToUpper(),
                                "X" => EnumGenderCode.Unspecified.ToString().ToUpper(),
                                _ => customer.Customer.MemberTier != Constants.Application.VACTier ? throw new ArgumentOutOfRangeException(nameof(customer.Customer.Gender)) : ""
                            };
                            basketPerson.ProfilePassenger = true;
                            basketPerson.MembershipDetails = customer.Customer.MemberTier != Constants.Application.VACTier ? new() { FQTNumber = fqtNumber, FQTProgramme = "VS", FQTValidityStatus = Constants.FQTValidationStatus.Valid } : new();

                            var updateLeadPassengerRequest = new PassengerUpdateRequest() { BasketId = basketId, People = basketPerson };
                            await ValidateBasket.ValidatePassenger(updateLeadPassengerRequest, offer, basket);
                            var updateLeadPassenger = await _basketServiceProvider.UpdatePeopleAsync(updateLeadPassengerRequest);
                            basket = updateLeadPassenger;
                        }
                    }
                    else if (basket.FlightsearchRequest.isAwardSearch)
                        throw new SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                    #endregion

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

                return basket;
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
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in retrieve basket query.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}
