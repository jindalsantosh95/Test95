using Microsoft.AspNetCore.Http.Features;

namespace VAA.EAPI.GraphQL.WebSocketHost
{
    internal sealed class BridgeRequestBodyFeature : IHttpRequestBodyDetectionFeature
    {
        public bool CanHaveBody => true;
    }
}
