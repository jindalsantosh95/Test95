# Test Data Principles
**VAA.EAPI.Automation**

---

Test data is the most common source of flake and false confidence in this suite. Bookings
expire, PNRs get consumed, and hard-coded values rot silently. AI agents must source data
from the shared, refreshed stores, keep it synthetic and safe, and surface missing data as
a failure rather than a null-deref. The canonical patterns are in
[`TEST_DATA_STRATEGY.md`](../TEST_DATA_STRATEGY.md); this document states the rules.

## Storage-Table-Driven By Default

Booking-shaped data is sourced from the shared Azure storage tables, never hard-coded.

- Every new test that needs a PNR, loyalty account, holiday booking, passport, or other
  booking-shaped record uses `VAA.EAPI.Automation.Core.Helpers.StorageAccount.RetrieveFromStorageTable`
  as the single entry point.
- Do not hard-code PNRs, names, or DOBs in test classes — they break the moment the backing
  booking is cancelled, expires, or is consumed by another suite.
- Use the helper that matches the scenario (current-week, specific-route, seat-scenarios,
  OCI, loyalty tier, holidays, passport) — see the table in
  [`TEST_DATA_STRATEGY.md`](../TEST_DATA_STRATEGY.md#default-storage-table-driven-data).
- This is part of the definition of done: data a test consumes must come from
  `RetrieveFromStorageTable` or match a documented exception.

## Handle Missing Data Explicitly

A lookup that finds nothing is a test failure, not a crash.

- The helpers return a `Dictionary<string, string>`; when no row matches, it contains a
  single `"error"` entry. Check for it and `Assert.Fail($"…: {error}")`.
- Do not index straight into the dictionary and let a missing key null-deref downstream —
  the failure message must say *what data was unavailable*.

## When Hard-Coded Data Is Legitimate

Inline literals are reserved for cases where dynamic data adds nothing.

- Negative-path inputs (malformed PNR, invalid tier), schema/contract tests, and pure unit
  tests of `Core` helpers may use inline values.
- The full exception list is in
  [`TEST_DATA_STRATEGY.md` § When you genuinely need hard-coded data](../TEST_DATA_STRATEGY.md#when-you-genuinely-need-hard-coded-data).
  If a case is not on that list, source the data from a table.

## Synthetic And Safe

Test data must never be real customer data.

- Passenger details, loyalty/identity details, booking references, passport data, contact
  and payment details are sensitive. Use only synthetic records that are safe to share.
- Do not paste production PNRs, member numbers, or personal data into tests, fixtures,
  payload files, or docs.
- The storage tables are fed by the team's PNR-creation pipeline with synthetic bookings —
  that is the supported source. Do not introduce a private side-channel of real data.
- See [Security Principles](security-principles.md) for PII handling in logs and reports.

## Ownership And Lifecycle

- Data setup that repeats belongs in a shared helper or data provider, not copied across
  tests (see [Architecture Principles](architecture-principles.md) on placement).
- A test that creates state owns cleaning it up; do not leave records that another suite
  could collide with.
- Cache or reuse fetched data within a test, but do not turn a static field into an
  untracked shared data store across tests.

## Choosing The Right Host

The base-URL config classes look interchangeable and are not.

- Default to `Config.TestResourcesVirginAtlantic` (the customer-facing host, ~367 call
  sites). Only use `Config.DigitalTestVirginAtlantic` for internal `/digital/vred/*` or
  legacy raw `/digital/graphql` paths, and add a `_env.Is("prod")` fallback because the
  digital-test host has no prod equivalent.
- The `digital/...` segment of a URL like
  `testresources.virginatlantic.com/.../digital/Graphql/rest/v1/...` is *path, not host* —
  do not be misled by the word "Digital" in the config-class name.
- Full decision table: [`TEST_DATA_STRATEGY.md` § Base URLs](../TEST_DATA_STRATEGY.md#base-urls).

> Reference: [`TEST_DATA_STRATEGY.md`](../TEST_DATA_STRATEGY.md),
> [`AGENTS.md` § Preferred workflow](../../AGENTS.md#preferred-workflow).
