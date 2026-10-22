namespace VAA.EAPI.GraphQL.Domain.Concierge.Models
{
    public sealed record ConciergeMessage(
        Guid MessageId,
        Guid ConversationId,
        string Message,
        string Response,
        DateTimeOffset CreatedAtUtc);
}
