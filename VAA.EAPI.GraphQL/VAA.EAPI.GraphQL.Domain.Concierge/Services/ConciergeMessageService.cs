using HotChocolate;
using HotChocolate.Subscriptions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Concierge.Models;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Services
{
    public sealed class ConciergeMessageService
    {
        private readonly ITopicEventSender sender;

        public ConciergeMessageService(ITopicEventSender sender)
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public static string Topic(Guid conversationId)
        {
            var principal = AsyncContext.Current?.Context?.GetJwtPrincipalFeature()?.Principal;
            var identity = principal?.Identities.FirstOrDefault(x => x.IsAuthenticated);
            var issuer = identity?.FindFirst("iss")?.Value;
            var subject = identity?.FindFirst("sub")?.Value ?? identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (issuer is not null && subject is not null)
            {
                var owner = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                    Common.Serialisation.Json.Serialize(new[] { issuer, subject }))));
                return $"concierge:{owner}:{conversationId:D}";
            }
            return $"concierge:{conversationId:D}";
        }

        public ConciergeMessage CreateReply(Guid conversationId, string message)
        {
            if (conversationId == Guid.Empty || message.IsNullOrWhiteSpace() || message.Length > 500)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Use a non-empty conversation ID and a message of 1 to 500 characters.")
                    .SetCode("400")
                    .Build());
            }

            var response = message.Trim().ToLowerInvariant() switch
            {
                "ping" => "pong",
                "pong" => "ping",
                _ => $"Concierge received: {message.Trim()}"
            };

            return new ConciergeMessage(Guid.NewGuid(), conversationId, message, response, DateTimeOffset.UtcNow);
        }

        public async Task<ConciergeMessage> PostAsync(Guid conversationId, string message, CancellationToken cancellationToken)
        {
            var reply = CreateReply(conversationId, message);
            await sender.SendAsync(Topic(conversationId), reply, cancellationToken);
            return reply;
        }
    }
}
