# MyAccount pre-push validation gate

These requirements apply to every agent change under `VAA.EAPI.Automation.MyAccount/`.

Before an agent runs `git commit` or `git push`, it must run and report the outcome of the local equivalents of the required pull-request checks. A failed command stops the commit and push until the cause is fixed or the user explicitly accepts the failed status.

From the repository root, run the closed-loop checks:

```pwsh
.\scripts\check-harness-enforcement-inventory.ps1 -FailOnGap
.\scripts\check-context-routing.ps1 -MaxAgeDays 30
.\scripts\check-harness-remediation-log.ps1 -MaxReviewAgeDays 14
```

From the repository root, run the docs-agent-loop and repository-parity equivalents:

```pwsh
.\scripts\verify.ps1 -ScopePath . -SkipBuild -SkipDeploymentCheck
.\scripts\verify.ps1 -ProjectPath 'VAA.EAPI.Automation/VAA.EAPI.Automation.MyAccount/VAA.EAPI.Automation.MyAccount.csproj' -SkipBuild
```

From `VAA.EAPI.Automation/`, run the Automation harness and CI-equivalent verification:

```pwsh
.\scripts\verify.ps1 -Base origin/main
```

Do not run `dotnet restore` or `dotnet build` separately. The verification scripts own the required restore/build/test work. The agent must state which checks passed, failed, or were explicitly waived before requesting or performing a commit/push.
