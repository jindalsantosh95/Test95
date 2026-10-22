# VAA.EAPI.GraphQL.Domain.WebForms Bruno collection

This collection is limited to the operations registered by `VAA.EAPI.GraphQL.Domain.WebForms`:

- `webformRefundStatus` on the public GraphQL endpoint
- `webformSubmitRefundRequest` on the public GraphQL endpoint
- `webformUpdatePayment` on the secure GraphQL endpoint

Select an environment and populate the collection's secret variables before running a request. The checked-in requests contain no credentials, bank details, or customer data.

The local environment points both visibility modes at the local Functions host because debugger-attached development registers both public and secure operations in the same schema. Azure environments keep the public and secure endpoints separate.
