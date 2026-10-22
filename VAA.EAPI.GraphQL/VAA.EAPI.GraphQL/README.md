# Account-linking proof request inspection

## Shared authorization errors

The public GraphQL error filter maps `UnauthorizedAccessException` with message `Unauthorized` or `Forbidden` to error code `401` or `403` and the matching `httpStatusCode` extension before configured domain-error lookup. These messages follow the existing REST/MCP host contract. Domain services do not need a process-wide host selector. Flying Club account-summary token failures use this shared contract; their public GraphQL message is now `Unauthorized`, while the code remains `401`.

## Request inspection

The new Account proof resolver limits both transaction assertions and transaction proofs to 8,192 characters before eligibility lookup or forwarding, on GraphQL and its REST projection.

Proof creation is available through both `POST /api/v1/GraphQL` and its generated REST projection `POST /api/v1/accountCreateLinkingProof`. The proof's signed `htu` must exactly match the path used. The account-domain resolver checks that binding and the authenticated Bearer token's issuer/audience on both surfaces before profile lookup. Identity validates the signature and full proof contract and accepts only those two declared paths. The generic REST host remains domain-agnostic.

The account-linking request middleware inspects at most 65,536 bytes plus one overflow byte. It restores seekable bodies. For non-seekable bodies, its replacement retains the existing gateway's `MemoryStream.ToArray()` contract and defers complete buffering until downstream consumption. Small and large unrelated requests are covered through the unchanged `GetData` gateway path.

Requests exceeding this limit continue through the existing GraphQL pipeline unchanged. A request-local feature prevents only the new `accountCreateLinkingProof` resolver from executing on such a request, including when the proof field occurs beyond the inspected prefix. This is not a new global GraphQL body-size limit. Inspection is bounded; downstream gateway consumption retains its existing full-body memory allocation behavior.
