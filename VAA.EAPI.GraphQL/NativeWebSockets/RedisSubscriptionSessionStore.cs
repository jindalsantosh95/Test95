#if HOTCHOCOLATE_13
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;
#else
using VAA.EAPI.GraphQL.Common.Extensions;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.FunctionHost.WebSockets
{
    /// <summary>Short-lived, caller-bound result mailboxes shared by all workers of one app.</summary>
    public sealed class RedisSubscriptionSessionStore : IDisposable
    {
        private readonly string prefix;
        private readonly Lazy<ConnectionMultiplexer> connection;
        public IConnectionMultiplexer Connection => connection.Value;

        public RedisSubscriptionSessionStore(string redisConfiguration, string prefix)
        {
            if (redisConfiguration.IsNullOrWhiteSpace() ||
                redisConfiguration.TrimStart().StartsWith("@Microsoft.KeyVault(", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Redis subscriptions require a resolved RedisCacheConnectionString.");
            }
            if (prefix is null || !Regex.IsMatch(prefix, @"\A[a-zA-Z0-9_-]{3,100}\z"))
            {
                throw new InvalidOperationException("NativeWebSocketsRedisPrefix must identify the app and environment using 3-100 letters, digits, underscores or hyphens.");
            }
            this.prefix = prefix;
            connection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var options = ConfigurationOptions.Parse(redisConfiguration);
                options.ChannelPrefix = RedisChannel.Literal(prefix + ":events:");
                // Keep a reconnecting multiplexer when Redis is unavailable during first use.
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            });
        }

        private RedisKey[] Keys(Guid id) => new RedisKey[]
        {
            prefix + ":session:{" + id.ToString("N") + "}:meta",
            prefix + ":session:{" + id.ToString("N") + "}:results"
        };

        private static string Identity(string owner, Guid connectionId) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(connectionId.ToString("N") + ":" + owner)));

        public async Task CreateAsync(Guid id, string owner, Guid connectionId)
        {
            var result = await Connection.GetDatabase().ScriptEvaluateAsync(@"
                if redis.call('EXISTS', KEYS[1]) == 1 then return 0 end
                local t = redis.call('TIME')
                redis.call('HSET', KEYS[1], 'owner', ARGV[1], 'state', 'running', 'heartbeat', t[1], 'bytes', 0)
                redis.call('EXPIRE', KEYS[1], 330)
                return 1", Keys(id), new RedisValue[] { Identity(owner, connectionId) });
            if ((int)result != 1) { throw new InvalidOperationException("Subscription identity collision."); }
        }

        // Heartbeats and writes use the same ownership check as reads/stops. Expiry is never extended.
        public async Task<bool> PublishAsync(Guid id, string owner, Guid connectionId, JObject result, bool complete = false)
        {
            var json = result?.ToString(Formatting.None) ?? "";
            var overflow = Error("Subscription buffer exceeded; reconnect and subscribe again.").ToString(Formatting.None);
            var reply = await Connection.GetDatabase().ScriptEvaluateAsync(@"
                if redis.call('HGET', KEYS[1], 'owner') ~= ARGV[1] or
                   redis.call('HGET', KEYS[1], 'state') ~= 'running' then return 0 end
                local t = redis.call('TIME')
                redis.call('HSET', KEYS[1], 'heartbeat', t[1])
                if #ARGV[2] > 0 then
                    local size = tonumber(redis.call('HGET', KEYS[1], 'bytes')) + #ARGV[2]
                    if size > 1048576 or redis.call('LLEN', KEYS[2]) >= 128 then
                        redis.call('DEL', KEYS[2])
                        redis.call('RPUSH', KEYS[2], ARGV[4])
                        redis.call('HSET', KEYS[1], 'state', 'finished', 'bytes', #ARGV[4])
                        redis.call('PEXPIRE', KEYS[2], redis.call('PTTL', KEYS[1]))
                        return 0
                    end
                    redis.call('RPUSH', KEYS[2], ARGV[2])
                    redis.call('HSET', KEYS[1], 'bytes', size)
                    redis.call('PEXPIRE', KEYS[2], redis.call('PTTL', KEYS[1]))
                end
                if ARGV[3] == '1' then redis.call('HSET', KEYS[1], 'state', 'finished') end
                return 1", Keys(id), new RedisValue[] { Identity(owner, connectionId), json, complete ? "1" : "0", overflow });
            return (int)reply == 1;
        }

        public async Task<JObject> ReadAsync(Guid id, string owner, Guid connectionId, CancellationToken cancellationToken)
        {
            var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                var reply = (RedisResult[])await Connection.GetDatabase().ScriptEvaluateAsync(@"
                    if redis.call('HGET', KEYS[1], 'owner') ~= ARGV[1] then return {-1} end
                    local t = redis.call('TIME')
                    if redis.call('HGET', KEYS[1], 'state') == 'running' and
                       tonumber(t[1]) - tonumber(redis.call('HGET', KEYS[1], 'heartbeat')) > 45 then
                        redis.call('DEL', KEYS[1], KEYS[2])
                        return {-2}
                    end
                    local value = redis.call('LPOP', KEYS[2])
                    if value then
                        redis.call('HINCRBY', KEYS[1], 'bytes', -#value)
                        return {1, value}
                    end
                    if redis.call('HGET', KEYS[1], 'state') == 'finished' then return {2} end
                    return {0}", Keys(id), new RedisValue[] { Identity(owner, connectionId) });
                switch ((int)reply[0])
                {
                    case -1: throw new UnauthorizedAccessException("Subscription unavailable for this caller.");
                    case -2: return Error("Subscription worker unavailable; reconnect and subscribe again.");
                    case 1: return JObject.Parse((string)reply[1]);
                    case 2: return new JObject { ["complete"] = true };
                }
                await Task.Delay(200, cancellationToken);
            } while (DateTimeOffset.UtcNow < deadline);
            return new JObject();
        }

        public async Task StopAsync(Guid id, string owner, Guid connectionId)
        {
            var reply = await Connection.GetDatabase().ScriptEvaluateAsync(@"
                if redis.call('HGET', KEYS[1], 'owner') ~= ARGV[1] then return 0 end
                redis.call('DEL', KEYS[1], KEYS[2])
                return 1", Keys(id), new RedisValue[] { Identity(owner, connectionId) });
            if ((int)reply != 1) { throw new UnauthorizedAccessException("Subscription unavailable for this caller."); }
        }

        public static JObject Error(string message) => new()
        {
            ["result"] = new JObject { ["errors"] = new JArray(new JObject { ["message"] = message }) }
        };

        public void Dispose()
        {
            if (connection.IsValueCreated) { connection.Value.Dispose(); }
        }
    }
}
