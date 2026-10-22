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
    public class PaymentStatusGraphQLRequestHandler : IRequestHandler<PaymentStatusGraphQLRequest, PaymentStatusResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentStatusGraphQLRequestHandler> _logger;

        public PaymentStatusGraphQLRequestHandler(IMediator mediator, ILogger<PaymentStatusGraphQLRequestHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<PaymentStatusResponse> Handle(PaymentStatusGraphQLRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var graphQLRequest = new GraphQLRequest
                {
                    PqId = ConfigReader.Get(Constants.Environment.OrderPaymentStatusPqId),
                    Variables = JObject.Parse(Json.Serialize(new { request = request }, false)),
                    Headers = Utility.GetHeaders(request.AuthToken)
                };

                var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);
                return Json.DeserializeObject<PaymentStatusResponse>(graphQLResponse.Data["bookUpdatePaymentStatus"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in PaymentStatusGraphQLRequestHandler: {ex.Message}, CorrelationId: {AsyncContext.Current.CorrelationId}");
                throw;
            }
        }
    }
}
