using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations.Basket
{
    [ExtendObjectType("Mutation")]
    public class ContactDetailMutation
    {
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<ContactDetailMutation> _logger;
        private readonly ICustomerProvider _customerProvider;
        private readonly ICustomerServiceProvider _customerServiceProvider;

        public ContactDetailMutation(IBasketServiceProvider basketServiceProvider, IOfferServiceProvider offerServiceProvider, IRefServiceProvider referenceServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, ILogger<ContactDetailMutation> logger, ICustomerProvider customerProvider, ICustomerServiceProvider customerServiceProvider)
        {
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerServiceProvider = customerServiceProvider ?? throw new ArgumentNullException(nameof(customerServiceProvider));
        }

        [GraphQLName("updateContactDetails")]
        [GraphQLDescription("Updates basket with contact details")]
        public async Task<Models.Basket.Basket> UpdateContactDetails(ContactUpdateRequest updateContactDetailsRequest)
        {
            string basketId = string.Empty;
            try
            {
                basketId = updateContactDetailsRequest.BasketId;

                Models.Basket.Basket basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);
                CustomerDTO customer = null;
                var leadPassenger = basket?.People?.FirstOrDefault(p => p.Id == "0");

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
                    {
                        throw new SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));
                    }
                    else
                    {
                        if (ConfigReader.Get<bool>(Constants.Environment.EnableAutoMatch))
                        {
                            if (updateContactDetailsRequest?.ContactDetails?.PeopleId == "0")
                            {
                                var basketEmail = basket?.ContactDetails?.EmailAddress;
                                var requestEmail = updateContactDetailsRequest?.ContactDetails?.EmailAddress;

                                // Determine if existing FQT came from Auto-Match 
                                bool wasAutoMatched = leadPassenger?.MembershipDetails?.FQTNumber != null && basket.FcAutoMatched == true;
                                updateContactDetailsRequest.FcAutoMatched = basket.FcAutoMatched;

                                // Email changed
                                if (!string.Equals(basketEmail, requestEmail, StringComparison.OrdinalIgnoreCase))
                                {
                                    // (a) Email cleared
                                    if (string.IsNullOrWhiteSpace(requestEmail))
                                    {
                                        updateContactDetailsRequest.ContactDetails.FcAutoMatchStamping = null;
                                        updateContactDetailsRequest.FcAutoMatched = null;

                                        // Clear membership only if it was auto-matched before
                                        if (wasAutoMatched && !string.IsNullOrWhiteSpace(leadPassenger?.MembershipDetails?.FQTNumber))
                                        {
                                            leadPassenger.MembershipDetails = new();
                                        }
                                    }
                                    else
                                    {
                                        // (b) New email entered — re-attempt auto-match if stampBooking is null or true
                                        if (updateContactDetailsRequest.ContactDetails.FcAutoMatchStamping is null or true)
                                        {
                                            var profiles = await _customerServiceProvider.GetCustomerDetailsByEmail(new() { Email = requestEmail});

                                            if (!profiles.IsNullOrEmpty())
                                            {
                                                var matchedProfile = profiles.First();
                                                if (string.Equals(matchedProfile.LastName, leadPassenger?.LastName, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    updateContactDetailsRequest.ContactDetails.FcAutoMatchStamping = true;
                                                    updateContactDetailsRequest.FcAutoMatched = true;

                                                    leadPassenger.MembershipDetails = new()
                                                    {
                                                        FQTNumber = matchedProfile.LoyaltyMemberNumber,
                                                        FQTProgramme = "VS",
                                                        FQTValidityStatus = Constants.FQTValidationStatus.Valid
                                                    };
                                                }
                                                else
                                                {
                                                    updateContactDetailsRequest.ContactDetails.FcAutoMatchStamping = false;
                                                    updateContactDetailsRequest.FcAutoMatched = false;
                                                }
                                            }
                                            else
                                            {
                                                updateContactDetailsRequest.ContactDetails.FcAutoMatchStamping = false;
                                                updateContactDetailsRequest.FcAutoMatched = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            updateContactDetailsRequest.FcAutoMatched = null;
                            updateContactDetailsRequest.ContactDetails.FcAutoMatchStamping = null;
                        }
                    }

                    basket = await _basketServiceProvider.UpdateContactAsync(updateContactDetailsRequest);


                    basket.Summary = BasketSummaryMapper.MapFlightSummary(offer, basket, refAirportData, refAirlineData, null, refCabinData);
                    basket.PointOfSale = new() { CountryCode = offer.Pos.CountryCode, CityCode = offer.Pos.CityCode };
                    basket.IsCoBrandEligible = CoBrandHelper.IsMemberCoBrandEligible(basket, customer);

                    #region Update MaskedFcDetails
                    if (updateContactDetailsRequest.FcAutoMatched == true && !string.IsNullOrEmpty(leadPassenger.MembershipDetails?.FQTNumber) && updateContactDetailsRequest.ContactDetails.FcAutoMatchStamping == true)
                    {
                        basket = await UpdateLeadPassengerWithMembership(basket, leadPassenger);
                    }
                    else if (updateContactDetailsRequest.ContactDetails.FcAutoMatchStamping == false)
                    {
                        leadPassenger.MembershipDetails = new();
                        basket = await UpdateLeadPassengerWithMembership(basket, leadPassenger);
                    }

                    #endregion
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
                             .SetMessage("Unhandled Exception in update contact mutation.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }

        private static string MaskFQTNumber(string fqtNumber)
        {
            if (string.IsNullOrWhiteSpace(fqtNumber))
                return string.Empty;

            return fqtNumber.Length <= 4
                ? new string('*', fqtNumber.Length)
                : new string('*', fqtNumber.Length - 4) + fqtNumber[^4..];
        }

        private async Task<Models.Basket.Basket> UpdateLeadPassengerWithMembership(Models.Basket.Basket basket, BasketPerson leadPassenger)
        {
            if (basket == null) return basket;
            if (!string.IsNullOrEmpty(leadPassenger?.MembershipDetails.FQTNumber))
            {
                var maskedFqtNumber = MaskFQTNumber(leadPassenger.MembershipDetails.FQTNumber);

                leadPassenger.MembershipDetails = new()
                {
                    FQTNumber = maskedFqtNumber,
                    FQTProgramme = "VS",
                    FQTValidityStatus = Constants.FQTValidationStatus.Valid
                };
            }

            var updateRequest = new PassengerUpdateRequest
            {
                BasketId = basket.BasketId.ToString(),
                People = leadPassenger
            };

            return await _basketServiceProvider.UpdatePeopleAsync(updateRequest);
        }
    }
}