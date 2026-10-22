# Concierge subscription POC

The Concierge domain follows the existing `IStartup` registration pattern. It exposes dummy ping/pong queries, a publishing mutation and a GraphQL subscription through the existing GraphQL and Stitch Function Apps.

The four POC fields are registered only in the non-secure (`Public`) schema and excluded from the `Secure` schema. No bearer token is required for anonymous ping/pong testing. This is schema visibility for dummy POC operations; authorization for other domains is unchanged. An APIM subscription key, where required by the gateway, remains separate from user authentication.

The domain is copied to the GraphQL host only. It is excluded from the REST artifact because its message service requires the subscription provider owned by the GraphQL host. The main GraphQL CI pipeline explicitly builds, tests, publishes and merges Concierge into its deployment package.

## Operations

- `conciergePing(conversationId)` returns a fresh message with response `pong`.
- `conciergePong(conversationId)` returns a fresh message with response `ping`.
- `postConciergeMessage(conversationId, message)` publishes a reply to the caller's conversation topic and returns the same message.
- `conciergeMessageReceived(conversationId)` subscribes to that topic.

Queries do not publish events. Message IDs and timestamps are generated dynamically. Messages must contain 1 to 500 characters and the conversation ID must be non-empty.

```graphql
query {
  conciergePing(conversationId: "11111111-1111-4111-8111-111111111111") {
    messageId message response createdAtUtc
  }
  conciergePong(conversationId: "11111111-1111-4111-8111-111111111111") {
    messageId message response createdAtUtc
  }
}
```

## Hosting and testing

Follow the [native WebSocket transport guide](../VAA.EAPI.GraphQL.WebSocketHost/README.md) for host settings, APIM configuration, subscription frames, publishing mutations, acceptance checks and rollback. Keep queries and mutations on the existing HTTP endpoint. Use the same authentication state and conversation ID when subscribing and publishing. You can omit Authorization for this POC; anonymous subscribers sharing a conversation ID receive its public events. Protected GraphQL fields continue to use the existing authorization middleware.

For Digital DEV public testing, publish queries/mutations to `https://va-dv-eun-gqlsingleinstance-fn-01.avadeunase014.appserviceenvironment.net/api/v1/GraphQL` and subscribe through a WebSocket route targeting that same Public app. Do not mix the Public mutation endpoint with a Secure subscription backend: their in-memory topics are separate. Configure the APIM WebSocket backend and Stitch target accordingly. Stitch's HTTP endpoint list must include this Public schema, and Stitch must reload the schema after the domain deployment; do not add the entire Secure schema to make the POC visible.

This registration change does not provision routes or update live Key Vault endpoint configuration. The DEV app-host callback and Stitch transport changes are tracked separately in [PR #2459](https://github.com/VAAD1ST/eapi/pull/2459); deploy the transport fix to the Public app as well before end-to-end public testing.

Publication uses HotChocolate's in-memory topic sender. The native transport executes subscriptions in the existing isolated worker; Stitch bridge mode executes subscriptions through its stitched schema and delegates to the owning remote GraphQL endpoint without referencing domain projects. See [Stitch configuration and testing](../VAA.EAPI.GraphQL.Stitch/README.md). No Web PubSub adapter, negotiation mutation or Azure resource is required.

The transport is an experimental Functions host extension. It requires one instance and one worker process, and still needs validation on Windows Azure hosting. Sessions expire after five minutes; clients reconnect and resubscribe, with no event replay. See the transport guide for the complete limits and Azure acceptance checks.
