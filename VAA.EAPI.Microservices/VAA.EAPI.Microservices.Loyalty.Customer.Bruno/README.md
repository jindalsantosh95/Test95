# Loyalty Customer Microservice Bruno collection

This collection contains a request for every HTTP-triggered endpoint in
VAA.EAPI.Microservices.Loyalty.Customer.

Select the localhost environment before sending a request. Its values are
synthetic test data and its base URL is the local Azure Functions host. The
collection does not store credentials: provide access_token (and, when
needed, apim_subscription_key) as Bruno secrets, then enable the inherited
headers for the target environment.

Enrolment, award, and account-management requests change customer data or
start asynchronous processing. Only use those requests with authorised,
non-production test data.
