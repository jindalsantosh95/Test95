using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Microservices.Retail.Seat.Domain;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Command
{
    public class SeatFulfilmentEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }

    public class SeatFulfilmentEventSubscriberCommandHandler : IRequestHandler<SeatFulfilmentEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeatFulfilmentEventSubscriberCommandHandler> _logger;
        private readonly IEmailService _emailService;

        public SeatFulfilmentEventSubscriberCommandHandler(IMediator mediator, ILogger<SeatFulfilmentEventSubscriberCommandHandler> logger, IEmailService emailService = null)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService;
        }

        public async Task<Result<Unit>> Handle(SeatFulfilmentEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = "", Status = ServiceResponseProcessStatus.Processing, Type = request.Type }, cancellationToken);

                var seatFulfilmentRequest = Json.Deserialize<SeatV8AddSeatQuery>(request.Data.ToString());

                var result = await _mediator.Send(seatFulfilmentRequest, cancellationToken);

                _logger.LogInformation($"Seat Fulfilment Response for Correlation Id - {AsyncContext.Current.CorrelationId} : {Json.Serialize(result)}"); // Log full response for debugging of SOA Response Reported on PRoducion
                var response = SeatFulFilmentResponseMapper.MapAddSeatResponse(result, seatFulfilmentRequest);

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Completed, Type = request.Type }, cancellationToken);

                if (result.Any(r => r.SeatSuccess != null) && seatFulfilmentRequest.ChannelId == Constants.Application.NdcChannel)
                    await _mediator.Send(new ServiceBusTopicMessageEventCommand() { Message = SeatFulfilmentCommandToEventMapper.CommandToEventMapper(SeatFulfilmentOrderDocUpdateMapper.UpdateOrderDocMapper(result?.First(r => r.SeatSuccess != null), seatFulfilmentRequest), request.Id) , ServiceBusTopicName = Constants.Application.NDCUpdateSeatTopicName, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderServiceBusConnectionString) }, cancellationToken);
                else if(seatFulfilmentRequest.paymentDetails != null && seatFulfilmentRequest.ChannelId != Constants.Application.NdcChannel)
                {                    
                    try
                    {
                        ArgumentNullException.ThrowIfNull(_emailService, "Email service is not configured for sending seat payment receipt email.");
                        ArgumentNullException.ThrowIfNull(seatFulfilmentRequest.paymentDetails.FirstOrDefault(a => a.Payer?.PayerContacts?.Email.IsNullOrWhiteSpace() == false), "No Email Address Found for Payer");

                        await _emailService.AddEventForEmailReceiptAsync((OrderEmailRequest)seatFulfilmentRequest);                      

                    }
                    catch (Exception exEmail)
                    {
                        _logger.LogError("Issue in triggering email receipt For PNR: " + seatFulfilmentRequest.BookingReferences.RLOC, exEmail);
                    }                                        
                }
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaSeatV8ErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message)), Status = ServiceResponseProcessStatus.Error, Type = request.Type }, cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.ExceptionErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)), Status = ServiceResponseProcessStatus.Completed, Type = request.Type }, cancellationToken);
                throw;
            }

            return Unit.Value;
        }
    }
}
