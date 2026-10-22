# Stream ITA Offer Solutions

- Status: completed
- Owner: Codex
- Created: 2026-08-13
- Scope: Retail Offer microservice, unit tests, benchmark tests, and deployment settings

## Objective

Process ITA `bundledSolutionSummary/solution` elements individually so the generated provider solution object is mapped to the EAPI offer model and released before the next solution is read.

## Context

An ITA response failed with `System.OutOfMemoryException` while `XmlSerializer` constructed the complete generated `result` graph. Passing a forward-only reader to the root serializer does not reduce that graph.

## Acceptance Criteria

- Offer search uses an async, forward-only `XmlReader` and never materialises the bundled solution array.
- Each solution is mapped immediately and discarded.
- The implementation does not impose response-byte or solution-count limits.
- Production-representative memory load is measured through the complete Offer handler at concurrent request levels.
- Existing reshop and reprice contracts remain unchanged.
- Focused tests and the repository harness pass.

## Plan

1. Add a callback-based streaming ITA adapter that parses lightweight result metadata and individual solutions.
2. Refactor the Offer mapper/handler to map one solution per callback and finalise the domain response.
3. Add unit, allocation benchmark, concurrent full-handler memory-load coverage, and scoped verification.

## Decisions

- 2026-08-13: Scope streaming to standard Offer search; other ITA flows retain their established full-result contract to avoid a cross-capability rewrite.
- 2026-08-13: Use a callback so provider solution objects cannot escape into a retained collection.
- 2026-08-13: Extract only the XML response body from the supplied mock, excluding request headers and request content.
- 2026-08-13: Remove response-byte and solution-count limits; rely on forward-only processing to control provider-model memory rather than rejecting otherwise valid responses.
- 2026-08-13: Exercise the complete Offer handler at concurrency 1, 2, 4, 8, and 16 while retaining mapped EAPI responses and sampling managed memory and process working set.
- 2026-08-13: Route all new service, handler, benchmark, and load tests through `ServiceCollection`, the real `HttpConnector`, the shared xUnit `HttpClientFactory`, and the supplied payload converted to a checked-in `.mock` resource.
- 2026-08-13: Strengthen the test-standards sensor and harness self-check so future changed HTTP-backed tests reject Moq, ad hoc `IHttpConnector` implementations/instance registrations, and missing `.mock` resources.

## Validation

- `dotnet build VAA.EAPI.Microservices.Retail.Offer.csproj -c Release --no-restore`: passed.
- Native focused test run for `ItaStreamingServiceTests` passed against a 17.38 MB, 312-solution response.
- Retail Offer test project: 106/106 passed. Microsoft Testing Platform ignored VSTest TRX/coverage options, so no TRX analysis was available.
- `scripts/verify.ps1 -ProjectPath VAA.EAPI.Microservices.Retail.Offer.csproj -SkipBuild`: passed.
- BenchmarkDotNet shared-pipeline allocation benchmark passed for 1, 4, and 8 sequential responses. One response averaged 1.044 seconds and allocated 498.92 MB; eight averaged 5.692 seconds and allocated 4,183.41 MB cumulatively. These conservative figures include `.mock` parsing/materialisation overhead and are not peak retained memory.
- Full-handler shared-pipeline concurrent memory-load suite: 5/5 passed for concurrency 1, 2, 4, 8, and 16. The concurrency-16 run peaked at 1,230.4 MB managed memory and 1,338.8 MB process working set, increases of 1,228.9 MB and 1,254.9 MB over that test's baselines. The `.mock` parser contributes to this conservative test-only envelope.
- Shared-pipeline Offer service and handler focused tests: 2/2 passed, with the expected `.mock` match confirmed in runner output.
- Test-standards exercise: 16/16 detector cases passed; changed-code test-standards checks and harness self-protection rules passed.
- `scripts/verify.ps1 -ProjectPath VAA.EAPI.Core.Benchmark.Tests.csproj -SkipBuild`: passed.

## Outcome

Standard Offer search now parses and maps each ITA solution independently and retains only EAPI domain output. The checked-in production-representative payload supports repeatable allocation and concurrent full-handler memory measurements without deliberately forcing the test process to exhaust memory.
