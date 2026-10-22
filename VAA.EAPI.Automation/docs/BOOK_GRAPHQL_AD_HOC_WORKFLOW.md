# Book GraphQL ad-hoc QA workflow

The **Book GraphQL Ad-hoc Journey** GitHub Actions workflow lets a QA run one generated booking journey without editing the checked-in scenario catalogue.

## Running a journey

1. Open **Actions** in GitHub and select **Book GraphQL Ad-hoc Journey**.
2. Select **Run workflow** and choose the branch containing the workflow.
3. Complete the route, journey, passenger, booking, seat and payment fields.
4. Run the workflow and open the job summary to see the generated scenario, result counts, PNR and lead passenger surname.
5. Download the artifact for the generated JSON and TRX evidence. Artifacts are retained for 14 days.

The job uses the existing network-restricted `eapi-automation` Windows runner and repository environment configuration. It exercises the full Book E2E path through search, basket, passenger details, optional seats, payment, order creation and post-order checks.

## Customer state

For a revenue booking, choose `Guest` for an anonymous journey or `LoggedIn` to retrieve a Flying Club test member at the selected tier and authenticate before search. Redemption and Pay With Points journeys always authenticate, regardless of the customer-state selection.

Logged-in journeys use managed automation test accounts; QAs do not enter membership credentials in the workflow.

## Mixed-carrier journeys

Set **Marketing airline** to the carrier used to select the offer. Set **Required operating airline** to a different two-letter airline code to require that operating carrier on at least one outbound or inbound leg. For example, use marketing airline `VS` and required operating airline `DL`.

Availability is data-dependent. If no matching offer is available, the run fails with the search diagnostics rather than silently selecting a different journey.

## Retries

Ad-hoc runs retry transient basket retrieval failures up to three times, with one-second then two-second delays. HTTP/transport timeouts, connection resets, throttling and gateway/service-unavailable responses are considered transient. Redemption and Pay With Points journeys also enable the existing loyalty lookup retry.

Payment and order creation are never automatically replayed. This prevents a retry from producing a duplicate payment or PNR.

## Conditional fields

- Logged-in revenue, `Redemption` and `PayWithPoints` journeys require a loyalty tier.
- `PayWithPoints` also requires a points amount.
- Any seat mode other than `None` uses the selected seat product and cabin.
- At least one adult, teen or child is required; all passenger counts must be non-negative whole numbers.

Do not enter real customer or payment data. Payment card test data remains controlled by the existing automation defaults and secrets.

## Evidence and troubleshooting

The run summary records the exact generated scenario. The uploaded artifact contains:

- `book-ad-hoc-scenario.json`, suitable for reproducing the request;
- the `.trx` test result, including the failing step and automation output.

When order creation succeeds, the summary includes the PNR and lead passenger surname captured by the Book automation. When no PNR is produced, the summary says so explicitly and includes the classified failure analysis.

The workflow is intentionally manual and runs one journey per dispatch. Different GitHub users can dispatch journeys concurrently; runs from the same user queue behind one another. Physical parallelism is limited by the number of available self-hosted runners carrying the `eapi-automation` label. Scheduled regression coverage continues to use the existing checked-in scenario catalogue.
