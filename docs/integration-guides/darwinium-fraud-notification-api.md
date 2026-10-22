# Darwinium Fraud Notification API Integration Guide

Document status: Draft for integration review

Audience: Darwinium integration and support teams

Last updated: 2026-09-04

## 1. Purpose

The Fraud Notification API allows Darwinium to submit a suspected Flying Club account-fraud notification for asynchronous processing.

The API acknowledges that a validly authenticated request has been securely accepted for processing. Processing continues asynchronously after the HTTP response.

## 2. Integration summary

| Item | Value |
| --- | --- |
| Protocol | HTTPS |
| Authentication | OAuth 2.0 Client Credentials |
| Request content type | `application/json` |
| HTTP method | `POST` |
| Resource path | `/FraudAlert` (unversioned) |
| Processing model | Asynchronous |
| Success response | `202 Accepted` with a generated correlation ID in the `api-correlation-id` response header |
| Source network control | Darwinium egress IP allowlist enforced at Akamai |

After VAA has provisioned the services, VAA will provide the environment-specific base URL, OAuth tenant, client ID, scope, rate limit, and support contacts through the agreed secure onboarding channel. During that external onboarding activity, Darwinium must provide the public egress IP range from which it will call the service so VAA can arrange the Akamai allowlist. Configuration exchange and allowlist provisioning are outside the fraud-alert service delivery goal.

## 3. Prerequisites

During onboarding and before testing begins, Darwinium must:

1. Provide the fixed public egress IP range that will originate API requests so it can be registered with Akamai.
2. After VAA has provisioned the services, receive the environment-specific API and OAuth configuration through the agreed secure onboarding channel.
3. Access the OAuth client secret from the VAA-managed AWS secure storage through the agreed mechanism, without persisting it elsewhere.
4. Confirm that the client secret and access token will never be written to application logs.
5. Support TLS 1.2 or later.

Requests from an unregistered source IP address will be rejected by the Akamai edge control.

## 4. Obtain an access token

Use the OAuth 2.0 Client Credentials grant to obtain an access token from the Microsoft identity platform. The token is issued using VAA's standard access-token lifetime; this integration does not define or request a custom token TTL.

### Token request

```http
POST https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token
Content-Type: application/x-www-form-urlencoded

client_id={client-id}&
client_secret={client-secret}&
scope={api-scope}/.default&
grant_type=client_credentials
```

Example using cURL:

```bash
curl --request POST \
  "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token" \
  --header "Content-Type: application/x-www-form-urlencoded" \
  --data-urlencode "client_id={client-id}" \
  --data-urlencode "client_secret={client-secret}" \
  --data-urlencode "scope={api-scope}/.default" \
  --data-urlencode "grant_type=client_credentials"
```

Use the returned `access_token` as a bearer token. Cache it only for its stated lifetime and request a new token before or after expiry. The facade validates the token's integrity and validity period, including expiry, and requires its issuer and audience to match VAA's environment-specific configuration. Do not send the client secret to the Fraud Notification API.

## 5. Submit a fraud notification

### Request

```http
POST {api-base-url}/FraudAlert
Authorization: Bearer {access-token}
Content-Type: application/json
Accept: application/json
```

### JSON body

```json
{
  "fcNumber": "0012345678",
  "emailAddress": "someone@somewhere.com",
  "reason": "Duplicate accounts"
}
```

### Field definitions

| Field | Type | Required for successful processing | Description |
| --- | --- | --- | --- |
| `fcNumber` | String | Yes | Flying Club membership number used as the primary account lookup key. Must contain exactly 10 numeric digits (`0`–`9`). Preserve any leading zeroes. |
| `emailAddress` | String | Yes | Email address associated with the Flying Club account. Must be validly formatted. Send the actual account address; matching does not infer provider-specific aliases by removing dots or `+tags`. |
| `reason` | String | Yes | Short descriptive explanation of the fraud signal. Outer whitespace is trimmed and the resulting value must contain 1–20 characters. After successful account validation and processing, the trimmed text is recorded as a contact note against the account. |

Field names are case-sensitive and must be sent exactly as documented. Send one member per request. All three fields are required; `reason` must not be empty or contain only whitespace. Do not include access tokens, credentials, payment data, identity documents, or unrelated personal data in `reason`.

### Complete cURL example

```bash
curl --request POST \
  "{api-base-url}/FraudAlert" \
  --header "Authorization: Bearer {access-token}" \
  --header "Content-Type: application/json" \
  --header "Accept: application/json" \
  --data '{
    "fcNumber": "0012345678",
    "emailAddress": "someone@somewhere.com",
    "reason": "Duplicate accounts"
  }'
```

## 6. Acceptance and correlation

A `202 Accepted` response means the notification has been accepted for asynchronous processing. It does not confirm that:

- the Flying Club account exists;
- the supplied email address matches the account;
- the account is in a state that permits the requested change; or
- processing has completed successfully, including acceptance of the contact note containing the supplied reason.

Every `202` response includes a newly generated correlation ID in the `api-correlation-id` HTTP response header. The value is a canonical hyphenated GUID, for example `8ef93f6e-74dd-4b12-8680-433a3ea6904f`. Darwinium must retain this value with its originating notification and use it in any support enquiry.

Darwinium must not generate or submit the authoritative correlation ID. A caller-provided correlation value will not replace the value generated for the accepted request.

## 7. HTTP responses

| Status | Meaning | Darwinium action |
| --- | --- | --- |
| `202 Accepted` | The request has been accepted for asynchronous processing. The `api-correlation-id` response header contains the generated correlation ID. | Store the returned `api-correlation-id` value. Do not retry. |
| `400 Bad Request` | The request body is malformed or fails the documented field validation rules. | Correct the JSON or invalid field and submit a new request. |
| `401 Unauthorized` | The access token is missing, invalid, expired, has an invalid validity period, or does not match the configured issuer or audience. | Obtain a fresh token, verify the environment-specific OAuth configuration, and retry once. |
| `403 Forbidden` | The source network is not permitted by the Akamai edge control or another external platform policy. The facade does not perform additional role or scope authorization. | Do not retry automatically. Verify the registered egress IP address and contact the agreed support route. |
| `429 Too Many Requests` | The configured request rate has been exceeded. | Honour `Retry-After` when present and retry with bounded backoff and jitter. |
| `500 Internal Server Error` | A technical failure prevented the request from being accepted. | Retry according to the agreed transient-failure policy. Retain the timestamp and any returned diagnostic identifier. |

Error responses will not disclose account existence, account details, credentials, or implementation information.

## 8. Retry guidance

- Do not retry a request that returned `202`.
- Do not automatically retry `400` or `403` responses.
- For `401`, obtain a new access token and retry once. Repeated failure requires configuration investigation.
- For `429`, honour `Retry-After` when supplied.
- For `500`, connection timeout, or connection failure, use bounded exponential backoff with jitter according to the agreed retry limits.
- Preserve the original request body when retrying a transient failure.
- Do not retry indefinitely. Escalate repeated failures through the agreed support route.

The final retry count, maximum elapsed retry window, initial delay, and maximum delay will be agreed before go-live.

## 9. Processing outcome and operational monitoring

The `202 Accepted` response is the only automated acknowledgement returned to Darwinium by this integration. No callback or status-query API for the asynchronous processing result forms part of this delivery.

The service records a privacy-safe terminal orchestration result against the generated correlation ID through operational application logging. A successful orchestration result means the service has durably accepted every downstream event it owns (including the status-update fan-out and the contact-note event); it does not confirm that the Customer account update or another downstream consumer has already applied those events. Every failure path is logged with the appropriate outcome and severity. Existing telemetry components route those logs to configured external tools for follow-on alerting, reporting, and business visualisation. Monitoring rules, reports, dashboards, and operational alerting are provisioned separately.

If the account is already in the fraud state, or its current state is outside the configured set that permits a fraud-state change, the service records that no-action outcome through the normal operational reporting process and performs no account update or downstream status publication.

Darwinium may provide the retained correlation ID through the agreed support route if an accepted notification requires investigation. Any information returned through support remains subject to the applicable security and data-disclosure policy.

## 10. Security and data handling

- Retrieve the client credential only from the VAA-managed AWS secure storage through the agreed access mechanism; do not copy or persist it elsewhere.
- Store access tokens only in approved secure storage and only for their stated lifetime.
- Restrict credential access to the calling service and authorised operators.
- Never send credentials in the JSON request body.
- Do not log the access token, client secret, complete request body, full email address, full Flying Club number, or free-text reason.
- Use the minimum data required for the notification.
- Notify the agreed security contact immediately if a credential is suspected to be compromised.
- Coordinate client-secret rotation and egress IP changes in advance.

## 11. Support information

When raising an integration issue, provide:

- environment;
- UTC request timestamp;
- returned `api-correlation-id` value, when available;
- HTTP status code;
- whether the failure occurred while obtaining a token or submitting the notification; and
- a redacted summary of the issue.

Do not attach client secrets, access tokens, or unredacted request payloads. Support contacts and service hours will be supplied during onboarding.

## 12. Items to confirm before go-live

- API base URL for each environment.
- OAuth tenant ID, client ID, and API scope.
- Registered Darwinium egress IP addresses.
- Rate limit and `Retry-After` behavior.
- Transient-failure retry limits.
- Support contacts, service hours, and escalation process.
