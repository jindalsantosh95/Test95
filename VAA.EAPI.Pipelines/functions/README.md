# GraphQL automation ordering

GraphQL application build stages build and package application code and run unit tests. Selected environment automation projects build in the test job after the corresponding environment deployment succeeds.

- CRM uses `build_selected_automation.yaml` to build CRM and MobileApp before its functional tests.
- Trips and CheckIn already build their selected project in their post-deployment test jobs; their application build templates must not duplicate that work.
- Both Loyalty release templates set `buildFromSource: true` for the shared functional-test jobs. Each job builds MarketingLoyalty before copying its output and running its selected tests. The default remains `false` for existing non-GraphQL callers that download `testAutomation`.
- Existing deployment dependencies, branch conditions, test categories and failure policies remain in place. Digital GraphQL test templates already build after deployment.

The shared build steps require checkout and .NET 10 on the test agent. Test jobs use their existing environment variable groups and build configuration. They do not publish a shared automation artifact, avoiding collisions between independently deployed environments.

For changes to this ordering, parse the modified YAML, trace test-stage dependencies back to DEV/SIT/UAT, check both Loyalty preparation modes, and run `scripts/verify.ps1 -SolutionPath VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.sln -SkipBuild`. Azure DevOps pipeline execution is required to validate agent capabilities and live environment tests.
