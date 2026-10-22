using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Infrastructure.Caching
{
    public sealed partial class DistributedCacheService
    {
        private readonly IConnectionMultiplexer _atomicConnection;

        public DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger,
            IConnectionMultiplexer atomicConnection) : this(cache, logger)
        {
            _atomicConnection = atomicConnection ?? throw new ArgumentNullException(nameof(atomicConnection));
        }

        private const string StoreSingleUseScript = @"
local ttl = tonumber(ARGV[5])
local window = tonumber(ARGV[7])
if redis.call('EXISTS', KEYS[4]) == 1 then return false end
local payload = ARGV[4]
local previous = redis.call('GET', KEYS[1])
if previous then
  local state = cjson.decode(previous)
  if state.Status ~= 'created' or state.Binding ~= ARGV[1] or state.Owner ~= ARGV[2] then return false end
  window = redis.call('TTL', KEYS[1])
  if window <= 0 or state.Replacements >= tonumber(ARGV[6]) then return false end
  state.Replacements = state.Replacements + 1
  payload = state.Payload
  redis.call('SET', KEYS[1], cjson.encode(state), 'EX', window)
else
  redis.call('SET', KEYS[1], cjson.encode({Status='created',Binding=ARGV[1],Owner=ARGV[2],Payload=payload,Replacements=0}), 'EX', window)
end
ttl = math.min(ttl, window)
redis.call('SET', KEYS[4], '1', 'EX', window)
local old = redis.call('GET', KEYS[3])
if old then redis.call('DEL', KEYS[5] .. old) end
redis.call('SET', KEYS[2], cjson.encode({Binding=ARGV[1],Payload=payload}), 'EX', ttl)
redis.call('SET', KEYS[3], ARGV[3], 'EX', window)
return ttl";

        private const string ConsumeSingleUseScript = @"
local rawState = redis.call('GET', KEYS[1])
if not rawState then return false end
local state = cjson.decode(rawState)
if state.Status ~= 'created' or state.Binding ~= ARGV[1] then return false end
if redis.call('GET', KEYS[3]) ~= ARGV[2] then return false end
local rawItem = redis.call('GET', KEYS[2])
if not rawItem then return false end
local item = cjson.decode(rawItem)
if item.Binding ~= ARGV[1] then return false end
local ttl = redis.call('TTL', KEYS[1])
if ttl <= 0 then return false end
state.Status = 'consumed'
redis.call('SET', KEYS[1], cjson.encode(state), 'EX', ttl)
redis.call('DEL', KEYS[2], KEYS[3])
return item.Payload";

        public async Task<DateTime?> SetAsync<T>(string key, T value, TimeSpan lifetime,
            CacheMode mode = CacheMode.Default, SingleUseCacheOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (mode == CacheMode.Default)
            {
                await Set(key, value, false, TimeSpan.Zero, lifetime);
                // Legacy writes suppress failures; do not claim a persisted expiry.
                return null;
            }
            if (mode != CacheMode.SingleUse) throw new ArgumentOutOfRangeException(nameof(mode));
            ArgumentNullException.ThrowIfNull(options);
            Require(key, nameof(key));
            Require(options.ScopeKey, nameof(options.ScopeKey));
            Require(options.Binding, nameof(options.Binding));
            Require(options.Owner, nameof(options.Owner));
            Require(options.IssuanceId, nameof(options.IssuanceId));
            if (lifetime <= TimeSpan.Zero || options.ScopeLifetime <= TimeSpan.Zero || options.MaximumReplacements < 0)
                throw new ArgumentOutOfRangeException(nameof(options));
            var database = AtomicDatabase();
            var prefix = Prefix(options.ScopeKey);
            var item = Hash(key);
            var payload = Convert.ToBase64String(JsonConvert.SerializeObject(value).Compress());
            var startedAt = DateTime.UtcNow;
            var result = await database.ScriptEvaluateAsync(StoreSingleUseScript,
                new RedisKey[] { prefix + "state", prefix + "item:" + item, prefix + "current",
                    prefix + "issuance:" + Hash(options.IssuanceId), prefix + "item:" },
                new RedisValue[] { Hash(options.Binding), Hash(options.Owner), item,
                    payload, Math.Max(1, (long)Math.Floor(lifetime.TotalSeconds)), options.MaximumReplacements,
                    Math.Max(1, (long)Math.Floor(options.ScopeLifetime.TotalSeconds)) });
            return result.IsNull ? null : startedAt.AddSeconds((long)result);
        }

        public async Task<T> TryConsumeAsync<T>(string key, string scopeKey, string binding,
            CancellationToken cancellationToken = default) where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            Require(key, nameof(key));
            Require(scopeKey, nameof(scopeKey));
            Require(binding, nameof(binding));
            var database = AtomicDatabase();
            var prefix = Prefix(scopeKey);
            var item = Hash(key);
            var result = await database.ScriptEvaluateAsync(ConsumeSingleUseScript,
                new RedisKey[] { prefix + "state", prefix + "item:" + item, prefix + "current" },
                new RedisValue[] { Hash(binding), item });
            return result.IsNull ? null : ((string)result).DecompressTo<T>();
        }

        private IDatabase AtomicDatabase() => _atomicConnection?.GetDatabase()
            ?? throw new InvalidOperationException("Single-use caching requires an atomic Redis backend.");

        private static void Require(string value, string name)
        {
            if (value.IsNullOrWhiteSpace()) throw new ArgumentException("A value is required.", name);
        }

        private static string Prefix(string scope) => $"single-use:{{{Hash(scope)}}}:";
        private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
