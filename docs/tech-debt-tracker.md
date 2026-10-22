# Technical Debt Tracker

This tracker records debt that affects agent legibility, harness coverage, architecture consistency, delivery safety, or repeated review effort. It is not a general product backlog.

Add an entry when:

- the same failure mode or review comment appears more than twice,
- a known gap is accepted because automation is currently too expensive,
- a stale pattern is likely to be copied by future agents,
- a missing guide or sensor makes future changes harder to verify.

| ID | Scope | Debt / Failure Mode | Risk | Current Control | Next Control | Status |
| --- | --- | --- | --- | --- | --- | --- |
| TD-0001 | Repository knowledge | Execution plans, decision records, and debt tracking were not first-class docs artifacts. | Context can remain trapped in prompts or PR comments. | Docs map and knowledge log. | Keep this tracker current; promote repeated debt into guides or sensors. | Open |
| TD-0002 | Harness enforcement inventory | Non-automation solution scopes do not yet have explicit owner + required status check mapping in the harness inventory. | Required checks can be inconsistent or missing across solutions, allowing uneven enforcement. | Automation scope has required check in `.github/workflows/ai-harness-vaa-eapi-automation.yml`; root harness docs define policy. | Complete enforcement inventory rows in `docs/harness-pipeline-checklist.md` with owners, pipeline paths, and required check names for each active scope; verify branch protection configuration. | Open |
| TD-0003 | Deployment coverage | `scripts/verify.ps1 -SolutionPath VAA.EAPI.Automation/VAA.EAPI.Automation.sln -SkipBuild` previously failed deployment coverage because `VAA.EAPI.Automation.Utility.HttpMocker.Deployment/Template/azuredeploy_apim.json` was missing. | Harness could not pass fail-closed required checks for this scope without manual skip flags. | Deployment coverage sensor caught and prevented the gap. | Added missing deployment asset and updated deployment project include list; keep deployment coverage sensor in place as permanent guard. | Closed (2026-07-02) |
| TD-0004 | Retail Offer generic ITA proxy rollout | Existing `ITAFlightOffer` direct and APIM callers have not yet been inventoried and provisioned for the new application-token guard. | Enforcing immediately would break legacy callers; staging both generic restrictions retains the existing direct and public-edge authorization gap until migration. | The dedicated reward calendar Function is anonymous behind the existing subscription-required Offer APIM API and its network restrictions. The generic guard is staged behind `ItaProxy:EnforceAuthorization=false` and generic APIM denial behind `blockGenericItaOffers=false` by default. | Inventory direct and APIM callers per environment, provision audience/role/allowlist, verify direct and edge behavior, then enable both generic restrictions and remove the temporary rollout switches. | Open |

## Maintenance

- Close entries when the guide, sensor, refactor, or decision record lands.
- Link to execution plans, decision records, PRs, tests, or harness updates.
- Revisit accepted gaps when risk increases or a cheap deterministic check becomes available.
