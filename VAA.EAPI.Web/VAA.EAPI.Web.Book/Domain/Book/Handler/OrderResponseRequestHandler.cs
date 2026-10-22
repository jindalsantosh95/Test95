using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.Book.Requests;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Mapper;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Domain.Book.Handler
{
    public class OrderResponseRequestHandler : IRequestHandler<OrderResponseRequest, ConfirmationResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderResponseRequestHandler> _logger;

        public OrderResponseRequestHandler(IMediator mediator, ILogger<OrderResponseRequestHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConfirmationResponse> Handle(OrderResponseRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var graphQLRequest = new GraphQLRequest
                {
                    PqId = ConfigReader.Get(Constants.Environment.OrderResponsePqId),
                    Variables = JObject.Parse(Json.Serialize(new
                    {
                        basketId = request.BasketId
                    })),
                    Headers = Utility.GetHeaders(request.AuthToken)
                };

                var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);
                return CreateOrderMapper.MapOrderCreateResponse(Json.DeserializeObject<ConfirmationResponse>(graphQLResponse.Data["orderResponse"]));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OrderResponseHandler: {ex.Message}, CorrelationId: {AsyncContext.Current.CorrelationId}");
                throw;
            }
        }
    }
}
