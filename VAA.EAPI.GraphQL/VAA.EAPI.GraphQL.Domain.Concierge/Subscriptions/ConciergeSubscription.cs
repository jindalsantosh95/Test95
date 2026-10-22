using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Concierge.Models;
using VAA.EAPI.GraphQL.Domain.Concierge.Services;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Subscriptions
{
    [ExtendObjectType(OperationTypeNames.Subscription)]
    public sealed class ConciergeSubscription
    {
        public ValueTask<ISourceStream<ConciergeMessage>> SubscribeToMessagesAsync(
            Guid conversationId,
            [Service] ITopicEventReceiver receiver,
            CancellationToken cancellationToken)
        {
            if (conversationId == Guid.Empty)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Use a non-empty conversation ID.")
                    .SetCode("400")
                    .Build());
            }

            return receiver.SubscribeAsync<string, ConciergeMessage>(ConciergeMessageService.Topic(conversationId), cancellationToken);
        }

        [Subscribe(With = nameof(SubscribeToMessagesAsync))]
        public ConciergeMessage ConciergeMessageReceived(Guid conversationId, [EventMessage] ConciergeMessage message) => message;
    }
}
