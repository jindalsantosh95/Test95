using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Email.Application.Command;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.StoreAppointment;
using VAA.EAPI.Microservices.CRM.Email.Domain;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Triggers.ServiceBus
{
    public class EmailMessageSubsciber
    {
        private readonly IMediator _mediator;
        private readonly OrderEmailComandHandler _orderEmailComandHandler;
        public EmailMessageSubsciber(IMediator mediator, OrderEmailComandHandler orderEmailComandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderEmailComandHandler = orderEmailComandHandler ?? throw new ArgumentNullException(nameof(orderEmailComandHandler));
        }

        [Function("EmailSubscriber")]

        public async Task Run([ServiceBusTrigger(Constants.Constants.EmailEventHandlerTopicName, Constants.Constants.EmailEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("EmailMessageSubscriber");
            log.LogInformation("Email Event received");
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(EmailCommandRequestMapper.EventRequestMapper(cloudEvent));

        }
        [Function("WelcomeEmailSubscriber")]

        public async Task WelcomeEmailSubscriber([ServiceBusTrigger(Constants.Constants.EmailEventHandlerTopicName, Constants.Constants.WelcomeEmailEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(WelcomeEmailCommandRequestMapper.EventRequestMapper(cloudEvent));
        }

        [Function("OTPEmailSubscriber")]
        public async Task RunAsync([ServiceBusTrigger(Constants.Constants.EmailEventHandlerTopicName, Constants.Constants.OTPEmailEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(OTPEmailCommandRequestMapper.EventRequestMapper(cloudEvent));

        }

        [Function("ManageAccountEmailSubscriber")]
        public async Task ManageAccountEmailSubscriber([ServiceBusTrigger(Constants.Constants.ManageAccountTopicName, Constants.Constants.ManageAccountEmailSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(Json.DeserializeObject<AdobeEmailCommand>((JToken)cloudEvent.Data));
        }

        [Function("HouseHoldInviteEmailSubscriber")]
        public async Task HouseHoldInviteEmailSubscriber([ServiceBusTrigger(Constants.Constants.HouseholdAccountTopicName, Constants.Constants.HouseHoldInviteEmailSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(Json.DeserializeObject<AdobeMemberInviteEmailCommand>((JToken)cloudEvent.Data));
        }

        [Function("VACEnrolEmailSubscriber")]
        public async Task VACEnrolEmailSubscriber([ServiceBusTrigger(Constants.Constants.EmailEventHandlerTopicName, Constants.Constants.VACEnrolEmailSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(Json.DeserializeObject<VACEnrolEmailCommand>((JToken)cloudEvent.Data));
        }

        [Function("StoreAppointmentEmailSubscriber")]
        public async Task StoreAppointmentEmailSubscriber([ServiceBusTrigger(Constants.Constants.StoreAppointmentTopicName, Constants.Constants.StoreAppointmentEmailSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(StoreAppointmentEmailEventProcessCommand.FromCloudEvent(cloudEvent));
        }

        [Function("OrderEmailSubscriber")]
        public async Task OrderEmailSubscriber([ServiceBusTrigger(Constants.Constants.SeatconfirmationEmailMessageTopicName, Constants.Constants.SeatConfirmEmailSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var orderEmailCloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            AsyncContext.Current?.MapFromCloudEvent(orderEmailCloudEvent);
            await _orderEmailComandHandler.Handle(Json.Deserialize<OrderEmailCommand>((Json.Serialize(orderEmailCloudEvent.Data))), CancellationToken.None);
        }
    }
}
