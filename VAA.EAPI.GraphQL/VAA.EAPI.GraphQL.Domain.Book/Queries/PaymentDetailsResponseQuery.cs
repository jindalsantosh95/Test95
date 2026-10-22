using HotChocolate;
using HotChocolate.Types;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class PaymentDetailsResponseQuery
    {
        protected readonly ILogger<PaymentDetailsResponseQuery> _logger;
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IPaymentServiceProvider _paymentServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly ICustomerProvider _customerProvider;

        public PaymentDetailsResponseQuery(ILogger<PaymentDetailsResponseQuery> logger, IBasketServiceProvider basketServiceProvider, IOfferServiceProvider offerServiceProvider, IPaymentServiceProvider paymentServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, IRefServiceProvider referenceServiceProvider, ICustomerProvider customerProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(paymentServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(_offerServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [GraphQLName("paymentDetailsResponse")]
        [GraphQLDescription("Get Payment Details Response From Cache")]
        public async Task<PaymentOrderResponse> GetPaymentDetailsResponseAsync(string basketId)
        {
            try
            {
                if (string.IsNullOrEmpty(basketId)) throw new ArgumentNullException(nameof(basketId));

                Basket basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

                #region Search Offer & Reference

                var offerId = basket.OfferIds.First();

                if (string.IsNullOrWhiteSpace(offerId))
                    throw new ArgumentException($"'{nameof(offerId)}'  must be spefied.");

                IEnumerable<AirportRefData> refAirportData = null;
                IEnumerable<AirlineRefData> refAirlineData = null;
                IEnumerable<CabinRefData> refCabinData = null;
                OfferResponse offer = null;

                Parallel.Invoke(
                    new Action(() => refAirportData = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                    new Action(() => refAirlineData = _referenceServiceProvider.RetrieveAirlineDataAsync().GetAwaiter().GetResult()),
                    new Action(() => refCabinData = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                    new Action(() => offer = _offerServiceProvider.OfferRetrieveAsync(offerId).GetAwaiter().GetResult()));

                #endregion


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

                    var customer = await _customerProvider.GetCustomer(string.Empty, fqtNumber);
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

                PaymentSession paymentSession = basket.PaymentSessions.First();
                if (!paymentSession.IsNullOrDefault())
                {
                    string paymentType = string.Empty;
                    string paymentId = string.Empty;

                    if (!string.IsNullOrEmpty(paymentSession.PaymentReferenceId))
                    {
                        paymentType = PaymentType.APM.ToDisplayName();
                        paymentId = paymentSession.PaymentReferenceId;
                    }
                    else
                    {
                        paymentType = PaymentType.Card.ToDisplayName();
                        paymentId = paymentSession.ParentPaymentReferenceId;
                    }
                    if (paymentSession.HppPaymentStatus != null)
                    {
                        var deltaPaymentMsREsponse = await _paymentServiceProvider.PaymentDetailsAsync(paymentId, paymentType);
                    }

                    decimal offerAmount = Convert.ToDecimal(offer.Offers.First().SalePrice.FirstOrDefault(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount);
                    return OrderPaymentMapper.GetOrderPaymentResponse(offerAmount, offer, basket, refAirportData, refAirlineData, refCabinData, paymentSession);

                }
                throw new NotImplementedException();
            }
            catch (RequestValidationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in Get PaymentDetails Response.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}