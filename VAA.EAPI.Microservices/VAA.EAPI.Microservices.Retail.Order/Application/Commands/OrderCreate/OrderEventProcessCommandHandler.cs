using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;

using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.Insurance;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.SCVResults;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order.CustomerEnrollment;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.OrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Mapper.OrderCreate;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate
{
    public class OrderEventProcessCommandHandler : IRequestHandler<OrderEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderEventProcessCommandHandler> _logger;
        private readonly IMetaPartnerEventCommandHandler _metaPartnerEventCommandHandler;
        private readonly IServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public OrderEventProcessCommandHandler(IMediator mediator, ILogger<OrderEventProcessCommandHandler> logger, IMetaPartnerEventCommandHandler metaPartnerEventCommandHandler, IServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metaPartnerEventCommandHandler = metaPartnerEventCommandHandler ?? throw new ArgumentNullException(nameof(metaPartnerEventCommandHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Result<Unit>> Handle(OrderEventProcessCommand request, CancellationToken cancellationToken)
        {
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderCreateEventSubject
            };

            try
            {
                var orderCreateRequest = Json.Deserialize<OrderCreateRequest>(Convert.ToString(request.Data));

                trackAnalyticsEventCommand.Track("OfferIds", string.Join(',', orderCreateRequest.SelectedOffers.Select(x => x.OfferId)));

                var response = await _mediator.Send(new OrderCreateServiceCommand { Request = orderCreateRequest }, cancellationToken);

                trackAnalyticsEventCommand.Track("OrderCreateStatus", response.IsSuccess.ToString());

                if (response.IsSuccess)
                {
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderCreateEventSubject }, cancellationToken);

                    var orderDocument = OrderDocumentMapper.MapOrderDocument(response.Value.Value, Constants.Application.OrderDocumentType, orderCreateRequest);

                    await _mediator.Send(new OrderDocumentCommand { OrderDocument = orderDocument }, cancellationToken);
                    if (orderCreateRequest.CustomerDetails.First().Preferences.PreferenceValue == "Y")
                    {
                        var cloudEventData = OrderDocumentMapper.MapCustomerDetails(orderCreateRequest);
                        var cloudEvent = OrderCreateCommandToEventMapper.EventMapper(cloudEventData, request.Id);

                        await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.CustomerOrderEnrollmentServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.CustomerOrderEnrollmentServiceBusTopicName) }, cancellationToken);
                    }
                    var enableChildMeal = ConfigReader.Try(Constants.Environment.EnableChildMeal, false);

                    if (enableChildMeal)
                    {
                        var childSpecialServiceRq = OrderDocumentMapper.OrderChildSsrMapping(orderCreateRequest, response.Value.Value);

                        if (childSpecialServiceRq != null)
                        {
                            var cloudEvent = OrderCreateCommandToEventMapper.EventMapper(childSpecialServiceRq, request.Id);

                            await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.OrderChangeServiceBusTopicName }, cancellationToken);
                        }
                    }

                    if (response.Value.Value.Warnings != null && response.Value.Value.Warnings.Any(x => x.Code == Constants.Application.WarningALLSEATFULFILMENTFAILED || x.Code == Constants.Application.WarningPARTIALSEATFULFILMENT))
                    {
                        _logger.LogError($"SEAT FULLFILMENT FAILED. PNR-{response.Value.Value.BookingReference.RLOC}");
                    }

                    if (orderCreateRequest.IsInsuranceAdded.HasValue && orderCreateRequest.IsInsuranceAdded.Value)
                    {
                        await _mediator.Send(new InsurancePurchaseCommand { InsurancePurchaseRequest = OrderCreateCommandToInsuranceEventMapper.GetPurchasesRequest(orderCreateRequest, response.Value.Value), CorrelationId = AsyncContext.Current.CorrelationId, Channel = AsyncContext.Current.Channel, SessionId = AsyncContext.Current.SessionId, DiagnosticsEnabled = AsyncContext.Current.DiagnosticsEnabled }, cancellationToken);
                    }
                    else
                    {
                        if (orderCreateRequest.IsInsuranceAdded.HasValue)
                        {
                            //send insurance decision to Allianz
                            var cloudEvent = OrderCreateCommandToInsuranceEventMapper.DecisionRequestMapper(orderCreateRequest, response.Value.Value);
                            await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicInsuranceDecision) }, cancellationToken);
                        }

                        var paymentReferenceId = orderCreateRequest.PaymentDetails.FirstOrDefault(a => !a.PaymentReferenceId.IsNullOrEmpty())?.PaymentReferenceId;
                        var enableClearCVV = ConfigReader.Try(Constants.Environment.EnableClearCVV, false);
                        //send event to clear payment details
                        if (enableClearCVV && paymentReferenceId != null)
                        {
                            var paymentEvent = PaymentDetailsMapper.ClearPaymentDetails(orderCreateRequest, paymentReferenceId);
                            await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = paymentEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicClearPaymentDetails) }, cancellationToken);
                        }
                    }

                    trackAnalyticsEventCommand.Track("Passengers", string.Join(',', response.Value.Value.DataList.CustomerDetails.GroupBy(x => x.Ptc).Select(x => $"{x.Key}: {x.Count()}")));
                    trackAnalyticsEventCommand.Track("OriginToDestination", string.Join(',', response.Value.Value.DataList.OriginDestinationAssociations.Select(x => $"{x.OriginDestination.OriginAirport} => {x.OriginDestination.DestinationAirport}")));
                    trackAnalyticsEventCommand.Track("DepartureDates", string.Join(',', response.Value.Value.DataList.OriginDestinationAssociations.Select(x => x.OriginDestination.DepartureDate)));
                    trackAnalyticsEventCommand.Track("FlightType", response.Value.Value.DataList.OriginDestinationAssociations.Count() > 1 ? "Return" : "OneWay");
                    trackAnalyticsEventCommand.Track("BookingReference", response.Value.Value.BookingReference.RLOC);
                    trackAnalyticsEventCommand.Track("OrderId", response.Value.Value.OrderNumber?.OrderId ?? string.Empty);

                    // Feature Flag to enable Meta Partner Event
                    if (ConfigReader.Try(Constants.Environment.EnableMetaPartnerEvent, false))
                    {
                        var metaPartnerId = orderCreateRequest.MetaPartner?.GetValueOrDefault("MetaPartnerId");
                        if (!metaPartnerId.IsNullOrWhiteSpace())
                        {
                            var metaPartnerEventCommand = (MetaPartnerEventCommand)(MetaPartnerCommand)(orderCreateRequest, orderDocument);
                            trackAnalyticsEventCommand.Track("MetaPartnerId", metaPartnerId);

                            // send event to notify meta partners
                            await _metaPartnerEventCommandHandler.Handle(metaPartnerEventCommand, cancellationToken);
                        }
                    }

                    await PublishVoucherRedemptionRemarksAsync(orderCreateRequest, response.Value.Value, cancellationToken);

                }
                else
                {
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderCreateEventSubject }, cancellationToken);
                    return Result.Failure<Unit>("Order event processing encountered an error.");
                }

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                return Result.Failure<Unit>("Order event processing encountered an error.");
            }
            finally
            {
                await _trackAnalyticsEventCommandHandler.Handle(trackAnalyticsEventCommand, cancellationToken);
            }
        }

        private async Task PublishVoucherRedemptionRemarksAsync(OrderCreateRequest orderCreateRequest, OrderResponse orderResponse, CancellationToken cancellationToken)
        {
            if (!orderCreateRequest.VoucherApplied)
            {
                return;
            }

            var specialRemarkRequest = OrderDocumentMapper.MapVoucherRedemptionRemark(orderCreateRequest, orderResponse);

            if (specialRemarkRequest is null)
            {
                _logger.LogWarning("Voucher remarks were not published because no applied vouchers were supplied. CorrelationId: {CorrelationId}", AsyncContext.Current.CorrelationId);
                return;
            }

            try
            {
                var cloudEvent = OrderCreateCommandToEventMapper.EventMapper(specialRemarkRequest, Guid.NewGuid().ToString());

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                {
                    Message = cloudEvent,
                    ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                    ServiceBusTopicName = Constants.Application.OrderChangeServiceBusTopicName
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Voucher remarks event could not be published. CorrelationId: {CorrelationId}",
                    AsyncContext.Current.CorrelationId);
            }
        }
    }
}
