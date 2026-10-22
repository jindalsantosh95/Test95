# Knowledge Repo

Local knowledge log for repository commits. Committed entries are append-only; the latest matching `pending` pre-commit entry may be refreshed to avoid duplicate captures.

## Repository snapshot

- Repository: multi-solution EAPI .NET monorepo
- Runtime baseline: .NET 10 for current projects, with legacy projects migrated as they are touched
- Scope model: entries are project/area-aware and may cover microservices, GraphQL, web, facade, orchestration, deployment, pipeline, docs, scripts, or harness changes
- Historical entries may mention the original solution that introduced this log; new entries should identify the impacted solution, project, or area rather than assuming one default solution.

## Entry schema

Each appended entry contains:

- `timestamp` (UTC)
- `sha` (or `pending` in pre-commit mode)
- `summary`
- `impacted projects`
- `impacted areas`
- `validation`
- `risks/notes`
- `files`

Legacy entries may omit `validation`, `risks/notes`, or `impacted areas` if they were captured before this schema update. Older `Projects` values may also include non-project areas such as `docs`, `scripts`, or `(root)`.

## Entries

### 2026-05-20 07:52:32 UTC - acc4e15bac5e78a09e8a2e728d43ae9a56f98767

- Summary: initial ai repo for orchestration solution
- Projects:
  - .githooks
  - docs
  - scripts
- Files:
  - VAA.EAPI.Orchastration/.githooks/post-commit
  - VAA.EAPI.Orchastration/.githooks/pre-commit
  - VAA.EAPI.Orchastration/AGENTS.md
  - VAA.EAPI.Orchastration/README.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo-guide.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md
  - VAA.EAPI.Orchastration/scripts/update-knowledge-repo.ps1
  - VAA.EAPI.Orchastration/scripts/verify.ps1

### 2026-05-20 07:53:00 UTC - 1fd53caaf224071f08e75b9b9617f0cc9aa2ade2

- Summary: updated knowledge repo
- Projects:
  - docs
- Files:
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md

### 2026-05-20 07:56:50 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - .githooks
  - docs
  - scripts
- Files:
  - VAA.EAPI.Orchastration/.githooks/post-commit
  - VAA.EAPI.Orchastration/.githooks/pre-commit
  - VAA.EAPI.Orchastration/AGENTS.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo-guide.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md
  - VAA.EAPI.Orchastration/scripts/update-knowledge-repo.ps1

### 2026-05-20 07:59:33 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - docs
- Files:
  - VAA.EAPI.Orchastration/AGENTS.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo-guide.md

### 2026-05-20 08:05:00 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - docs
- Files:
  - VAA.EAPI.Orchastration/AGENTS.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo-guide.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md

### 2026-05-20 12:48:38 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - docs
- Files:
  - VAA.EAPI.Orchastration/docs/PATTERNS.md

### 2026-05-20 17:37:07 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - docs
- Files:
  - VAA.EAPI.Orchastration/docs/sequence-diagrams.md
  - VAA.EAPI.Orchastration/docs/workflows.md

### 2026-05-21 17:03:15 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - .githooks
  - docs
  - scripts
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/.githooks/pre-commit
  - VAA.EAPI.Orchastration/docs/knowledge-repo-guide.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md
  - VAA.EAPI.Orchastration/scripts/update-knowledge-repo.ps1

### 2026-05-21 17:03:46 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - .githooks
  - docs
  - scripts
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/.githooks/pre-commit
  - VAA.EAPI.Orchastration/docs/knowledge-repo-guide.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md
  - VAA.EAPI.Orchastration/scripts/update-knowledge-repo.ps1

### 2026-05-21 17:04:07 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - .githooks
  - docs
  - scripts
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/.githooks/pre-commit
  - VAA.EAPI.Orchastration/docs/knowledge-repo-guide.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md
  - VAA.EAPI.Orchastration/scripts/update-knowledge-repo.ps1

### 2026-05-22 11:16:28 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - VAA.EAPI.Orchestration.Notifications
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications/Triggers/Timer/CheckInOpeningTimerFunction.cs

### 2026-05-22 11:19:26 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - (root)
  - .githooks
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/.githooks/pre-commit
  - VAA.EAPI.Orchastration/AGENTS.md
  - VAA.EAPI.Orchastration/docs/configuration-reference.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/startup-and-di.md
  - VAA.EAPI.Orchastration/docs/triggers-and-endpoints.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md

### 2026-05-22 11:20:21 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - (root)
  - .githooks
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/.githooks/pre-commit
  - VAA.EAPI.Orchastration/AGENTS.md
  - VAA.EAPI.Orchastration/docs/configuration-reference.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/startup-and-di.md
  - VAA.EAPI.Orchastration/docs/triggers-and-endpoints.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md

### 2026-05-22 11:21:31 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - (root)
  - .githooks
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/.githooks/pre-commit
  - VAA.EAPI.Orchastration/AGENTS.md
  - VAA.EAPI.Orchastration/docs/configuration-reference.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/startup-and-di.md
  - VAA.EAPI.Orchastration/docs/triggers-and-endpoints.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md

### 2026-05-27 11:29:15 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/docs/PATTERNS.md
  - VAA.EAPI.Orchastration/docs/sequence-diagrams/vaa-eapi-orchestration-notificationstarlink.md

### 2026-05-27 11:40:53 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/docs/sequence-diagrams/vaa-eapi-orchestration-notificationstarlink.md

### 2026-05-27 15:09:57 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - (root)
  - .codex
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/.codex/hooks.json
  - VAA.EAPI.Orchastration/.codex/hooks/permission_request_policy.ps1
  - VAA.EAPI.Orchastration/.codex/hooks/post_tool_use_feedback.ps1
  - VAA.EAPI.Orchastration/.codex/hooks/pre_tool_use_policy.ps1
  - VAA.EAPI.Orchastration/.codex/hooks/session_start_context.ps1
  - VAA.EAPI.Orchastration/.codex/hooks/stop_verify_reminder.ps1
  - VAA.EAPI.Orchastration/README.md
  - VAA.EAPI.Orchastration/docs/harness-contract.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/harness-pipeline-checklist.md

### 2026-05-28 07:33:11 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - .github
  - docs
  - scripts
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/.github/copilot-instructions.md
  - VAA.EAPI.Orchastration/docs/harness-contract.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/harness-pipeline-checklist.md
  - VAA.EAPI.Orchastration/scripts/drift_coverage_scan.ps1
  - VAA.EAPI.Orchastration/scripts/inferential_review.ps1

### 2026-05-28 14:59:01 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/docs/sequence-diagrams/vaa-eapi-orchestration-notificationstarlink.md

### 2026-05-29 11:28:14 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - (root)
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/AGENTS.md
  - VAA.EAPI.Orchastration/docs/harness-pipeline-checklist.md
  - VAA.EAPI.Orchastration/docs/PATTERNS.md
  - VAA.EAPI.Orchastration/docs/sequence-diagrams.md
  - VAA.EAPI.Orchastration/docs/sequence-diagrams/vaa-eapi-orchestration-checkinnotification.md
  - VAA.EAPI.Orchastration/docs/sequence-diagrams/vaa-eapi-orchestration-notificationstarlink.md

### 2026-05-29 11:33:27 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/docs/sequence-diagrams.md

### 2026-05-29 11:36:34 UTC - pending

- Summary: pending commit (captured at pre-commit)
- Projects:
  - (root)
  - docs
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj),scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - none captured
- Files:
  - VAA.EAPI.Orchastration/README.md
  - VAA.EAPI.Orchastration/docs/readme.md

### 2026-05-29 11:54:45 UTC - pending

- Summary: pending orchestration update (captured at pre-commit)
- Projects:
  - (none)
- Areas:
  - .githooks
  - docs
  - scripts
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj)
  - scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - harness or hook automation changed; verify local hook behavior
- Files:
  - VAA.EAPI.Orchastration/.githooks/pre-commit
  - VAA.EAPI.Orchastration/docs/knowledge-repo-guide.md
  - VAA.EAPI.Orchastration/scripts/update-knowledge-repo.ps1

### 2026-05-29 12:08:07 UTC - pending

- Summary: pending behavior harness alignment
- Projects:
  - VAA.EAPI.Orchestration.Notifications.Tests
- Areas:
  - docs
  - scripts
- Validation:
  - PowerShell parser check for scripts/verify.ps1 and scripts/check-deployment-projects.ps1
  - scripts/check-deployment-projects.ps1
  - dotnet test VAA.EAPI.Orchestration.Notifications.Tests/VAA.EAPI.Orchestration.Notifications.Tests.csproj -c Release --nologo
  - scripts/verify.ps1 -SkipBuild
  - scripts/verify.ps1
- Risks/Notes:
  - behavior harness changed; keep sequence diagrams mapped to executable tests or explicit proposed status
  - deployment project policy is now enforced by verify/pre-commit lightweight checks
- Files:
  - VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications.Tests/BehaviorFixtures/checkin_notification_expectations.json
  - VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications.Tests/CheckInNotificationBehaviorTests.cs
  - VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications.Tests/NotificationEventContractTests.cs
  - VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications.Tests/VAA.EAPI.Orchestration.Notifications.Tests.csproj
  - VAA.EAPI.Orchastration/docs/behavior-harness.md
  - VAA.EAPI.Orchastration/docs/harness-contract.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/harness-pipeline-checklist.md
  - VAA.EAPI.Orchastration/docs/readme.md
  - VAA.EAPI.Orchastration/docs/sequence-diagrams.md
  - VAA.EAPI.Orchastration/scripts/check-deployment-projects.ps1
  - VAA.EAPI.Orchastration/scripts/verify.ps1

### 2026-05-29 14:03:57 UTC - pending

- Summary: pending sequence diagram folder rename
- Projects:
  - VAA.EAPI.Orchestration.Notifications.Tests
- Areas:
  - docs
- Validation:
  - no remaining old sequence diagram folder path references
  - scripts/verify.ps1 -SkipBuild
  - dotnet test VAA.EAPI.Orchestration.Notifications.Tests/VAA.EAPI.Orchestration.Notifications.Tests.csproj -c Release --no-restore --nologo --filter FullyQualifiedName~NotificationEventContractTests
- Risks/Notes:
  - sequence diagram folder renamed to `docs/sequence-diagrams`; keep future links path-safe
- Files:
  - VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications.Tests/NotificationEventContractTests.cs
  - VAA.EAPI.Orchastration/docs/behavior-harness.md
  - VAA.EAPI.Orchastration/docs/readme.md
  - VAA.EAPI.Orchastration/docs/sequence-diagrams.md
  - VAA.EAPI.Orchastration/docs/knowledge-repo.md
  - VAA.EAPI.Orchastration/docs/sequence-diagrams/vaa-eapi-orchestration-checkinnotification.md
  - VAA.EAPI.Orchastration/docs/sequence-diagrams/vaa-eapi-orchestration-notificationstarlink.md

### 2026-05-29 13:04:05 UTC - pending

- Summary: pending orchestration update (captured at pre-commit)
- Projects:
  - VAA.EAPI.Orchestration.Notifications.Tests
- Areas:
  - docs
  - scripts
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj)
  - scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - deployment surface changed; verify deployment project and pipeline alignment
  - harness or hook automation changed; verify local hook behavior
- Files:
  - VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications.Tests/BehaviorFixtures/checkin_notification_expectations.json
  - VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications.Tests/CheckInNotificationBehaviorTests.cs
  - VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications.Tests/NotificationEventContractTests.cs
  - VAA.EAPI.Orchastration/docs/behavior-harness.md
  - VAA.EAPI.Orchastration/docs/harness-contract.md
  - VAA.EAPI.Orchastration/docs/harness-engineering.md
  - VAA.EAPI.Orchastration/docs/harness-pipeline-checklist.md
  - VAA.EAPI.Orchastration/docs/readme.md
  - VAA.EAPI.Orchastration/docs/sequence-diagrams.md
  - VAA.EAPI.Orchastration/scripts/check-deployment-projects.ps1
  - VAA.EAPI.Orchastration/scripts/verify.ps1

### 2026-06-01 08:29:42 UTC - pending

- Summary: pending orchestration update (captured at pre-commit)
- Projects:
  - (none)
- Areas:
  - scripts
- Validation:
  - central package management guard (no inline PackageReference Version= in staged .csproj)
  - scripts/verify.ps1 -SkipBuild
- Risks/Notes:
  - harness or hook automation changed; verify local hook behavior
- Files:
  - VAA.EAPI.Orchastration/scripts/drift_coverage_scan.ps1

### 2026-06-08 14:03:30 UTC - fd670e0088

- Summary: Align harness knowledge workflow
- Projects:
  - (none)
- Areas:
  - (root)
  - .githooks
  - docs
  - scripts
- Validation:
  - scripts/check-knowledge-health.ps1
  - scripts/check-knowledge-health.ps1 -CheckRepositoryLinks
  - scripts/verify-staged.ps1 -SkipBuild
  - scripts/verify.ps1 -ScopePath . -SkipBuild
  - PowerShell parser checks for changed scripts
  - Git hook syntax check for .githooks/pre-commit
  - git diff --check
- Risks/Notes:
  - deployment surface changed; verify deployment project and pipeline alignment
  - Harness and knowledge workflow changed; root docs and scripts scope verified.
  - harness or hook automation changed; verify local hook behavior
- Files:
  - .githooks/pre-commit
  - AGENTS.md
  - docs/harness-contract.md
  - docs/harness-coverage.md
  - docs/harness-engineering.md
  - docs/harness-pipeline-checklist.md
  - docs/knowledge-repo-guide.md
  - docs/readme.md
  - docs/decisions/readme.md
  - docs/decisions/template.md
  - docs/exec-plans/readme.md
  - docs/exec-plans/active/readme.md
  - docs/exec-plans/completed/readme.md
  - docs/exec-plans/template.md
  - docs/tech-debt-tracker.md
  - scripts/check-knowledge-health.ps1
  - scripts/verify-staged.ps1
  - scripts/verify.ps1

### 2026-06-25 07:20:03 UTC - pending

- Summary: Document EAPI extension helper preference
- Projects:
  - VAA.EAPI.Microservices.Loyalty.Identity
- Areas:
  - VAA.EAPI.Microservices
  - docs
- Validation:
  - dotnet test VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.Identity.Tests/VAA.EAPI.Microservices.Loyalty.Identity.Tests.csproj -c Release --no-restore
  - powershell -ExecutionPolicy Bypass -File scripts/check-knowledge-health.ps1
  - git diff --check
- Risks/Notes:
  - Prefer shared EAPI extension helpers from VAA.EAPI.Common.Extensions over equivalent built-in static calls when available.
  - Style-only handler update; no intended runtime behavior change.
- Files:
  - VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.Identity/Application/Command/AccountAccess/CompleteSignUpHandler.cs
  - docs/standards/coding-standards.md

### 2026-06-25 07:39:57 UTC - pending

- Summary: Address CompleteSignUp PR review comments
- Projects:
  - VAA.EAPI.Microservices.Loyalty.Identity
  - VAA.EAPI.Microservices.Loyalty.Identity.Tests
- Areas:
  - VAA.EAPI.Microservices
  - docs
- Validation:
  - dotnet test VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.Identity.Tests/VAA.EAPI.Microservices.Loyalty.Identity.Tests.csproj -c Release --no-restore
- Risks/Notes:
  - Preserve requested email identity when Graph returns no existing identities for the Entra user PATCH.
  - Added validation coverage for missing member number and missing or empty email address.
- Files:
  - VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.Identity/Application/Command/AccountAccess/CompleteSignUpHandler.cs
  - VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.Identity.Tests/CompleteSignUpHandlerTests.cs
  - VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.Identity.Tests/VAA.EAPI.Microservices.Loyalty.Identity.Tests.csproj
  - VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.Identity.Tests/.mocks/completeSignUpPatchUserWithoutIdentities.mock
  - VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.Identity.Tests/.mocks/completeSignUpVerifyUserWithoutIdentities.mock

