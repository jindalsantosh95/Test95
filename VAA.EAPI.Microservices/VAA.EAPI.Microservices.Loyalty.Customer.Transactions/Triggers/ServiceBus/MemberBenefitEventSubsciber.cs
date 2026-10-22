using Microsoft.Azure.Functions.Worker;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class MemberBenefitEventSubsciber
    {
        private readonly IMemberCustomAttributesCreateEventHandler _memberCustomAttributesCreateEvent;
        private readonly IMemberCustomAttributesPatchEventHandler _memberCustomAttributesPatchEvent;
        
        public MemberBenefitEventSubsciber(IMemberCustomAttributesCreateEventHandler memberCustomAttributesCreateEvent, IMemberCustomAttributesPatchEventHandler memberCustomAttributesPatchEvent)
        {
            _memberCustomAttributesCreateEvent = memberCustomAttributesCreateEvent ?? throw new System.ArgumentNullException(nameof(memberCustomAttributesCreateEvent));
            _memberCustomAttributesPatchEvent = memberCustomAttributesPatchEvent ?? throw new System.ArgumentNullException(nameof(memberCustomAttributesCreateEvent));
        }

        [Function("CreateMemberCustomAttributesEventSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.TransactionEventTopicName, Constants.Application.CreateMemberCustomAttributesEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            await _memberCustomAttributesCreateEvent.Handle(Json.Deserialize<MemberCustomAttributesCreateEvent>(eventRequestPayload), cancellationToken);
        }

        [Function("PatchMemberCustomAttributesEventSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.TransactionEventTopicName, Constants.Application.UpdateMemberCustomAttributesEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            await _memberCustomAttributesPatchEvent.Handle(Json.Deserialize<MemberCustomAttributesPatchEvent>(eventRequestPayload), cancellationToken);
        }
    }
}
