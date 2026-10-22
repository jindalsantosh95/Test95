# VAA.EAPI.Microservices.CRM.Notification

Azure Functions isolated worker microservice that consumes CRM notification events from Service Bus and sends push notifications to Airship.

## Event Payload

The notification event data is deserialized into `NotificationRequest`.

Required fields:

- `message`: push alert text.
- `deepLink`: deep link opened by the push notification.
- `flightOrderData`: flight order records used to build Airship audience tags.

Optional fields:

- `customEvent`: Airship custom event emitted from the push payload.
- `campaigns`: Airship campaign categories for analytics.

Example optional payload:

```json
{
  "customEvent": {
    "name": "emit_starlink"
  },
  "campaigns": {
    "categories": [
      "Starlink",
      "WiFi"
    ]
  }
}
```

When supplied, these map to Airship as `notification.actions.add_custom_event` and top-level `campaigns`. When omitted or null, they are not included in the Airship request.

## Local Test Command

From the repository root:

```powershell
dotnet test VAA.EAPI.Microservices\VAA.EAPI.Microservices.CRM.Notification.Tests\VAA.EAPI.Microservices.CRM.Notification.Tests.csproj -c Release --no-restore
```
