# Distributed cache modes

Existing `IDistributedCacheService` calls and its two-argument constructor are unchanged:
`Get` may refresh expiration; `Set`, `Refresh` and `Remove` retain their existing error suppression and circuit breaker.

`DistributedCacheService` additionally implements `ISingleUseDistributedCacheService`.
Register this interface only for opt-in callers. A supplied `IConnectionMultiplexer` enables atomic operations;
it is owned by DI, not disposed by the cache service. There is no global switch or new environment setting.

- `SetAsync(..., mode: CacheMode.Default)` delegates to existing absolute-expiry Set. It returns null, because legacy writes suppress failures and cannot promise a persisted expiry.
- `SetAsync(..., mode: CacheMode.SingleUse, options: ...)` atomically stores a compressed payload and returns its persisted UTC expiry, or null for a replay/policy conflict. Dependency and configuration failures throw.
- Options contain a logical scope key, expected binding, owner identity, unique issuance ID, absolute scope lifetime and maximum replacements. The calling domain supplies these; the cache knows nothing about account linking.
- `TryConsumeAsync<T>(key, scopeKey, binding)` atomically checks the binding/current item, marks the scope consumed and removes the item. Only one caller receives a payload; mismatch, expiry and repeat consumption return null. It never refreshes expiry.
- A scope can issue one initial item and the allowed number of replacements; the first payload and expiry window are retained. Every replacement requires a new issuance ID and invalidates the old item. After consumption, the scope cannot be reused until its window expires. Start a new scope for a new workflow.
- Single-use records use their own hashed namespace and Redis cluster hash tag. Normal Get/Refresh/Remove cannot access them. Lua metadata remains JSON; payloads use existing GZip helpers/base64. Compression is not encryption.
- Cancellation is checked before dispatch. An interrupted or lost Redis response can leave an unknown outcome; callers must not interpret it as a successful write or consumption.

Do not emulate atomic consumption with separate reads/deletes or an in-process lock.
Use the existing Core Xunit cache mock for default-mode tests; real Redis is necessary for concurrent single-use tests.
