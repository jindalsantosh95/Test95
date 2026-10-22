using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events
{
    public class ManageCustomerEventSubscribeCommandHandler : IRequestHandler<ManageCustomerEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManageCustomerEventSubscribeCommandHandler> _logger;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;

        public ManageCustomerEventSubscribeCommandHandler(IMediator mediator, ILogger<ManageCustomerEventSubscribeCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentException(nameof(serviceBusTopicMessageEventCommandHandler));
        }

        public async Task<Result<Unit>> Handle(ManageCustomerEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var manageCustomerReq = Json.Deserialize<ManageCustomerRequest>(Json.Serialize(request.Data));
                var orderResponse = await _mediator.Send(new ManageCustomerRepoCommand { ManageCustomerRequest = manageCustomerReq }, cancellationToken);

                if (orderResponse.IsSuccess)
                {
                    var orderDocument = orderResponse.Value.Value;

                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(orderDocument), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.ManageCustomerEventSubject }, cancellationToken);

                    if (manageCustomerReq.TicketedNameCorrection || manageCustomerReq.UnTicketedNameCorrection)
                    {
                        var cloudEvent = ManageCustomerCommandToEventMapper.CommandToCloudEventMappper(orderDocument, request.Id);
                       
                        await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString), ServiceBusTopicName = Constants.Application.NDCNameUpdateEventHandlerTopicName }, cancellationToken);

                        if(manageCustomerReq.Pos.CountryCode == "US")
                        {
                            await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString), ServiceBusTopicName = Constants.Application.NDCArcDocumentUpdateSubscriberTopicName }, cancellationToken);
                        }
                    }
                }

                else
                {
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(orderResponse), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.ManageCustomerEventSubject }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manage customer event processing failed for {EventId}", request.Id);
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.Id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.ManageCustomerEventSubject }, cancellationToken);
                throw;
            }
            
            return Unit.Value;
        }
    }
}
