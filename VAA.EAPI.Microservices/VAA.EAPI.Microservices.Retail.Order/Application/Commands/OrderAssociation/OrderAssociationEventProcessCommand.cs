using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.CosmosGremlinAPI;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Interface;
using VAA.EAPI.Microservices.Retail.Order.Mapper.InflightOrders;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderAssociation
{
    public class OrderAssociationEventProcessCommand : CloudEvent, IRequest<Result<Response<string>>>
    {
        public new OrderAssociationRequestData Data { get; set; }
    }

    public class OrderAssociationEventProcessCommandhandler : IRequestHandler<OrderAssociationEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderAssociationEventProcessCommandhandler> _logger;
        private readonly ICachedAirport _cachedAirport;
        private readonly ICachedCarrier _cachedCarrier;

        public OrderAssociationEventProcessCommandhandler(IMediator mediator,
             ICachedAirport cachedAirport,
             ICachedCarrier cachedCarrier,
             ILogger<OrderAssociationEventProcessCommandhandler> logger)
        {
            _mediator = mediator;
            _cachedAirport = cachedAirport;
            _cachedCarrier = cachedCarrier;
            _logger = logger;

        }
        public async Task<Result<Response<string>>> Handle(OrderAssociationEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Order association Event Subscribe command handler, id:" + request.Id);

                //var requestData = Json.DeserializeObject<OrderAssociationRequestData>((JToken)request.Data.ToString());
                var airportData = _cachedAirport.GetCachedAirport();
                var carrierData = _cachedCarrier.GetCachedCarrier();
                CosmosGremlinRepoCommand cmdRepo = new()
                {
                    GraphService = "CreateGraph",
                    GraphTraversalItems = OrderAssociationDataToCosmosGraphObjectsMapper.GetGraphModel(request.Data, airportData, carrierData)
                };

                var orderAssociationResponse = await _mediator.Send(cmdRepo, cancellationToken);

                _logger.LogInformation($"Status of Event of request with id is:  {orderAssociationResponse.Status.ToString()} With result:  {orderAssociationResponse.Value}");

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(orderAssociationResponse), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderAssociationEventSubject });

                return orderAssociationResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in OrderAssociation Event Subscribe Command Handler");
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = ex.StackTrace, Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderAssociationEventSubject });
                return Result.Failure<Response<string>>(ex.Message);
            }
        }
    }
}
