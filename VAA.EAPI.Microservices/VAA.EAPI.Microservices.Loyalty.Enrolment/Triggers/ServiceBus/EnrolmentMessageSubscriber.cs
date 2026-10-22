using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Triggers.ServiceBus
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
            var log = context.GetLogger("HttpFunction");
            log.LogInformation("Enrolment Event received");
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            var enrolmentEventRequest = EnrolmentSubscriptionReqMapper.EnrolmentEventRequestMapper(cloudEvent);
            await _mediator.Send(enrolmentEventRequest);
        }
    }
}
