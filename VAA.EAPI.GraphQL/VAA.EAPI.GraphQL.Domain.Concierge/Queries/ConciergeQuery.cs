using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Concierge.Models;
using VAA.EAPI.GraphQL.Domain.Concierge.Services;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class ConciergeQuery
    {
        public ConciergeMessage ConciergePing(Guid conversationId, [Service] ConciergeMessageService service) =>
            service.CreateReply(conversationId, "ping");

        public ConciergeMessage ConciergePong(Guid conversationId, [Service] ConciergeMessageService service) =>
            service.CreateReply(conversationId, "pong");
    }
}
