using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCreate
{
    public class OrderCreateRequestCommandHandler : IRequestHandler<OrderCreateRequestCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateRequestCommandHandler> _logger;

        public OrderCreateRequestCommandHandler(IMediator mediator, ILogger<OrderCreateRequestCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Unit>> Handle(OrderCreateRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));
                var req = Json.DeserializeObject<OrderCreateRequestData>((JToken)request.Data);

                //Create Order
                bool status = await CreateOrder(req);
            }
            catch (CosmosDBException ex)
            {
                _logger.LogError(ex, "Order create failed in Cosmos DB with error code {ErrorCode} for event {EventId}", ErrorCodes.CosmosErorrCode, request?.Id); throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order create failed with error code {ErrorCode}, exception type {ExceptionType} for event {EventId}",
                    ErrorCodes.UnableToCreate, ex.GetType().Name, request?.Id); throw;

            }
            return Unit.Value;
        }

        public async Task<bool> CreateOrder(OrderCreateRequestData request)
        {
            await _mediator.Send(new OrderDocRepositoryCommand(request));

            if ((bool)request?.NotificationRequest.FulfillmentDetails?.Any(x => x.IdType == OrderFulfillmentType.PNR))
            {
                await _mediator.Send(new PnrSearchDocRepositoryCommand(request));
            }

            return true;
        }
    }
}
