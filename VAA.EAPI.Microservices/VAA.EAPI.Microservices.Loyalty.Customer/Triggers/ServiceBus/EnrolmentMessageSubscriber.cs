using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.ServiceBus
{

    public class EnrolmentMessageSubscriber
    {

        private readonly IMediator _mediator;

        public EnrolmentMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }


        [Function("EnrolmentEventSubscriber")]

        public async Task Run([ServiceBusTrigger(Constants.Application.EnrolmentEventHandlerTopicName, Constants.Application.EnrolmentEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            var enrolmentEventRequest = EnrolmentSubscriptionReqMapper.EnrolmentEventRequestMapper(cloudEvent);
            await _mediator.Send(enrolmentEventRequest);
        }

        [Function("DeDupEventSubscriber")]
        public async Task DeDupValidateSubscriberAsync([ServiceBusTrigger(Constants.Application.EnrolmentEventHandlerTopicName, Constants.Application.DeDupValidateSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var command = Json.Deserialize<VaAccountEnrolmentCommand>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(command);
            await _mediator.Send(command);
        }

        [Function("VacEnrolmentEventSubscriber")]
        public async Task VacEnrol([ServiceBusTrigger(Constants.Application.EnrolmentEventHandlerTopicName, Constants.Application.VacEnrolmentEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var command = Json.Deserialize<VacEventEnrolmentCommand>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(command);
            await _mediator.Send(command);
        }
    }
}
