using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Banner;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class BannerEventSubscriber
    {
        private readonly IMediator _mediator;

        public BannerEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("CreateBannerEventSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.CampaignEnrolmentDocEventTopicName, Constants.Application.CreateBannerEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<BannerCreateEvent>(eventRequestPayload));
        }

        [Function("UpdateBannerEventSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.CampaignEnrolmentDocEventTopicName, Constants.Application.UpdateBannerEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<BannerUpdateEvent>(eventRequestPayload));
        }
    }
}
