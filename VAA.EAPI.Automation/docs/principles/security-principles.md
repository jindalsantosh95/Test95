# Security Principles
**VAA.EAPI.Automation**

---

These **extend** the platform [Security Principles](../../../doc/principles/security-principles.md)
and [EAPI Coding Standards](../../../doc/standards/coding-standards.md) for the test suite.
The platform baseline — secrets resolved from Key Vault, secure-by-default and
fail-closed, defence in depth, PII classification, encryption at rest, least-privilege and
auditable key access — **holds here and is not restated**. This document states only how
those rules apply when *writing and running tests*: a suite handles credentials, hits real
environments, and processes booking and loyalty data, so a leaked secret or real customer
record committed here is a real incident.

## Secrets Never In Source (test-suite application)

The platform rule "secrets are resolved from secure runtime configuration, never committed"
applies to test config specifically.

- Test-specific credentials — environment subscription keys, BrowserStack username/access
  key, storage keys, OAuth client secrets, partner keys — live in `local.settings.json`
  (gitignored), pipeline variable groups, or Key Vault. Never in code.
- Do not put secrets in `appsettings*.json`, test payloads, page objects, fixtures, or
  README/markdown docs. This is in the definition of done:
  *do not hard-code environment secrets in code or markdown*.
- Do not concatenate tokens into URLs or log lines; pass bearer tokens through the request
  wrapper's token parameter / request-specific headers (see [Integration Principles](integration-principles.md)).
- An accidental secret commit is an incident — follow the platform rotation/removal process;
  do not just delete it in a later commit.

## Synthetic Data Only

The suite must never carry real customer data.

- Passenger, loyalty, identity, passport, contact, payment, and refund data are sensitive.
  Use only synthetic records sourced from the team's storage tables (see
  [Test Data Principles](test-data-principles.md)).
- Do not paste production PNRs, member numbers, or personal data into tests, fixtures, or
  docs, and do not build a private side-channel of real data.

## Don't Leak Sensitive Data Into Output

Logs, reports, and assertions are shared artifacts.

- Do not log full downstream request/response payloads when they may contain PII or
  secrets; assert on the specific field instead.
- Be deliberate about what ExtentReports screenshots and BrowserStack sessions capture —
  a screenshot of a logged-in screen or an OTP is sensitive; avoid capturing credentials
  or personal data in checkpoints.
- Keep secrets and PII out of test names, trait values, cache keys, and failure messages.

## Least Privilege And Environment Hygiene

- Run against the lowest environment that proves the behaviour; reserve prod-targeted runs
  for what genuinely needs them, and handle the prod host differences explicitly (see
  base-URL guidance in [Test Data Principles](test-data-principles.md)).
- Use test accounts and identities scoped to automation; do not reuse privileged or
  personal accounts.
- Validate environment access through the documented setup
  ([`LOCAL_SETUP.md`](../LOCAL_SETUP.md)) rather than embedding bypasses.

## Dependencies

- Add packages only through Central Package Management; a single pinned version per package
  keeps the supply chain auditable (see [Execution & Infrastructure](execution-infrastructure-principles.md)).
- When a package is bumped for a security advisory, keep the whole related version band
  aligned, as the harness work did for the `Microsoft.Extensions.*` / `System.*` runtime
  band — partial bumps reintroduce the vulnerable transitive.

> Reference: [`AGENTS.md`](../../AGENTS.md), [`LOCAL_SETUP.md`](../LOCAL_SETUP.md),
> [`TEST_DATA_STRATEGY.md`](../TEST_DATA_STRATEGY.md),
> [`HARNESS_ENGINEERING.md`](../HARNESS_ENGINEERING.md).
