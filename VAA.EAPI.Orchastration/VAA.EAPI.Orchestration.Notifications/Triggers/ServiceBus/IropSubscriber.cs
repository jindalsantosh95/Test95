using Microsoft.Azure.Functions.Worker;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Orchestration.Notifications.Application.Events;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop;

namespace VAA.EAPI.Orchestration.Notifications.Triggers.ServiceBus
{
    public class IropSubscriber(IIropEventCommand iIropEventCommand)
    {
        private readonly IIropEventCommand _iIropEventCommand = iIropEventCommand;

        [Function("IropOrchestratorSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Constants.IropNotificationTopicName, Constants.Constants.IropSubscriptionName, Connection = Constants.Environment.NotificationServiceBusConnectionString)] string eventRequestPayload, CancellationToken cancellationToken)
        {
            var bizTalkIropSbEvent = Xml.Deserialize<Ctx>(eventRequestPayload);

            await _iIropEventCommand.Handle(bizTalkIropSbEvent, cancellationToken);
        }
    }
}
