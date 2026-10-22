using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Concierge.Models;
using VAA.EAPI.GraphQL.Domain.Concierge.Services;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public sealed class ConciergeMutation
    {
        public Task<ConciergeMessage> PostConciergeMessageAsync(
            Guid conversationId,
            string message,
            [Service] ConciergeMessageService service,
            CancellationToken cancellationToken) =>
            service.PostAsync(conversationId, message, cancellationToken);
    }
}
