using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public sealed class AccountLinkingRedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer _connection;

        public AccountLinkingRedisHealthCheck(IConnectionMultiplexer connection)
        {
            _connection = connection ?? throw new System.ArgumentNullException(nameof(connection));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _connection.GetDatabase().PingAsync();
                const string probe = "return 1";
                var writableServers = 0;
                foreach (var endpoint in _connection.GetEndPoints())
                {
                    var server = _connection.GetServer(endpoint);
                    if (!server.IsConnected || server.IsReplica) continue;
                    cancellationToken.ThrowIfCancellationRequested();
                    var hash = await server.ScriptLoadAsync(probe);
                    await server.ExecuteAsync("EVALSHA", System.Convert.ToHexString(hash).ToLowerInvariant(), 0);
                    await server.ExecuteAsync("EVAL", probe, 0);
                    writableServers++;
                }
                if (writableServers == 0)
                    return HealthCheckResult.Unhealthy("No writable account-linking Redis server is available.");
                return HealthCheckResult.Healthy("Account-linking Redis connectivity and scripting are available.");
            }
            catch (System.Exception exception)
            {
                return HealthCheckResult.Unhealthy("Account-linking Redis continuity check failed.", exception);
            }
        }
    }
}
