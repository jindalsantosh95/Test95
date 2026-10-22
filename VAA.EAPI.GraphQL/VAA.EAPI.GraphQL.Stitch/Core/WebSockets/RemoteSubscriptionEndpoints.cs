using System.Collections.Specialized;

namespace VAA.EAPI.GraphQL.Stitch.Core.WebSockets
{
    public sealed class RemoteSubscriptionEndpoints
    {
        public const string HeadersProperty = "Stitch.SubscriptionHeaders";
        private readonly NameValueCollection httpEndpoints;

        public RemoteSubscriptionEndpoints(NameValueCollection httpEndpoints)
        {
            this.httpEndpoints = new NameValueCollection(httpEndpoints);
        }

        public Uri Get(string schema, bool serverSentEvents = false)
        {
            if (!Uri.TryCreate(httpEndpoints[schema], UriKind.Absolute, out var httpEndpoint) ||
                (httpEndpoint.Scheme != "https" && !(httpEndpoint.Scheme == "http" && httpEndpoint.IsLoopback)) ||
                !string.IsNullOrEmpty(httpEndpoint.UserInfo) || !string.IsNullOrEmpty(httpEndpoint.Fragment) ||
                !string.IsNullOrEmpty(httpEndpoint.Query))
            {
                throw new HotChocolate.GraphQLException("No safe subscription endpoint can be derived for this remote schema. Correct Graphql:Endpoints to use an HTTPS URL without credentials, query or fragment.");
            }

            return new UriBuilder(httpEndpoint)
            {
                Scheme = serverSentEvents ? httpEndpoint.Scheme : httpEndpoint.Scheme == "https" ? "wss" : "ws",
                Path = httpEndpoint.AbsolutePath.TrimEnd('/') + (serverSentEvents ? "/sse" : "/ws")
            }.Uri;
        }
    }
}
