# Retail Reference microservice

## Mobile feature configuration

`GET /MobileFeatures` returns an object with `features: Dictionary<string, string>`.
The endpoint uses the existing OAuth/APIM and default middleware security boundary.
`MobileFeaturesController` directly injects `IMobileFeaturesQueryHandler`, registered
in `Program.cs`; the handler reads through `IAzureBlobService`. This path does not
use MediatR or its request/handler interfaces.
Only client-visible feature configuration belongs in this document; never put secrets
or authorization rules in mobile flags.

Store the JSON document in the existing reference storage account and container:

```json
{"features":{"nativeBooking":"false","minimumVersion":"2.4"}}
```

`MobileFeaturesBlob` selects the blob (deployment default: `mobile-features.json`).
The existing `BlobStorageUri`, `ContainerName`, and `SasKey` settings provide access.
An empty `features` object is valid. Values must be JSON strings; booleans, numbers,
objects, and arrays are rejected without coercion. Missing/null features, null values, blank keys,
invalid JSON, and storage failures do not return a successful configuration.
Blob retrieval failures become a fixed configuration-unavailable error without the original
exception or connection URI, preventing SAS values from reaching exception middleware.

The shared blob service caches content according to `AzureBlobServiceBlobExpiration`
(default: one day). Account for that delay when changing flags; this is not an
instant emergency switch. Existing HTTP/GraphQL cache policy can add further delay.

Deploy the blob before enabling callers, then deploy Reference and import its
generated OpenAPI through the existing pipeline, followed by GraphQL. Environment
parameters supply the blob name; no new storage resource or pipeline is required.
On 2026-10-12, `referencedata/mobile-features.json` was initialized directly in
DEV (`avadeunstreferencems`), SIT (`avateunstreferencems`), and UAT
(`avaseunstreferencems`) with `{"features":{}}`. Each upload used create-only
conditions and was downloaded again to verify the exact content. No features
are enabled by this initial configuration. Production configuration and the
application deployment remain separate rollout steps; no repository example file
is maintained.

See [the sequence and client contract](../../docs/sequence-diagrams/vaa-eapi-mobile-features.md).
