using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Mapper.InflightOrders;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderAssociation
{
    class OrderAssociationEventCommandHandler : IRequestHandler<OrderAssociationEventCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderAssociationEventCommandHandler> _logger;
        public OrderAssociationEventCommandHandler(IMediator mediator, ILogger<OrderAssociationEventCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        public async Task<Result<Response<string>>> Handle(OrderAssociationEventCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Customer Order Association EventCommandHandler");
            string guid = Guid.NewGuid().ToString();
            try
            {
                // Store the Event in event Storage
                _logger.LogInformation($"Customer Order Association EventCommandHandler | Cloud event reqeust:  {Json.Serialize(request.cloudEvent)}");
                await _mediator.Send(new EventRequestInsertCommand { Id = request.cloudEvent.Id, Type = request.cloudEvent.Type, Data = request.cloudEvent.Data, Subject = request.cloudEvent.Subject });

                // Publish the Event 
                await _mediator.Publish(request.cloudEvent);
                _logger.LogInformation($"Customer Order Association EventCommandHandler | Cloud event inserted successfully in Azure table storage:  {Json.Serialize(request.cloudEvent)}");
                var orderAssociationResponse = OrderAssociationEventCommandResponseMapper.OrderAssociationResponseMapper(request.cloudEvent.Id);
                return orderAssociationResponse != null ? Result.Success(orderAssociationResponse) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
            }
            catch (Exception ex)
            {
                // Rollback from EventStore
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = request.cloudEvent.Id, Type = request.cloudEvent.Type, Data = request.cloudEvent.Data, Subject = request.cloudEvent.Subject });
                _logger.LogInformation($"Customer Order Association EventCommandHandler || ErrorId {guid} ||  Exception: {ex.Message} StackTrace : {ex.StackTrace}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, guid, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }

        }
    }
}
