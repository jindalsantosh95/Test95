using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.CustomerAssignAward;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.ServiceBus
{
    public class CustomerAssignAwardSubscriber
    {
        private readonly IMediator _mediator;
        public CustomerAssignAwardSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("CustomerAssignAwardSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CustomerAssignAwardTopicName, Constants.Application.CustomerAssignAwardSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("CustomerAssignAwardSubscriber");
            log.LogInformation("Message received");
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            var request = AssignAwardCommandMapper.Mapper(cloudEvent);
            await _mediator.Send(request);
        }
    }
}
