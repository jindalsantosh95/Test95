using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet;
using VAA.EAPI.Microservices.Loyalty.Wallet.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Triggers.ServiceBus
{
    public class WalletMessageSubscriber
    {
        private readonly IMediator _mediator;

        public WalletMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("WalletMessageSubscriber")]
        public async Task Run1([ServiceBusTrigger(Constants.Application.AppleWalletTierEventHandlerTopicName, Constants.Application.CustomerTierSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {            
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(WalletSubToCommandMapper.WalletEventRequestMapper(cloudEvent));
        }

        [Function("MemberTierUpdateEventSubscriber")]
        public async Task MemberTierUpdateEvent([ServiceBusTrigger(Constants.Application.MemberTierUpdateEventTopicName, Constants.Application.WalletTierUpdateSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(WalletSubToCommandMapper.MemberTierUpdateEventRequestMapper(cloudEvent));
        }

        [Function("WalletNotificationUpdateSubscriber")]
        public async Task WalletPushEvent([ServiceBusTrigger(Constants.Application.MemberTierUpdateEventTopicName, Constants.Application.WalletNotificationUpdateSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<WalletNotificationCommand>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(cloudEvent);
        }
    }
}
