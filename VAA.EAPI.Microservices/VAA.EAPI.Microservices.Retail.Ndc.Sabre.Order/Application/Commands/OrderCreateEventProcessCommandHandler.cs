using Azure.Messaging.ServiceBus;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands
{
    public class OrderCreateEventProcessCommandHandler : IRequestHandler<OrderCreateEventProcessCommand, Result<Unit>>
    {
        private readonly ILogger<OrderCreateEventProcessCommandHandler> _logger;
        private readonly OrderCreateRepoCommandHandler _orderCreateRepoCommandHandler;
        private readonly ServiceResponseInsertCommandHandler _serviceResponseInsertCommandHandler;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;

        public OrderCreateEventProcessCommandHandler(ILogger<OrderCreateEventProcessCommandHandler> logger, OrderCreateRepoCommandHandler orderCreateRepoCommandHandler, ServiceResponseInsertCommandHandler serviceResponseInsertCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, ServiceBusClient serviceBusClient, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderCreateRepoCommandHandler = orderCreateRepoCommandHandler ?? throw new ArgumentNullException(nameof(orderCreateRepoCommandHandler));
            _serviceResponseInsertCommandHandler = serviceResponseInsertCommandHandler ?? throw new ArgumentNullException(nameof(serviceResponseInsertCommandHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        }

        public async Task<Result<Unit>> Handle(OrderCreateEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orderCreateRequest = Json.Deserialize<OrderCreateRequest>(Json.Serialize(request.Data));
                var orderResponse = await _orderCreateRepoCommandHandler.Handle(new OrderCreateRepoCommand { OrderCreateRequest = orderCreateRequest }, cancellationToken);

                if (orderResponse.IsSuccess)
                {
                    var orderDocument = OrderDocumentMapper.Mapper(orderCreateRequest, (orderResponse.Value.Value));

                    await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(orderDocument), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderCreateEventSubject }, cancellationToken);

                    var trimmedOrderDoc = TrimOrderDocument(orderDocument);
                    var cloudEvent = OrderCreateDataMapper.EventMapper(trimmedOrderDoc, request.Id);

                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.OrderDataServiceBusTopicName) }, cancellationToken);

                    if (orderCreateRequest.TimeLimits?.Any() == true)
                    {
                        var requestForTtl = (TtlExpiryRequest)(orderCreateRequest, orderDocument);

                        await using var ttlSender = _serviceBusClient.CreateSender(ConfigReader.Get(Constants.Environment.TtlExpiryTopicName));

                        var ttlLimit = DateTimeOffset.Parse(
                           orderCreateRequest.TimeLimits.First().Limit,
                           System.Globalization.CultureInfo.InvariantCulture,
                           System.Globalization.DateTimeStyles.RoundtripKind);

                        var proactiveTime = ttlLimit.AddHours(-24);
                        var proactiveEvent = new VAA.EAPI.Domain.Events.CloudEvent
                        {
                            Data = requestForTtl,
                            Type = Constants.Application.ProactiveTtlExpiryEventSubject,
                            Subject = Constants.Application.ProactiveTtlExpiryEventSubject,
                            Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
                        };

                        await ttlSender.ScheduleMessageAsync(
                            new ServiceBusMessage(Json.Serialize(proactiveEvent, false))
                            {
                                Subject = proactiveEvent.Type,
                                MessageId = $"{request.Id}-proactive"
                            }.AddEapiTraceContext(),
                            proactiveTime,
                            cancellationToken);


                        // after expiry
                        var reactiveEvent = new VAA.EAPI.Domain.Events.CloudEvent
                        {
                            Data = requestForTtl,
                            Type = Constants.Application.ReactiveTtlExpiryEventSubject,
                            Subject = Constants.Application.ReactiveTtlExpiryEventSubject,
                            Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
                        };

                        await ttlSender.ScheduleMessageAsync(
                            new ServiceBusMessage(Json.Serialize(reactiveEvent, false))
                            {
                                Subject = reactiveEvent.Type,
                                MessageId = $"{request.Id}-reactive"
                            }.AddEapiTraceContext(),
                            ttlLimit,
                            cancellationToken);
                    }
                }
                else
                {
                    await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(orderResponse), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderCreateEventSubject }, cancellationToken);
                    throw new Exception("Exception in OrderCreate Event Process Command Handler while creating PNR");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrderCreate event processing failed; event-store request will be rolled back. {EventId}", request.Id);
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = request.Id, Type = Constants.Application.OrderCreateEventType }, cancellationToken);
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
