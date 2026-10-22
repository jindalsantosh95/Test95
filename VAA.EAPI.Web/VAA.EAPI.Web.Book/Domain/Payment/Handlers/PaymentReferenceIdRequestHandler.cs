using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.Payment.Requests;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Models.Book.Payment;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Domain.Payment.Handlers
{
    public class PaymentReferenceIdRequestHandler : IRequestHandler<PaymentReferenceIdRequest, PaymentResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentReferenceIdRequestHandler> _logger;

        public PaymentReferenceIdRequestHandler(IMediator mediator, ILogger<PaymentReferenceIdRequestHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PaymentResponse> Handle(PaymentReferenceIdRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var graphQLRequest = new GraphQLRequest
                {
                    PqId = ConfigReader.Get(Constants.Environment.OrderPaymentRefIdPqId),
                    Variables = JObject.Parse(Json.Serialize(new { request = new PaymentReferenceGraphQLRequest { BasketId = request.BasketId, AmountToPay = request.AmountToPay, IpAddress = request.IpAddress, BrowserInfo = request.BrowserInfo, DeviceInfo = request.DeviceInfo, PaymentFailureLink = request.ExpiryUrl, PaymentSuccessLink = request.ReturnUrl, PayWithPointsValue = request.PayWithPointsValue } }, false)),
                    Headers = Utility.GetHeaders(request.AuthToken)
                };

                var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);

                var paymentResponse = Json.DeserializeObject<PaymentResponse>(graphQLResponse.Data["orderPaymentReferenceId"]);
                paymentResponse.UpdateLinks(request.Link, request.ExpiryUrl, request.ReturnUrl);
                paymentResponse.UpdatePaymentSession();

                return paymentResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in PaymentReferenceIdHandler: {ex.Message}, CorrelationId: {AsyncContext.Current.CorrelationId}");
                throw;
            }
        }
    }
}
