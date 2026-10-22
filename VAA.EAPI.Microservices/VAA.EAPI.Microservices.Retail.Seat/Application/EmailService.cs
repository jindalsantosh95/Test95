using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Seat.Domain;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Seat.Mapper;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application
{
    public class EmailService : IEmailService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IMediator mediator, ILogger<EmailService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> AddEventForEmailReceiptAsync(OrderEmailRequest request)
        {
            bool eventCreated = false;
            var id = string.Empty;

            try
            {
                var cloudEvent = CloudEventMapper.Map(request);

                id = cloudEvent.Id;
               
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);

                var serviceBusCommand = new ServiceBusTopicMessageEventCommand()
                {
                    Message = cloudEvent,
                    ServiceBusTopicName = ConfigReader.Try(Constants.Environment.EmailMSServiceBusTopicName),
                    ServiceBusConnectionString = ConfigReader.Try(Constants.Environment.EmailServiceBusConnectionString)
                };
                await _mediator.Send(serviceBusCommand, CancellationToken.None);              
                eventCreated = true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");

                if (eventCreated)
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = SeatFulfilmentEventType.SeatPaymentReceiptEmailTrigger.ToString(), Data = request, Subject = Constants.Application.SeatPaymentReceiptEventSubject });
                
            }
            return eventCreated;
        }
    }
}
