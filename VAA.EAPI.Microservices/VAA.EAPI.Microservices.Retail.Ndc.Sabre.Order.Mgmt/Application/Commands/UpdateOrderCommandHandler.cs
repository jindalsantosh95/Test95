using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    public class UpdateOrderCommandHandler : IUpdateOrderCommand
    {
        private readonly ILogger<UpdateOrderCommandHandler> _logger;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
       
        public UpdateOrderCommandHandler(ILogger<UpdateOrderCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
           
        }

        public async Task<Result<Response<string>>> Handle(UpdateOrderRequest request, CancellationToken cancellationToken)
        {
            var cloudEventId = Guid.NewGuid().ToString();

            try
            {
                if (request.OrderDocument == null) throw new ArgumentNullException(nameof(request));

                var trimmedOrderDocument = TrimOrderDocument(request.OrderDocument);

                var updateOrderCloudEvent = UpdateOrderDataMapper.EventMapper(trimmedOrderDocument, cloudEventId);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = updateOrderCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.OrderDataServiceBusTopicName) }, cancellationToken);

                var response = UpdateOrderDataMapper.ResponseMapper();

                return response != null ? Result.Success(response) : Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update order command handling failed for {EventId}", cloudEventId);

                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }

        }

        public static OrderDocument TrimOrderDocument(OrderDocument orderDocument)
        {
            if (orderDocument.OrderHistory != null)
            {
                foreach (var history in orderDocument.OrderHistory)
                {
                    if (history.DataList?.CustomerDetails != null)
                    {
                        foreach (var customer in history.DataList.CustomerDetails)
                        {
                            customer.TravelDocuments = null;
                            customer.DateOfBirth = null;
                            customer.Gender = null;
                        }
                    }
                }
            }

            return orderDocument;
        }
    }
}
