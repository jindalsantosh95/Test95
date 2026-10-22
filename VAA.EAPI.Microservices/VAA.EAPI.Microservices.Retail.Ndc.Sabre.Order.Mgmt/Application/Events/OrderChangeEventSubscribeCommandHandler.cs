using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events
{
    public class OrderChangeEventSubscribeCommandHandler : IRequestHandler<OrderChangeEventProcessCommand, Unit>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeEventSubscribeCommandHandler> _logger;
        private readonly IOrderChangeAcceptTimeChangeDisruptionCommand _timeChangeDisruptionCommand;
        private readonly IOrderChangeAcceptCancellationDisruptionCommand _cancellationDisruptionCommand;
        private readonly IOrderChangeReshopDisruptionCommand _reshopDisruptionCommand;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly IOrderChangeReshopHoldBookingCommand _orderChangeReshopHoldBookingCommand;

        public OrderChangeEventSubscribeCommandHandler(IMediator mediator, ILogger<OrderChangeEventSubscribeCommandHandler> logger, IOrderChangeAcceptTimeChangeDisruptionCommand timeChangeDisruptionCommand, IOrderChangeAcceptCancellationDisruptionCommand cancellationDisruptionCommand, IOrderChangeReshopDisruptionCommand reshopDisruptionCommand, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, IOrderChangeReshopHoldBookingCommand orderChangeReshopHoldBookingCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeChangeDisruptionCommand = timeChangeDisruptionCommand;
            _cancellationDisruptionCommand = cancellationDisruptionCommand;
            _reshopDisruptionCommand = reshopDisruptionCommand;
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _orderChangeReshopHoldBookingCommand = orderChangeReshopHoldBookingCommand;
        }

        public async Task<Unit> Handle(OrderChangeEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Result<Response<OrderDocument>> orderResponse = null;
                var orderChangeRequestData = Json.Deserialize<OrderChangeRequestData>(request.Data.ToString());

                if (orderChangeRequestData.IsReshopDisruption && !orderChangeRequestData.IsChangeReshopConfirmHoldBooking && !orderChangeRequestData.IsChangeReshopHoldBooking)
                    orderResponse = await _reshopDisruptionCommand.Handle(orderChangeRequestData, cancellationToken);
                else if(orderChangeRequestData.ChangeType == Domain.ChangeTypeEnum.FlightDateTimeChange)
                    orderResponse = await _timeChangeDisruptionCommand.Handle(orderChangeRequestData, cancellationToken);
                else if(orderChangeRequestData.ChangeType == Domain.ChangeTypeEnum.FlightCancellation)
                    orderResponse = await _cancellationDisruptionCommand.Handle(orderChangeRequestData, cancellationToken);
                else if (orderChangeRequestData.IsChangeReshopConfirmHoldBooking || orderChangeRequestData.IsChangeReshopHoldBooking)
                    orderResponse = await _orderChangeReshopHoldBookingCommand.Handle(orderChangeRequestData, cancellationToken);
                else
                    orderResponse = await _mediator.Send(new OrderChangeRepoCommand { orderChangeRequestData = orderChangeRequestData }, cancellationToken);

                if (orderResponse.IsSuccess)
                {
                    var orderDocument = orderResponse.Value.Value;

                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(orderDocument), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderChangeEventSubject }, cancellationToken);

                    if (orderChangeRequestData.ChangeType != Domain.ChangeTypeEnum.FlightDateTimeChange && orderChangeRequestData.ChangeType != Domain.ChangeTypeEnum.FlightCancellation && !orderChangeRequestData.IsReshopDisruption)
                    {
                        var trimmedOrderDoc = TrimOrderDocument(orderDocument);
                        var cloudEvent = OrderChangeCommandToEventMapper.EventMapper(trimmedOrderDoc, request.Id);

                        await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString), ServiceBusTopicName = Constants.Application.NDCOrderHistoryUpdateSubscriberTopicName }, cancellationToken);

                        if (orderChangeRequestData.Pos.CountryCode == "US" && !orderChangeRequestData.IsChangeReshopConfirmHoldBooking && !orderChangeRequestData.IsChangeReshopHoldBooking)
                        {
                            await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString), ServiceBusTopicName = Constants.Application.NDCArcDocumentUpdateSubscriberTopicName }, cancellationToken);
                        }

                    }
                }
                else
                {
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(orderResponse), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderChangeEventSubject }, cancellationToken);
                    throw new Exception("Exception in OrderChange Process");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order change event processing failed for {EventId}", request.Id);
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = request.Id, Type = Constants.Application.OrderChangeEventType }, cancellationToken);
            }
            return Unit.Value;
        }

        private static OrderDocument TrimOrderDocument(OrderDocument orderDocument)
        {
            foreach (var cust in orderDocument.DataList.CustomerDetails)
            {
                cust.GovtIssuedDocs = null;
                cust.DateOfBirth = null;
                cust.TravelDocuments = null;
                cust.Gender = null;
                cust.Addresses = null;
            }

            if (orderDocument.PaymentDetails?.CardMethod != null)
            {
                orderDocument.PaymentDetails.CardMethod.CardNumber = null;
                orderDocument.PaymentDetails.CardMethod.Cvv = null;
                orderDocument.PaymentDetails.CardMethod.ExpiryDate = null;
            }
            return orderDocument;
        }
    }
}
