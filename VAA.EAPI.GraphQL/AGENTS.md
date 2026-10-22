# GraphQL Solution Agent Guide

This file supplements the repository-root `AGENTS.md` for all work under `VAA.EAPI.GraphQL/`. Follow the root guide and maintained `docs/` guidance first.

## REST 404 compatibility

The GraphQL REST router translates domain-query outcomes into HTTP responses. Whenever GraphQL domain code is added or changed, review whether the change introduces a potential not-found outcome and keep the router behavior and regression coverage aligned.

Check all affected query paths for:

- a successful resolver returning `null` because the requested resource does not exist;
- exception messages or inner-exception messages that represent a missing resource;
- `HttpRequestException` or `HttpResponseException` responses with HTTP 404 status;
- reported or thrown GraphQL errors whose `Code` or `Extensions["code"]` indicates 404;
- new symbolic not-found codes or domain wording such as no matching records, bookings, passengers, offers, or results.

Do not classify validation, authorization, or technical failures as 404 merely because their wording contains words such as “not,” “unable,” or “couldn’t.” Explicit non-404 GraphQL HTTP codes such as 400, 422, and 500 remain authoritative unless the domain contract itself is deliberately changed.

When a genuine 404 candidate is introduced or changed:

1. Prefer expressing the intended status explicitly in the domain GraphQL error contract.
2. Update `VAA.EAPI.GraphQL.Rest/FunctionHost/Router.cs` only when a new reusable code, message, or exception shape must be recognized.
3. Add or update the owning domain test to assert its not-found contract.
4. Add or update `VAA.EAPI.GraphQL.Rest.Tests/RouterTests.cs` to assert the resulting HTTP 404 response.
5. Include a negative regression test when message matching could overlap a 400, 422, or 500 failure.

Keep reusable not-found message wording in the router's single combined regular-expression pattern. Match it through the shared `VAA.EAPI.GraphQL.Common.Helper.RegexCompiled` helper with case-insensitive, culture-invariant options; do not introduce a parallel string-indicator collection, multiple regex passes, direct static `Regex` calls, or a locally constructed `Regex`. Group closely related wording in the pattern, escape punctuation deliberately, and account for realistic typography such as straight and curly apostrophes. Use word boundaries or equally explicit constraints so partial words do not create false 404 responses.

Whenever that pattern changes, add positive coverage for every newly supported wording or punctuation variant and retain negative coverage for nearby non-not-found wording. Explicit GraphQL HTTP codes remain authoritative over message matching, so tests must confirm that supported 400-599 codes are preserved even when their messages resemble a not-found indicator.

Use project-scoped test runs with TRX analysis as described by the repository-root `AGENTS.md`. Before completion, run `scripts/verify.ps1 -ProjectPath <changed-project.csproj> -SkipBuild` for each changed scope, or use the smallest owning solution when appropriate.
