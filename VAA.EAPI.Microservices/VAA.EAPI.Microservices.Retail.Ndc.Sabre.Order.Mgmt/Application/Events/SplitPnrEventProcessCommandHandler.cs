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
    public class SplitPnrEventProcessCommandHandler : ISplitPnrEventProcessCommand
    {

        private readonly ISplitPnrRepoCommand _splitPnrRepoCommand;
        private readonly ServiceResponseInsertCommandHandler _serviceResponseInsertCommandHandler;
        private readonly ILogger<SplitPnrEventProcessCommandHandler> _logger;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;

        public SplitPnrEventProcessCommandHandler(ISplitPnrRepoCommand splitPnrRepoCommandHandler, ServiceResponseInsertCommandHandler serviceResponseInsertCommandHandler, ILogger<SplitPnrEventProcessCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler)
        {
            _splitPnrRepoCommand = splitPnrRepoCommandHandler;
            _serviceResponseInsertCommandHandler = serviceResponseInsertCommandHandler ?? throw new ArgumentNullException(nameof(serviceResponseInsertCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentException(nameof(serviceBusTopicMessageEventCommandHandler));
        }

        public async Task<Result<Unit>> Handle(EAPI.Domain.Events.CloudEvent request, CancellationToken cancellationToken)
        {
            try
            {
                var SplitPnrReq = Json.Deserialize<SplitPnrRequest>(Json.Serialize(request.Data));
                var orderResponse = _splitPnrRepoCommand.Handle(SplitPnrReq, cancellationToken);

                if (orderResponse.Result.IsSuccess)
                {
                    var parentOrderDocument = SplitPnrResponseMapper.SplitParentOrderDocument(orderResponse.Result.Value.Value, SplitPnrReq);

                    var trimmedData = SplitPnrCommandToEventMapper.TrimOrderDocumentData(parentOrderDocument);

                    await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(trimmedData), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.SplitPnrEventSubject }, cancellationToken);

                    foreach (var cust in parentOrderDocument.DataList.CustomerDetails)
                    {
                        cust.GovtIssuedDocs = null;
                        cust.DateOfBirth = null;
                        cust.TravelDocuments = null;
                        cust.Gender = null;
                        cust.Addresses = null;
                    }

                    var parentCloudEvent = SplitPnrCommandToEventMapper.EventMapper(parentOrderDocument, request.Id);
                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = parentCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.OrderDataServiceBusTopicName) }, cancellationToken);

                    var version = parentOrderDocument.Version;
                    var childOrderDocument = SplitPnrResponseMapper.SplitChildOrderDocument(orderResponse.Result.Value.Value, SplitPnrReq, version);

                    foreach (var cust in childOrderDocument.DataList.CustomerDetails)
                    {
                        cust.GovtIssuedDocs = null;
                        cust.DateOfBirth = null;
                        cust.TravelDocuments = null;
                        cust.Gender = null;
                        cust.Addresses = null;
                    }

                    var childCloudEvent = SplitPnrCommandToEventMapper.EventMapper(childOrderDocument, request.Id);
                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = childCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.OrderDataServiceBusTopicName) }, cancellationToken);
                }
                else
                {
                    await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(orderResponse), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.SplitPnrEventSubject }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Split PNR event processing failed for {EventId}", request.Id);
                await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.Id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.SplitPnrEventSubject }, cancellationToken);
                throw;
            }
            return Unit.Value;
        }
    }
}
