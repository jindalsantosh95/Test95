# GraphQL RefData domain

The public schema exposes mobile bootstrap configuration through:

```graphql
query MobileFeatures {
  mobileFeatures {
    features
  }
}
```

```json
{"data":{"mobileFeatures":{"features":{"nativeBooking":"false","minimumVersion":"2.4"}}}}
```

`features` uses the existing `Any` scalar to retain a JSON object with string values,
backed by a C# `Dictionary<string, string>`. It has no nested GraphQL selection set.
The explicit dictionary projection prevents Hot Chocolate from returning a list of
key/value pairs. Mobile clients should map this scalar to a string dictionary.

The query delegates to Retail Reference `GET /MobileFeatures` using
`ReferenceMSMobileFeaturesUrl`, the existing `ReferenceMSOcpKey`, and an OAuth token
for `RefDataAPI:AppId`. Deployment templates and environment parameters carry the
new URL. The public registration supports startup before member login and preserves
the host's existing access controls; it is not registered on the secure-only schema.

Treat absent/unrecognized flags and fetch failures as disabled by default. For
boolean flags, agree on the string `"true"` with the configuration owner; other
values remain strings. Client flags do not replace server-side authorization.
An intentionally empty configuration succeeds. Missing server configuration is a
technical error (including downstream 404 mapped to code `500`), not a missing user
resource. The REST router uses its existing explicit error-code handling.

Request cancellation stops waiting for OAuth and is passed to the Reference HTTP call.
The shared OAuth interface has no cancellation parameter, so token acquisition itself
may finish independently after the caller stops waiting.

See [the sequence and rollout notes](../../docs/sequence-diagrams/vaa-eapi-mobile-features.md)
and `VAA.EAPI.GraphQL.Bruno/Queries/mobileFeatures.bru` for a request example.
