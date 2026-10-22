using System.Collections.Specialized;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Helpers.Requests
{
    public static class Endpoints
    {
        private static readonly string _env = Config.Environment;

        private static readonly string _frontendApiOcpSubKey = Vault.Default.GetSecretValue("MarketingLoyalty-FE-OcpSubKey");

        private static string loopsEnv = _env.Is("UAT") ? "uat/v2" : "uat";

        private static readonly string _frontendApiEndpoint = Config.TestResourcesVirginAtlantic.GetUrl($"/{loopsEnv}/loyaltyfrontend");

        private static NameValueCollection GetFrontendApiHeaders() =>
            new()
            {
                { "Ocp-Apim-Subscription-Key", _frontendApiOcpSubKey },
                { "Content-Type", "application/json" }
            };

        public static HttpResponseMessage CreateMemberTier(string env, string customerId, string bearer, string updatedOn, string tierStartDate, string tierEndDate, int revision, string tierName, string appliedRuleType)
        {
            NameValueCollection headers = new ()
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MarketingLoyalty-BackendCustomerSubscriptionKey-UAT") }
            };

            string customerApiEndpoint = env.Is("sit") ? Config.TestResourcesVirginAtlantic.GetUrl("/UAT/LoyaltyManagement/Enterprise/CustomerManagement") :
                    Config.TestResourcesVirginAtlantic.GetUrl("/UAT/V2/LoyaltyManagement/Enterprise/CustomerManagement");

            return WebRequest.Json.Post($"{customerApiEndpoint}/customers/{customerId}/tier",
                Factory.CreateMemberTier(updatedOn, tierStartDate, tierEndDate,
                revision, tierName, appliedRuleType), headers);
        }

        public static HttpResponseMessage UpdateMemberTier(string env, string customerId, string bearer, string docId, string updatedOn, string tierStartDate, string tierEndDate, int revision)
        {
            NameValueCollection headers = new ()
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MarketingLoyalty-BackendCustomerSubscriptionKey-UAT") }
            };

            string customerApiEndpoint = env.Is("sit") ? Config.TestResourcesVirginAtlantic.GetUrl("/UAT/LoyaltyManagement/Enterprise/CustomerManagement") :
                   Config.TestResourcesVirginAtlantic.GetUrl("/UAT/V2/LoyaltyManagement/Enterprise/CustomerManagement");

            return WebRequest.Json.Patch($"{customerApiEndpoint}/customers/{customerId}/tier",
                Factory.UpdateMemberTier(docId, updatedOn, tierStartDate, tierEndDate,
                revision), headers);
        }

        public static HttpResponseMessage CreateTransaction(string env, string bearer, string ocpSubKey, string customerId, string memberNumber, string partnerCode, string productName, string activityDate, string comments, int tierPoints, int points, string productDescription, string partnerName, string partnerCategory, string productCategory, string transactionSource, string createdBy, string transactionChannel, string statementText, string activityType)
        {
            NameValueCollection headers = new ()
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}/digital/vred/VredTransaction/CreateTransaction"), Factory.CreateTransaction(customerId, memberNumber, partnerCode, productName, activityDate, comments, tierPoints, points, productDescription, partnerName, partnerCategory, productCategory, transactionSource, createdBy, transactionChannel, statementText, activityType), headers);
        }

        public static HttpResponseMessage GetTransactionResponse(string bearer, string ocpSubKey, string url)
        {
            NameValueCollection headers = new ()
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Get(url, headers);
        }

        public static HttpResponseMessage GetMemberPointBalance(string env, string bearer, string ocpSubKey, string memberNumber)
        {
            NameValueCollection headers = new ()
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/vredMicroservice/{memberNumber}/VirginPoints"), headers) : WebRequest.Get(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}/digital/vred/{memberNumber}/VirginPoints"), headers);
        }

        public static HttpResponseMessage UpdateFCMemberStatus(string memberNumber, string customerId, string firstName, string lastName, string dob, string status, string title)
        {
            NameValueCollection headers = GetFrontendApiHeaders();

            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/ProfileUpdate",
                 Factory.UpdateFCMemberStatus(memberNumber, customerId, firstName, lastName, dob, status, title),headers);
        }

        public static HttpResponseMessage GetSchema(string msEndpoint, string ocpSubKey)
        {
            NameValueCollection headers = new ()
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey}
            };

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/{msEndpoint}/swagger.json"), headers);
        }

        public static HttpResponseMessage CreateFlightAccuralTransaction(string bookingReference, string bookingClass, string activityDate, string airlineCode, string ticketIssueDate, string originAirport, string flightNumber, string destinationAirport, string memberNumber, string customerId, string createdOn, string comments, string ticketNumber)
        {
            NameValueCollection headers = GetFrontendApiHeaders();

            return WebRequest.Json.Post($"{_frontendApiEndpoint}/claims/retrospective/vs/flightaccural", Factory.CreateFlightAccrual(bookingReference, bookingClass, activityDate, airlineCode, ticketIssueDate, originAirport, flightNumber, destinationAirport, memberNumber, customerId, createdOn, comments, ticketNumber),headers);
        }

        public static HttpResponseMessage AddOrUpdateAwardToTheMember(string operationIdentifier, string customerId, string memberNumber, string updatedBy, string status,
            string vaPrefCardName, string dateOfBirth, string firstName, string lastName, string title, string formalSalutation, string awardProductName, string startDate)
        {
            NameValueCollection headers = GetFrontendApiHeaders();

            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/ProfileUpdate",
                Factory.AddOrUpdateAwardToTheMember(operationIdentifier, customerId, memberNumber, updatedBy, status, vaPrefCardName, dateOfBirth, firstName, lastName, title, formalSalutation, awardProductName, startDate), headers);

        }

        public static HttpResponseMessage AwardVoucher(string customerId, string voucherName, string startDate, string expiryDate, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/{customerId}/vouchers",
                Factory.AwardVoucher(voucherName, startDate, expiryDate, transactionSource, createdByEmailId, createdByUserName),
                headers);
        }
    }
}
