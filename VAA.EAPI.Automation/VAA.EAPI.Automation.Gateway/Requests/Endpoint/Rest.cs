using System.Collections.Specialized;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Gateway.Requests.Endpoint
{
    public static class Rest
    {
        private const string OcpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

        // Single config entry — `gatewayRest` in local.settings.local.json — holds the
        // shared REST base (e.g. "/digital/Graphql/rest/v1"). Each endpoint method
        // appends its own path segment, so endpoint names are visible at the call site
        // rather than buried in config keys.
        //
        // Host: Config.TestResourcesVirginAtlantic — testresources.virginatlantic.com
        // (resources.virginatlantic.com in prod). This is the default for customer-facing
        // public APIs and matches the 367-other endpoint methods across the solution.
        // Don't confuse with Config.DigitalTestVirginAtlantic, which is reserved for
        // non-prod internal hosts (/digital/vred/*, legacy raw /digital/graphql) and
        // doesn't exist in prod. See docs/TEST_DATA_STRATEGY.md § Base URLs.
        //
        // The `/{env}` segment is omitted in prod — non-prod hits e.g.
        // testresources.virginatlantic.com/DEV/..., but prod hits
        // resources.virginatlantic.com/... with no env path. Matches the convention in
        // Core/Helpers/Requests/Endpoints.cs:73 and FlightOperations/Microservice.cs.
        private static string Url(string env, string endpoint)
        {
            string envSegment = env.Is("Prod") ? string.Empty : env;

            string gatewayRest = (ConfigReader.Get("gatewayRest") ?? string.Empty).Trim();
            if (!gatewayRest.IsNullOrWhiteSpace() && !gatewayRest.StartsWith("/"))
            {
                gatewayRest = $"/{gatewayRest}";
            }

            gatewayRest = gatewayRest.TrimEnd('/');

            string endpointSegment = (endpoint ?? string.Empty).Trim().TrimStart('/');

            return Config.TestResourcesVirginAtlantic.GetUrl($"/{envSegment}{gatewayRest}/{endpointSegment}");
        }

        private static NameValueCollection BearerHeaders(string token) =>
            new() { { "Authorization", $"Bearer {token}" } };

        private static string GatewaySubscriptionKey()
        {
            string fromConfig = (ConfigReader.Get("gatewayOcpApimSubscriptionKey") ?? string.Empty).Trim();
            if (!fromConfig.IsNullOrWhiteSpace())
            {
                return fromConfig;
            }

            string keyVaultSecretName = (ConfigReader.Get("gatewayOcpApimSubscriptionKeySecretName") ?? string.Empty).Trim();
            return keyVaultSecretName.IsNullOrWhiteSpace()
                ? string.Empty
                : (Vault.Default.GetSecretValue(keyVaultSecretName) ?? string.Empty).Trim();
        }

        private static NameValueCollection? GatewayHeaders(NameValueCollection? additionalHeaders = null)
        {
            NameValueCollection headers = additionalHeaders == null ? new NameValueCollection() : new NameValueCollection(additionalHeaders);

            string subscriptionKey = GatewaySubscriptionKey();
            if (!subscriptionKey.IsNullOrWhiteSpace() && headers[OcpApimSubscriptionKeyHeader].IsNullOrWhiteSpace())
            {
                headers.Add(OcpApimSubscriptionKeyHeader, subscriptionKey);
            }

            return headers.Count == 0 ? null : headers;
        }

        public static HttpResponseMessage Booking(string env, string usingString, string confirmationNum, string givenNames, string surname)
        {
            return WebRequest.Json.Post(
                Url(env, "booking"),
                Factory.Rest.Booking(usingString, confirmationNum, givenNames, surname),
                GatewayHeaders());
        }

        public static HttpResponseMessage DeepLinks(string env, string tripReservationId, string deeplinkOption)
        {
            return WebRequest.Json.Post(
                Url(env, "deeplink"),
                Factory.Rest.DeepLinks(tripReservationId, deeplinkOption),
                GatewayHeaders());
        }

        public static HttpResponseMessage GetDeepLinkForPost(string env, string id)
        {
            // This operation is exposed under the non-public GraphQL REST base in SIT, so it
            // does not live under the configurable `/.../rest/public/v1` base used by other
            // Gateway REST routes.
            string url = Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("Prod") ? string.Empty : env)}/digital/Graphql/rest/v1/GetDeepLinkForPost");
            //string url = Url(env, "GetDeepLinkForPost");
            return WebRequest.Json.Post(
                url,
                Factory.Rest.GetDeepLinkForPost(id),
                GatewayHeaders());
        }

        public static HttpResponseMessage AccountMemberDetails(string env, string token, int vouchersCount, int flightsCount, int activitiesCount)
        {
            return WebRequest.Json.Post(
                Url(env, "accountMemberDetails"),
                Factory.Rest.AccountMemberDetails(vouchersCount, flightsCount, activitiesCount),
                GatewayHeaders(BearerHeaders(token)));
        }

        public static HttpResponseMessage LoyaltyIssueFlyingClubApplicationToken(string env, string token, string fcNumber)
        {
            string url = Config.TestResourcesVirginAtlantic.GetUrl(
               $"/{(env.Is("Prod") ? string.Empty : env)}/digital/Graphql/rest/v1/loyaltyIssueFlyingClubApplicationToken");

            return WebRequest.Json.Post(url, Factory.Rest.LoyaltyIssueFlyingClubApplicationToken(fcNumber), GatewayHeaders(BearerHeaders(token)));
        }

        public static HttpResponseMessage LoyaltyFlyingClubCustomerProfile(string env, string token)
        {
            string url = Config.TestResourcesVirginAtlantic.GetUrl(
                $"/{(env.Is("Prod") ? string.Empty : env)}/digital/Graphql/rest/v1/loyaltyFlyingClubCustomerProfile");

            return WebRequest.Get(url, GatewayHeaders(BearerHeaders(token)));
        }

        public static HttpResponseMessage RetrieveHouseHoldAccount(string env, string token, string memberType, string houseHoldFcNumber)
        {
            return WebRequest.Json.Post(
                Url(env, "retrieveHouseHoldAccount"),
                Factory.Rest.RetrieveHouseHoldAccount(memberType, houseHoldFcNumber),
                GatewayHeaders(BearerHeaders(token)));
        }

        // Negative-test entry point: POST an arbitrary (possibly malformed) body to the
        // retrieveHouseHoldAccount endpoint so tests can assert the gateway's error responses
        // (e.g. the 400 raised when the request key casing is wrong). Reuses the same URL and
        // bearer-header construction as the typed method above.
        public static HttpResponseMessage RetrieveHouseHoldAccountRaw(string env, string token, string body)
        {
            return WebRequest.Json.Post(
                Url(env, "retrieveHouseHoldAccount"),
                body,
                GatewayHeaders(BearerHeaders(token)));
        }

        public static HttpResponseMessage RetrieveHolidayBooking(string env, string bookingReference, string dateOfBirth, string surname)
        {
            return WebRequest.Json.Post(
                Url(env, "retrieveHolidayBooking"),
                Factory.Rest.RetrieveHolidayBooking(bookingReference, dateOfBirth, surname),
                GatewayHeaders());
        }

        // Negative-test entry point: POST an arbitrary (possibly malformed) body to the
        // retrieveHolidayBooking endpoint so tests can assert the gateway's error responses.
        public static HttpResponseMessage RetrieveHolidayBookingRaw(string env, string body)
        {
            return WebRequest.Json.Post(
                Url(env, "retrieveHolidayBooking"),
                body,
                GatewayHeaders());
        }
        public static HttpResponseMessage TripDetails(string env, string bookingReference, string dateOfBirth, string bookingSurname, string pnrLocator, string givenNames, string pnrSurname)
        {
            return WebRequest.Json.Post(
                Url(env, "TripDetails"),
                Factory.Rest.TripDetails(bookingReference, dateOfBirth, bookingSurname, pnrLocator, givenNames, pnrSurname),
                GatewayHeaders());
        }

        public static HttpResponseMessage ServicingValidatePnrOrder(string env, string pnr, string givenName, string lastName)
        {
            return WebRequest.Json.Post(
                Url(env, "servicingValidatePnrOrder"),
                Factory.Rest.ServicingValidatePnrOrder(pnr, givenName, lastName),
                GatewayHeaders());
        }
    }
}
