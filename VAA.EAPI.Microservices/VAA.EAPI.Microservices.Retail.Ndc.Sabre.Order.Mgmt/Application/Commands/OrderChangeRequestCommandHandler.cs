using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Validations;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    public class OrderChangeRequestCommandHandler : IRequestHandler<OrderChangeRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeRequestCommandHandler> _logger;

        public OrderChangeRequestCommandHandler(IMediator mediator, ILogger<OrderChangeRequestCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<Response<string>>> Handle(OrderChangeRequestCommand request, CancellationToken cancellationToken)
        {
            bool eventCreated = false;
            var id = string.Empty;

            try
            {
                var trimmedData = TrimOrderChangeCloudEventData(request);
                var orderChangeCloudEvent = OrderChangeCommandToEventMapper.CommandToEventMapper(request);

                id = orderChangeCloudEvent.Id;

                var result = OrderChangeRequestValidator.ValidateRequest(request);

                if (result.Value.Status == Status.OK)
                {
                    await _mediator.Send(new EventRequestInsertCommand { Id = orderChangeCloudEvent.Id, Type = orderChangeCloudEvent.Type, Data = trimmedData, Subject = orderChangeCloudEvent.Subject });
                    try
                    {
                         await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = orderChangeCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicName) }, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Order change event processing failed for {EventId}", orderChangeCloudEvent.Id);
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = orderChangeCloudEvent.Id, Type = orderChangeCloudEvent.Type, Data = trimmedData, Subject = orderChangeCloudEvent.Subject }, cancellationToken);

                        throw;
                    }
                }
                else
                {
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rolling back event store for {EventId} after order change request handling failed", id);

                if (eventCreated)
                    await _mediator.Send(new EventStoreRequestDeleteCommand
                    {
                        Id = id,
                        Type = OrderEventType.OrderChangeInitiate.ToString(),
                        Data = request,
                        Subject = Constants.Application.OrderChangeEventSubject
                    });

                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }

            var orderChangeResponse = GetOrderChangeResponse(id);

            return orderChangeResponse != null ? Result.Success(orderChangeResponse) : Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
        }

        private static Response<string> GetOrderChangeResponse(string corelationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.OrderChangeResponseUrl).Replace("{correlationid}", corelationid) } },
            };
        }

        private static OrderChangeRequestCommand TrimOrderChangeCloudEventData(OrderChangeRequestCommand request)
        {
            return new OrderChangeRequestCommand
            {
                Rloc = request.Rloc,
                DataList = new Domain.OrderChange.DataList()
                {
                    SegmentDetails = request.DataList.SegmentDetails,
                    PriceClass = request.DataList.PriceClass,
                    Penalty = request.DataList.Penalty,
                    CustomerDetails = request.DataList.CustomerDetails,
                    OriginDestinationAssociations = request.DataList.OriginDestinationAssociations,
                },
                Parties = request.Parties,
                Pos = request.Pos,
                SelectedOffers = request.SelectedOffers
            };
        }
    }
}



