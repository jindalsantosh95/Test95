# Linux Actions migration

- Status: in progress
- Owner: Codex
- Created: 2026-10-01
- Scope: GitHub Actions, Automation and backend resource portability, CI validation

## Objective

Move the 20 hosted Windows job definitions to Linux where validation supports it;
retain any job whose execution dependencies cannot be established safely.

## Acceptance criteria

- Migrate the eight metadata and routing jobs.
- Fix shell, test discovery, Search resource paths and validate CRLF checkout.
- Validate the eight conditional candidates on Linux before migrating them.
- Preserve self-hosted Windows jobs and all existing gates.
- Publish a draft PR with exact Linux evidence and remaining limitations.

## Decisions

- Existing Automation `.gitattributes` already pins C# to CRLF; retain that contract.
- Linux validation runs offline build/tests and harness exercises with read-only
  repository permissions. It does not dispatch live booking or agent mutation jobs.
- The configured delivery, review and human-comment commands resolve to the installed
  Linux Codex CLI. Shared verification and recovery exercises pass on Linux; migrate
  the remaining four delivery jobs without running live mutation agents for validation.
- Linux build exposed an OCI linked-resource case mismatch. Correct the source include
  to `checkInGraphQLQuery.txt`, preserving the existing output filename.

## Validation

- Local Automation harness: 10 architecture and 76 Core tests passed; TRX analysis clean.
- Local Search project scoped repository verifier: passed.
- Linux run 36857645002: agent command probe and scheduled harness exercises passed.
- Linux repository parity run 36857650392: all scopes and final gate passed.
- Linux docs guard run 36857650095: passed.
- Initial Linux build failed on OCI linked-resource casing; fixed. Automation build
  run 36858322378 and Automation harness run 36858322057 subsequently passed.
- OCI targeted local test: one existing NotReady scenario skipped; no live journey run.
- Linux backend validation covers Infrastructure, CRM Content, Retail Offer and Check-in,
  not the entire non-Automation unit-test estate.
- PR review identified remaining MyAccount and Retail Offer resource paths; converted
  these and shared backend test-data helpers to forward slashes with exact casing.
- The initial CRM Content probe exposed an existing test-orderer reflection failure
  on both Windows and Linux. Use xUnit's method metadata instead of resolving an
  unqualified type name across assemblies; rerun the existing CRM tests.
- Automation harness run 36858322057 passed on Linux (10 architecture + 76 Core tests).
- One duplicate diagnostic runner exhausted disk while fetching full history; use a
  depth-one comparison ref in diagnostic jobs.
- Local mandatory MyAccount checks initially compared against the stale local `main`;
  rerun against `origin/main`, the branch base, to exclude unrelated upstream changes.
- Local MyAccount required gates passed against `origin/main`; scoped Retail Offer
  verification also passed. CRM Content passes all 15 tests after the orderer repair;
  Retail Offer passes 110 tests. Both runs produced TRX and Cobertura evidence.
- Linux Retail Offer execution exposed nested Windows-style test-data keys and one
  fixture case mismatch. Normalize keys at the shared test-data readers, fix direct
  XML fixture paths, and make shared error/warning resource constants portable.
- The next Linux run passed 109/110 Retail Offer tests; the remaining failure used
  a legacy `GlobalException\ErrorConfig.json` argument. Normalize separators in the
  common file reader and exercise both path styles and missing-file behavior in the
  existing Infrastructure unit suite.
- Linux run 36860481805 passes all three initial backend suites after the common
  reader repair. Complete the adjacent Check-in resource constants and validate its
  111-test unit suite as the fourth backend scope (local run passed).
- Linux run 36860931776 at `61989e8f5c` passed all seven jobs: 337 tests passed,
  one existing skip, and no failures. This includes the Check-in suite.
- 2026-10-02, Codex: review follow-up confirmed the two open resource-path findings
  are already fixed. Correct the verifier's stale native exit-code check by catching
  analyser exceptions, and distinguish retained TRX files from console summaries.
  Full local Automation verification passed (86 tests, clean TRX analysis). A focused
  probe confirmed stale native failures are ignored by analysis and analyser invocation
  errors are reported. All nine review threads have explanatory replies and are resolved.
  Solution-scoped repository verification also passed against `origin/main`.
  Next action: push the review follow-up; EAPI maintainers review the draft for merge.
