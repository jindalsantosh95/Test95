# Authoritative UX Sequence Goals

- Status: completed
- Owner: Platform Engineering
- Created: 2026-09-04
- Scope: sequence-goal authoring, cross-repository UI delivery, and acceptance validation

## Objective

Treat every supplied UX artefact as a direct implementation instruction while using the sequence diagram as the authoritative flow-and-behaviour contract.

## Acceptance Criteria

- The sequence-diagram template captures supplied UX, authority, screen/state mapping, and visual verification.
- UI delivery agents must inspect and implement supplied UX rather than treating it as contextual inspiration.
- UI automation agents reject delivery that omits mapped screens, controls, states, or transitions.
- Deterministic checks fail supplied-UX goals without a complete authoritative UX contract.
- Existing non-UI sequence goals remain valid.

## Plan

1. Add the UX contract to authoring guidance and the diagram template.
2. Enforce supplied-UX handling in production and automation agent prompts.
3. Add deterministic validation and mutation coverage.
4. Run the full repository harness and record the result.

## Outcome

Sequence-goal authoring now requires a structured UX contract whenever a diagram supplies an image or Figma reference. Production agents must inspect and implement the artefact as a direct instruction, with the detailed sequence owning flow and behaviour. The automation phase must compare the associated implementation with every mapped screen/state and record screenshot/render evidence; semantic acceptance fails while that evidence is absent or planned. Mandatory target-repository lint, formatting, build, and test gates must also run successfully before delivery; missing prerequisites leave the goal in progress rather than deferring proof to pull-request CI. The macOS iOS production job now installs SwiftLint when absent and verifies it before generation begins. Mobile iOS branches, commits, PR titles, and pre-push validation now follow the naming and hook conventions in that repository's README.

Validation passed on 2026-09-04:

- `scripts/verify.ps1 -ScopePath . -SkipBuild -SkipDeploymentCheck`
- `scripts/check-sequence-diagram-status.ps1`
- `scripts/check-harness-rules.ps1`
- `scripts/check-agent-workflow-controls.ps1`
- YAML parsing for `.github/workflows/ai-delivery-sequence-goal-loop.yml`
- `git diff --check`
