# Dynamic component coverage in release reports

## AI assessment deferred — restricted execution required

The external-model coverage assessment is deferred from this PR. There is no mobile
AI assessment workflow, schedule, manual entry point, model credential mapping or assessment
artifact download. The existing Book self-hosted runner remains for functional tests only;
it must not be reused for AI processing simply because it is self-hosted.

Before reintroducing this capability in a separate reviewed PR:

- Provision a dedicated restricted runner or equivalent isolated environment.
- Enforce deny-by-default outbound networking with only approved provider destinations;
  proxy configuration alone is insufficient if direct connections can bypass it.
- Validate allowed and blocked destinations before supplying source or credentials.
- Prevent access to internal APIs, storage, Key Vault, other runners and unrelated host files.
- Expose only prepared source inputs; use ephemeral credentials/workspaces and verified cleanup.
- Obtain approval for proprietary source processing by the external provider.
- Reintroduce bounded attempts, time/cost controls, structured evidence validation and central
  harness checks, with offline and isolated-runtime qualification before enabling a schedule.

Self-hosting alone does not enforce these controls or avoid external model processing.
The previous implementation is recoverable from Git history; it is not executable here.

## Current non-AI reporting

Sanity retains the documented component matrix percentage, deterministic source-change
detection and current-run TRX mapping. It does not send source to an AI provider.
New or changed source marks coverage for review; it does not automatically classify new
functionality or recalculate the accepted component matrix without a reviewed update.

The mobile Slack summary uses the existing **Current component coverage** table in
`E2E-COVERAGE-MATRIX.md`, not the Swift-file inventory as a coverage denominator.
The percentage is `(Mapped + Partial) / all assessed components`, calculated afresh on
each run. Adding a Gap row increases the denominator and lowers the percentage. Partial
rows count toward representative mapping breadth, not complete coverage; their count and
the gap count are shown separately. The documented baseline is 19/20 (95%), including three
partial areas and one gap. It is historical, not a fresh qualification of develop.

This is **mapped functional coverage**, not Swift line,
branch or exhaustive behavioural coverage. It does not turn a green Sanity run into
release approval. Test outcomes and source-mapping confidence are separate signals.

## How it works

1. At workflow start, read the requested iOS `ios_ref` (default `develop`). Never build,
   execute or modify iOS code. Inventory committed `.swift` files under the SearchAndBook
   module source root, including UI, ViewModels, models and supporting source files.
   Native unit tests and other modules are outside this explicitly bounded inventory.
2. Compare the entire source-subtree hash with `matrixReviewedSourceTree` in
   `Contracts/ios-source-coverage.json`. Additions, removals and modifications anywhere
   in this subtree trigger **NeedsReview**, including changes inside existing files.
   Unknown source access gives **Unknown**, never a current-matrix claim. The baseline
   percentage remains explicitly documented/historical until the source impact review
   updates the component table and baseline commit/date/tree together. Detection does
   not infer whether a change is a new feature, nor automatically invent coverage rows.
   Resources also trigger conservative review; functionality outside this module needs
   manual scope review. No iOS application code is modified.

   For supplementary diagnostics only, match exact source paths to the legacy four-file
   seed in `Contracts/ios-source-coverage.json`. Compare Git blob
   hashes with the reviewed version. New/unmapped files, changed mappings and deleted
   mapped files are reported for review. Any source-file change conservatively invalidates
   its mapping, even a cosmetic change. A rename is removed plus unmapped, not auto-covered.
3. Resolve mapped scenario IDs through the existing automation contract and join exact
   method names to the **current run's** TRX. Every scenario mapped to a file must pass for
   that mapping to have passing evidence. Skipped/not-run mappings remain unqualified;
   duplicate results are ambiguous. No historical passes are mixed into a current run.
4. Upload `coverage/ios-coverage.json` and `coverage/ios-coverage.md` alongside the TRX.
   Slack reports the existing component matrix's percentage, partial/gap counts, review
   date/commit, freshness flag, source/build alignment and a link to the matrix. Test
   pass/fail figures remain separate. Swift-file counts are confined to the detailed artifact.

The supplementary four representative file mappings are seeded from the documented 22 September
review, not presented as a complete or newly reviewed mapping of current develop.
They cover airport selection, calendar, passenger selector and summary. Source changes
since that audit are explicitly stale. Other files are **unmapped**, not automatically
classified as missing tests: supporting implementation files may not warrant UI scenarios.
These file-level numbers are not the coverage percentage and are not the Slack headline.
The existing human-authored E2E component matrix is the authoritative report input.

## Source/build identity and CI access

Supply `app_commit` from the upload/build record if known. It is compared with the resolved
source commit, yielding `DeclaredMatch`, `Mismatch` or `Unknown`. A declared match is not
cryptographic build attestation. Set `ios_ref` to that exact commit to assess the uploaded
build; choose develop to assess drift against develop. Do not conflate the two.

The workflow reuses `SEQUENCE_GOAL_GITHUB_APP_ID` and
`SEQUENCE_GOAL_GITHUB_APP_PRIVATE_KEY`, requesting contents-read access only to
`VAAD1ST/vaa-mobile-ios`. The App installation must permit that repository. The normal
EAPI `GITHUB_TOKEN` does not confer private cross-repository access. Availability of these
credentials/permissions on the mobile runner has not yet been live-qualified.
If source access is missing, the test run may continue and the documented matrix can still
be shown, with freshness Unknown and source/build comparison unavailable. If the report
cannot be generated, Slack says unavailable. Never label the historical baseline current
when source changed or access failed.

## Maintaining mappings

Review changed functionality and test assertions, update/add component rows and their
Mapped/Partial/Gap status, then update the matrix review commit/date/source-tree hash.
Do not refresh that hash merely to clear NeedsReview. Source changes do not automatically
mean a test gap, so this review is deliberately required rather than guessed by a file counter.
For the supplementary file mappings, review source and assertions before updating `reviewedBlob`.
Add exact scenario IDs and record the review evidence in the manifest; do not refresh hashes
just to clear a warning. An unchanged file with a mapped passing scenario only establishes
representative scenario evidence, not all actions, branches, dependencies or permutations.
Changes to related files appear as unmapped/changed inventory for human impact analysis;
this is not a Swift dependency analyser. Expanding reviewed behaviour mappings remains QA work.

Local read-only scan (committed source, not dirty working-tree contents):

```powershell
./scripts/get-mobile-ios-coverage.ps1 -IosRepository <ios-checkout> -IosRef develop `
  -TrxPath <current-run.trx> -OutputDirectory <run-results>/coverage
./scripts/test-mobile-ios-coverage.ps1
./scripts/test-mobile-release-slack.ps1
```

No Appium session, booking, backend request or Slack post is made by the source scan or
offline checks. Report output contains source paths, hashes, scenario IDs and outcomes,
not Swift contents, passenger data, raw TRX errors or credentials.
