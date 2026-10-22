# Contributing to VAA.EAPI.Automation

This guide describes how to contribute safely and consistently to `VAA.EAPI.Automation.sln`.

## Scope

These guidelines apply to all projects in `VAA.EAPI.Automation`, including API, GraphQL, SOAP, and UI/mobile automation suites.

## Pull Request Checklist

Before opening a PR, ensure:

- [ ] Change is scoped to the smallest relevant project(s).
- [ ] Existing helper/fixture patterns are reused before introducing new abstractions.
- [ ] New tests include clear trait/category metadata aligned with existing conventions.
- [ ] Local validation commands were run (or reason for skipping is documented).
- [ ] Documentation was updated where applicable (`README.md`, `docs/*`, or skill references).

## Branching & Commits

- Use feature branches from the active integration branch.
- Keep commits small and logically grouped.
- Commit messages should explain *what* changed and *why*.

Suggested commit style:

- `feat(automation-myaccount): add GraphQL helper for OTP flows`
- `test(automation-mobileapp-ui): add login happy path scenario`
- `docs(automation): add local setup and execution guide`

## Coding Expectations

- Keep tests readable: **arrange, act, assert**.
- Keep step definitions thin; put UI interaction details in page objects.
- Keep shared helpers pure and reusable.
- Prefer extending existing helpers/fixtures over introducing one-off utilities.

## Validation Expectations

Run commands from the `VAA.EAPI.Automation` folder when possible.

Typical baseline:

```bash
dotnet restore VAA.EAPI.Automation.sln
dotnet build VAA.EAPI.Automation.sln -c Release
```

Run targeted tests relevant to the changed project/trait instead of full-suite runs where practical.

Before pushing, run the harness verification from the `VAA.EAPI.Automation` folder:

```pwsh
scripts/verify.ps1
```

It runs the analyzer ratchet build, the offline test suites (architecture + `Core`
behavior), and a formatting check, and is the same check an agent and CI should run.
See [`docs/HARNESS_ENGINEERING.md`](docs/HARNESS_ENGINEERING.md) ("Using the harness").

## PR Description Template

Use this structure in PRs:

1. **Summary**: what changed and why.
2. **Impacted projects**: list impacted automation projects.
3. **Validation**: commands executed and outcomes.
4. **Risks/notes**: data dependencies, environment assumptions, or known constraints.

## Ownership

- Each automation project is typically owned by its corresponding service team.
- Shared automation libraries and framework-level changes should include automation lead review.
