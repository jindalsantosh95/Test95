# VAA.EAPI.Microservices.Retail.Order.Payment

## Endpoints

- `POST /holiday/payment`  
  Proxies the request payload to the configured CJS checkout payment-app endpoint and returns the downstream JSON response.
  The request body must include `bookingReference`, `surname`, and `dateOfBirth` (used for MMB query login) in addition to the CJS payment-app payload fields.

## Configuration

- `CjsCheckoutPaymentAppUrl`  
  Full URL for the CJS checkout payment-app endpoint (example: `https://stg.virginatlantic.com/cjs-checkout-api/v2/payment/app`).

- `CjsCheckoutPaymentAppVersion`  
  Value used for the `app-version` header when calling CJS (default `1.0.0`).

- `CjsCheckoutPaymentAmountUrl`  
  Full URL for the CJS checkout payment amount endpoint (example: `https://stg.virginatlantic.com/cjs-checkout-api/v2/cpd/payment/amount`).

- `MmbQueryLoginUrl`  
  Full URL for the MMB query login endpoint (example: `https://vsolpublic-uat.webdev.vholsinternal.co.uk/mmb-query-api/login`).
