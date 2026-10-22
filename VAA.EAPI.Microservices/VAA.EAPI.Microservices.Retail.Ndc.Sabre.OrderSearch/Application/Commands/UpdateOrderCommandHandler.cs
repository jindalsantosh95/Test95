using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands
{
    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateOrderCommandHandler> _logger;
        private readonly string _correlationId;

        public UpdateOrderCommandHandler(IMediator mediator, ILogger<UpdateOrderCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationId = AsyncContext.Current.CorrelationId.ToString();
        }

        public async Task<Result<Response<string>>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var eventId = string.Empty;

            try
            {
                if (request.UpdateOrderRequest.OrderDocument == null)
                    throw new ArgumentNullException(nameof(request));

                var trimmedOrderDocument = TrimOrderDocument(request.UpdateOrderRequest.OrderDocument);

                var updateOrderCloudEvent = UpdateOrderDataMapper.EventMapper(trimmedOrderDocument, _correlationId);
                eventId = updateOrderCloudEvent.Id;

                await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = updateOrderCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.OrderDataServiceBusTopicName) }, cancellationToken);

                var response = UpdateOrderDataMapper.ResponseMapper(updateOrderCloudEvent.Id);

                return response != null ? Result.Success(response) : Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateOrder command failed. {EventId}", eventId);

                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }

        }

        public static OrderDocument TrimOrderDocument(OrderDocument orderDocument)
        {
            if (orderDocument.PaymentDetails != null)
            {
                orderDocument.PaymentDetails.CardMethod = null;
                orderDocument.PaymentDetails.BillingAddress = null;
                orderDocument.PaymentDetails.VoucherMethod = null;
                orderDocument.PaymentDetails.PaymentCardAuthentication = null;
                orderDocument.PaymentDetails.PaymentCardAuthorization = null;
                orderDocument.PaymentDetails.Payer = null;
            }

            if(orderDocument.DataList != null)
            {
                foreach (var customer in orderDocument.DataList.CustomerDetails)
                {
                    customer.GovtIssuedDocs = null;
                    customer.DateOfBirth = null;
                    customer.TravelDocuments = null;
                    customer.Gender = null;
                    customer.Addresses = null;
                }
            }

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
