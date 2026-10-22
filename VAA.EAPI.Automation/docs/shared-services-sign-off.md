# Shared-service sign-off automation

Run `.github/workflows/shared-services-sign-off.yml` manually from GitHub Actions. The workflow can run the full tagged pack, one shared service, one consuming domain, or their intersection.

## Repository settings

Configure these GitHub Actions repository variables. Defaults in the workflow match the existing EAPI workflows, but explicit repository values are preferred.

| Variable | Purpose |
|---|---|
| `BASE_URL` | Test Resources host, normally `https://testresources.virginatlantic.com` |
| `BASE_URL_DIGITAL` | Digital test host, normally `https://digital-test.virginatlantic.com` |
| `GRAPHQL_MOBILE_PATH` | Mobile/Retailing/Trips GraphQL path |
| `GRAPHQL_TRIPS_SECURE` | Secure Trips GraphQL path |
| `GRAPHQL_BOOK_PATH` | Book GraphQL path |
| `GRAPHQL_SEARCH_PATH` | Search GraphQL path |
| `GRAPHQL_MYACCOUNT_PUBLIC_PATH` | MyAccount public GraphQL path |
| `GRAPHQL_MYACCOUNT_SECURE_PATH` | MyAccount secure GraphQL path |
| `GRAPHQL_LOYALTY_PATH` | Loyalty GraphQL path |
| `HPP_PAGE_ALLOWED_ORIGINS` | Exact HPP page origins allowed to receive access headers |
| `HPP_HEADER_ALLOWED_ORIGINS` | Exact HPP API origins allowed to receive access headers |

Configure these GitHub Actions secrets:

| Secret | Purpose |
|---|---|
| `KEY_VAULT_URL` | Automation Key Vault URL |
| `KEY_VAULT_TENANT_ID` | Service-principal tenant |
| `KEY_VAULT_CLIENT_ID` | Service-principal client ID |
| `KEY_VAULT_CLIENT_SECRET` | Service-principal credential |
| `HPP_VAA_DIGITAL_FIRST_TOKEN` | VAA HPP access token used by the Book HPP journey |
| `HPP_DELTA_ENTERPRISE_PAYMENTS_TOKEN` | Delta HPP access token used by the Book HPP journey |
| `SLACK_WEBHOOK_URL_SHARED_SERVICES` | Existing incoming webhook configured for the `shared-services-signoff-results` Slack channel |

The Key Vault identity must be able to read the existing automation secrets used by the selected tests. The important dependencies include GraphQL and microservice subscription keys, automation storage-account credentials and data, loyalty test accounts, the PDM/reference SQL connection strings, Cosmos keys, and `TESTING-CmdbOrderDocument-Automation-Key` for Book E2E.

The workflow posts one aggregated Slack message after all selected consumer jobs finish. The message includes the environment, selected service and consumer, aggregate passed/failed/skipped counts, triggering user and a workflow-run link. Notification failure is reported as a workflow warning and does not replace the test result.

## Runner and test-data prerequisites

- Runner labels: `self-hosted`, `Windows`, `eapi-automation`.
- Access to .NET SDK downloads and the repository NuGet sources. The workflow installs SDK `10.0.x` into the runner's temporary directory.
- Network access to the selected environment, Key Vault, SQL, Cosmos DB and Azure Table Storage.
- Playwright/Selenium browser support for the Book HPP run.
- Current PNR data in the automation storage tables for FlightOperations and Trips.
- Paid-seat PNR data in `MobileAppPnrsForSeatScenarios` for Trips SeatMS/PaymentMS coverage.
- Valid loyalty-account data for MyAccount SecurityMS/InRuleMS and FlightOperations retrieval.
- An active eligible campaign plus customer-transaction Cosmos data for MarketingLoyalty InRuleMS.
- Reference database connectivity for the MyAccount direct ReferenceMS comparison.

## Filters

The workflow composes these traits:

```text
Category=SharedServicesSignOff
SharedService=<selected service>
SharedServiceConsumer=<selected domain>
```

Focused contract evidence is additionally marked with `TestType=Contract`. This is descriptive rather than a separate workflow filter: contract cases retain `Category=SharedServicesSignOff`, so selecting their shared service automatically runs them with the rest of that service's sign-off pack.

Current focused contract evidence includes SecurityMS in MyAccount, direct BrandMS in Search, OrderDataMS and Retail.OrderSearchMS in both Trips and FlightOperations, and SeatMS in both Book and Trips. These reuse the owning project's established request wrappers and storage-backed test data.

Examples:

- `SeatMS` with consumer `all` runs both Book and Trips coverage.
- `OfferMS` runs focused GraphQL and direct-MS coverage for one-way, return, flex-date, pricing, deep-link and invalid-request contracts, plus Book E2E consumption.
- `ReferenceMS` with consumer `MyAccount` runs the direct MyAccount distance check.
- `Retail.PaymentMS` runs both Book HPP/create-order and Trips paid-seat coverage.
- `Retail.OrderMgmtMS` runs the Book child journey and verifies the asynchronous downstream `CHML/HK` PNR remarks.

Every service currently backed by a tagged test is available as an individual `shared_service` workflow-dispatch choice: `SecurityMS`, `OfferMS`, `BrandMS`, `OrderDataMS`, `Retail.OrderSearchMS`, `SeatMS`, `FlightServicesMS`, `ReferenceMS`, `Retail.PaymentMS`, `Retail.OrderMgmtMS` and `InRuleMS`. Selecting one adds `SharedService=<name>` to the xUnit filter and limits the project matrix to consumers mapped to that service. Selecting a consumer as well runs only that service/consumer intersection.

This is test-pack isolation, not runtime dependency isolation. A Book or Trips GraphQL journey selected for one service can still call other downstream services required to complete that journey; its assertions provide sign-off evidence for the selected service contract.

Services without an implemented tagged test are intentionally absent from the selector. `EmailMS`, Loyalty FC-number stamping through OrderMgmtMS, partner-flight InRule and NDC OCN InRule remain deferred below. Add them to the selector and project map only when their qualifying automation tests are introduced.

## Deferred MyAccount and Loyalty coverage

The following tests are deliberately **not part of this PR**. They must not be reported as covered by the current `SharedServicesSignOff` trait or manual workflow until the deployed tests and their prerequisites have been added.

### Loyalty to Retail.OrderMgmtMS: Flying Club number stamping

Loyalty is a confirmed consumer of Retail.OrderMgmtMS. The deployed call path is:

```text
loyaltyStampFCNumToPNR
  -> StampFCNumToPNRMutationsProvider
  -> OrderManagement:MsUrl
  -> PATCH AddFFP/StampFCNumberToPNR
```

`loyaltyUpgradeVACMember` can also invoke the same provider after a successful upgrade when an eligible future PNR is found. The direct `loyaltyStampFCNumToPNR` mutation is the preferred primary sign-off test because it isolates the integration and does not conditionally skip stamping.

Required test prerequisites:

- a valid test-environment PNR with a known passenger;
- matching passenger first and last name;
- a valid Flying Club member number;
- a `graphqlv2` service token containing the `Loyalty.FCToPNR.Add` role; and
- deployed OrderMgmt URL, application ID and OCP subscription-key settings.

Acceptance criteria:

1. invoke `loyaltyStampFCNumToPNR` with the PNR, member number and matching passenger name;
2. assert an error-free GraphQL success response;
3. retrieve the PNR independently after the mutation; and
4. assert that the Flying Club number is present against the intended passenger.

The GraphQL response alone is insufficient sign-off evidence because it does not prove that the downstream PNR mutation was committed. A VAC-upgrade/autostamping journey can be added later as broader secondary coverage.

### MyAccount to InRuleMS: partner-flight points claim

`claimPartnerFlightPoints` uses a separate InRule calculation method and contract from `claimVSPoints`. Existing VS-flight coverage therefore does not prove the partner-flight consumer path.

```text
claimPartnerFlightPoints
  -> partner/Sky eligibility
  -> ReferenceMS route and cabin lookup
  -> InRulePointsCalculatorForSkyFlights
  -> customer transaction creation and partner feedback
```

Required test prerequisites:

- an authenticated, active Flying Club test account;
- an eligible and previously unclaimed partner-airline ticket;
- matching airline, flight, cabin, route, date, PNR and passenger data accepted by the partner eligibility service;
- matching ReferenceMS route and cabin data;
- partner-flight mock responses disabled; and
- access to verify the resulting customer transaction.

Acceptance criteria:

1. invoke `claimPartnerFlightPoints` through the deployed MyAccount GraphQL endpoint;
2. assert an error-free response with the expected segment, points and tier/bonus calculation;
3. assert the returned carrier, flight and route match the submitted claim;
4. verify a processed customer transaction exists for the unique ticket; and
5. verify partner feedback/status when that state is observable to automation.

Use controlled seeded partner-ticket data where possible. Input-validation tests and the existing `claimVSPoints` test must not be tagged as evidence for this separate InRule contract.
