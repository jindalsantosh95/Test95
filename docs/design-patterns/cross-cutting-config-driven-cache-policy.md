# Config-Driven Cache Policy

**Pattern group:** Cross-Cutting

**Status:** Recommended

**Last reviewed:** 2026-10-01

**Owner:** EAPI Technical Leads

## Intent

Control EAPI caching behavior through operation-specific configuration so cache eligibility, cache keys, expiry, and response caching can be changed without embedding cache rules in resolver or handler code.

This pattern keeps caching as a performance optimisation that fails open and does not become required for correctness.

## Use When

- Caching GraphQL or HTTP responses based on operation name, headers, query text, or variables.
- A response is safe to cache and does not contain user-specific or sensitive data beyond the key.
- Cache behavior needs environment-specific control.

## Do Not Use When

- The response contains sensitive or personalised data that cannot be safely keyed.
- Correctness depends on cache availability.
- The data owner requires fresh reads for every request.

## Pattern Shape

1. Configure `Cache:<operation>` policy with expiry and optional conditions.
2. Middleware checks whether the operation is cacheable.
3. Middleware builds a key from operation, headers, query, variables, or configured tokens.
4. Cache hit returns the compressed cached response.
5. Cache miss executes normally and caches only eligible successful responses.

## Implementation Guidance

- Treat cache failures as non-fatal and continue normal execution.
- Do not cache responses containing errors.
- Include all relevant user, channel, or request discriminators in the key.
- Review cache policies for PII and data leakage risk.
- The independent [shared HTTP security PR #2308](https://github.com/VAAD1ST/eapi/pull/2308) proposes response-only cache entries: request URIs, headers and bodies must not be serialized into entries, and cache hits should attach the current request. This hardening is not delivered by address PR #2071; legacy implementations still store request metadata until the platform change is adopted.
- That platform change also proposes hashing configured HTTP cache discriminators with SHA-256 under the `http-response:v2:` prefix. This is not encryption or a substitute for safe cache policies: response data still needs an appropriate sensitivity and retention review. Address PR #2071 retains the existing key format.
- Moving from reversible legacy HTTP keys creates a cold cache. Roll out with downstream capacity in mind. Old entries are not migrated or read; operators must expire or purge credential-bearing legacy entries under their approved process (including retained backups), and assess credential rotation where needed. Old application instances continue writing the legacy format until upgraded.

## Code References

- [CachingMiddleware.cs](../../VAA.EAPI.GraphQL/VAA.EAPI.GraphQL/Middleware/CachingMiddleware.cs:23): middleware read-through/write-through behavior.
- [ConfigReader.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/Helpers/ConfigReader.cs:156): cache eligibility from configuration.
- [ConfigReader.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/Helpers/ConfigReader.cs:337): cache key construction.

## Related Principles And Standards

- [Data Principles](../principles/data-principles.md)
- [Security Principles](../principles/security-principles.md)
- [Infrastructure Principles](../principles/infrastructure-principles.md)

## AI Agent Checklist

- Response is safe to cache.
- Cache key contains all required discriminators.
- Error responses are not cached.
- Cache failure does not break the core workflow.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-10-01 | Document response-only HTTP cache entries, hashed keys and legacy-entry rollout considerations. | EAPI maintainers |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
