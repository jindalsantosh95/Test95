namespace VAA.EAPI.GraphQL.WebSocketHost
{
    public sealed class TransportOptions
    {
        public const string Protocol = "graphql-transport-ws";
        public const string BridgeRoute = "/api/internal/graphql-websocket";
        public string Path { get; init; } = "/api/v1/GraphQL/ws";
        public string SsePath => Path.TrimEnd('/')[..Path.TrimEnd('/').LastIndexOf('/')] + "/sse";
        public string BridgeSecret { get; init; } = "";

        public static TransportOptions FromEnvironment()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NativeWebSocketsRedisPrefix")?.Trim()))
            {
                throw new InvalidOperationException("Native subscriptions require NativeWebSocketsRedisPrefix.");
            }
            var secret = Environment.GetEnvironmentVariable("NativeWebSocketsBridgeSecret") ?? "";
            if (secret.Trim().Length < 32 || secret.TrimStart().StartsWith("@Microsoft.KeyVault(", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid bridge credential (resolved value of at least 32 characters required).");
            }
            return new TransportOptions
            {
                BridgeSecret = secret,
                Path = Environment.GetEnvironmentVariable("NativeWebSocketsPath") ?? "/api/v1/GraphQL/ws"
            };
        }
    }
}
