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
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations.Basket
{
    [ExtendObjectType("Mutation")]
    public class UpdatePaymentStatusMutation
    {
        protected readonly ILogger<UpdatePaymentStatusMutation> _logger;
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly ICustomerProvider _customerProvider;

        public UpdatePaymentStatusMutation(IBasketServiceProvider basketServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, ILogger<UpdatePaymentStatusMutation> logger, IOfferServiceProvider offerServiceProvider, ICustomerProvider customerProvider)
        {
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [GraphQLName("bookUpdatePaymentStatus")]
        [GraphQLDescription("Update Book Payment Status.")]
        public async Task<UpdatePaymentStatusResponse> UpdatePaymentStatus(UpdatePaymentStatusRequest request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.BasketId) || string.IsNullOrWhiteSpace(request.HppPaymentStatus.ToString()))
                    throw new ArgumentException($"'{nameof(request.BasketId)} or {nameof(request.HppPaymentStatus)} ' must be spefied.");

                Models.Basket.Basket basket = await _basketServiceProvider.RetrieveBasketAsync(request.BasketId);

                if (basket is not null && basket.FlightsearchRequest.isAwardSearch && TokenHelper.GetAuthorizationToken() == string.Empty) throw new SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                if (TokenHelper.GetAuthorizationToken() != string.Empty)
                {
                    var fQTNumber = TokenHelper.GetFQTNumber(_tokenB2CProvider).Result;
                    var customerFQTNumber = basket?.CustomerProfile?.MembershipDetails.FQTNumber;

                    if (!string.IsNullOrEmpty(customerFQTNumber) && !string.Equals(fQTNumber, customerFQTNumber))
                    {
                        throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                    }

                    if (basket?.OfferIds?.Any() ?? false)
                    {
                        OfferResponse offer = await _offerServiceProvider.OfferRetrieveAsync(basket.OfferIds.First());
                        if (offer != null)
                        {
                            var customer = await _customerProvider.GetCustomer(string.Empty, fQTNumber);
                            if (customer != null)
                            {
                                var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                                var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customer.Customer.DateOfBirth.ToString()), departureTime);

                                if (passengerAgeonDepartureDate < 16)
                                    throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                            }
                        }
                    }
                }
                else if (basket?.CustomerProfile?.MembershipDetails?.FQTNumber.IsNullOrEmpty() == false)
                    throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));

                if (basket?.PaymentSessions?.Any() ?? false)
                {
                    var paymentSession = basket?.PaymentSessions.First();
                    paymentSession.PaymentReferenceId = request.PaymentReferenceId;
                    paymentSession.HppPaymentStatus = request.HppPaymentStatus.ToString();
                    var basketPaymentRequest = new BasketPaymentRequest
                    {
                        BasketId = request.BasketId,
                        PaymentSessionsBasketMS = OrderPaymentMapper.UpdateBasketPaymentSessionRequest(paymentSession)
                    };
                    Models.Basket.Basket basketUpdate = await _basketServiceProvider.UpdatePaymentDetailsAsync(basketPaymentRequest);
                    return new UpdatePaymentStatusResponse() { PaymentStatus = request.HppPaymentStatus };
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (RequestValidationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Unhandled Exception in create PRI from payment mutation.")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                            .Build());
            }
        }
    }
}