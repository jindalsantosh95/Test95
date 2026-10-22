using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Models.Book.Payment;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Domain.Payment.Handlers
{
    public class PaymentDetailsResponseRequestHandler : IRequestHandler<PaymentDetailsResponseRequest, PaymentResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentDetailsResponseRequestHandler> _logger;

        public PaymentDetailsResponseRequestHandler(IMediator mediator, ILogger<PaymentDetailsResponseRequestHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PaymentResponse> Handle(PaymentDetailsResponseRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var graphQLRequest = new GraphQLRequest
                {
                    PqId = ConfigReader.Get(Constants.Environment.PaymentDetailsResponsePqId),
                    Variables = JObject.Parse(Json.Serialize(new
                    {
                        basketId = request.BasketId
                    })),
                    Headers = Utility.GetHeaders(request.AuthToken)
                };

                var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);
                var paymentResponse = Json.DeserializeObject<PaymentResponse>(graphQLResponse.Data["paymentDetailsResponse"]);
                var paymentStatus = paymentResponse.PaymentSessions.FirstOrDefault();
                if (paymentStatus != null && paymentStatus.HppPaymentStatus == "SUCCESS")
                {
                    paymentResponse.UpdateLinks(string.Empty, string.Empty, string.Empty, true);
                }
                else
                {
                    paymentResponse.UpdateLinks(string.Empty, string.Empty, string.Empty, false);
                }

                return paymentResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OrderResponseHandler: {ex.Message}, CorrelationId: {AsyncContext.Current.CorrelationId}");
                throw;
            }
        }
    }
}
