using System.Collections.Specialized;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Factory;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint
{
    public static class LoopsFrontEndAPIEndpoints
    {

        private static string frontendAPIEnv = ConfigReader.Get("FrontendAPIEnv") ?? "";

        // Project-DEV -> MarketingLoyaltyAPI->Cosmos Acc -> SIT KeyVault
        // BAU-SIT -> LOOPS BAU SIT ->Cosmos Acc  -> SIT KeyVault
        // BAU-UAT -> New LOOPS BAU UAT ->Cosmos UAT -> UAT KeyVault        

        private static readonly string _frontendApiEndpoint = frontendAPIEnv.Is("Project-DEV") ? Vault.Default.GetSecretValue("MarketingLoyalty-FECustomerApiEndpoint") : frontendAPIEnv.Is("BAU-SIT") ? Config.TestResourcesVirginAtlantic.GetUrl("/uat/loyaltyfrontend") : Config.TestResourcesVirginAtlantic.GetUrl("/uat/v2/loyaltyfrontend");

        private static readonly Lazy<string> _frontendApiOcpSubKey =
             new(() => Vault.Default.GetSecretValue("MarketingLoyalty-FE-OcpSubKey"));

        private static NameValueCollection GetFrontendApiHeaders() =>
            new()
            {
                { "Ocp-Apim-Subscription-Key", _frontendApiOcpSubKey.Value },
                { "Content-Type", "application/json" }
            };

        #region Transaction API Endpoints

        public static HttpResponseMessage RetrieveAllTransactions(int limit, string accountNumber)
        {
            NameValueCollection headers = GetFrontendApiHeaders();

            return WebRequest.Get($"{_frontendApiEndpoint}/transactions/history/{accountNumber}/recent?limit={limit}", headers);
        }

        public static HttpResponseMessage RetrieveSubsetOfTransactionUsingPagination(string accountNumber, int pageNo, int pageSize, string filter)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Get($"{_frontendApiEndpoint}/transactions/history/{accountNumber}?pageNo={pageNo}&pageSize={pageSize}&filter={filter}", headers);
        }

        public static HttpResponseMessage RetrieveTransactionWithMemberNumberAndTxnId(string accountNumber, string transactionId)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Get($"{_frontendApiEndpoint}/transactions/{accountNumber}/{transactionId}", headers);
        }

        #endregion

        #region VRED API Endpoints


        public static HttpResponseMessage VRedStatementAPI(string bearer, string memberNumber)
        {
            string apimSubscriptionKey = Vault.Default.GetSecretValue("MYA-SubscriptionKeyVred");

            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", apimSubscriptionKey },
                { "Content-Type", "application/json" }
            };
            return WebRequest.Get(Config.VredEndPoint.GetUrl($"/statements/v2?partnerUserId={memberNumber}"), headers);
        }

        public static HttpResponseMessage VRedBalanceAPI(string bearer, string memberNumber)
        {
            string apimSubscriptionKey = Vault.Default.GetSecretValue("MYA-SubscriptionKeyVred");

            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", apimSubscriptionKey },
                { "Content-Type", "application/json" }
            };
            return WebRequest.Get(Config.VredEndPoint.GetUrl($"/balances/v2?partnerUserId={memberNumber}"), headers);
        }

        public static HttpResponseMessage VRedMemberStatusAPI(string bearer, string memberNumber)
        {
            string apimSubscriptionKey = Vault.Default.GetSecretValue("MYA-SubscriptionKeyVred");

            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", apimSubscriptionKey },
                { "Content-Type", "application/json" }
            };
            return WebRequest.Get(Config.VredEndPoint.GetUrl($"members/members/v1?partnerUserId={memberNumber}&requestId=123"), headers);
        }

        #endregion

        #region Customer API Endpoints


        public static HttpResponseMessage ProcessManualRedemption(string partnerCode, string productName, string activityDate, string points, string comments, string customerId, string accountNumber, string pnr = "", string ticketNumber = "", string originAirportCode = "", string destinationAirportCode = "", string bookingClass = "", int noOfPassengers = 0, int noOfSegments = 0, string voucherNumber = "", string voucherUsage = "")
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/{accountNumber}/redemptions/manual/flight",
                                   LoopsFrontEndAPIFactory.CreateManualRedemption(partnerCode, productName, activityDate, points, comments, customerId, pnr, ticketNumber, originAirportCode, destinationAirportCode, bookingClass, noOfPassengers, noOfSegments, voucherNumber, voucherUsage),
                                   headers);
        }

        public static HttpResponseMessage CreateFlightAccuralTransaction(string bookingReference, string bookingClass, string activityDate, string airlineCode, string ticketIssueDate, string originAirport, string flightNumber, string destinationAirport, string memberNumber, string customerId, string createdOn, string comments, string ticketNumber)
        {
            NameValueCollection headers = GetFrontendApiHeaders();

            return WebRequest.Json.Post($"{_frontendApiEndpoint}/claims/retrospective/vs/flightaccural",
                                   LoopsFrontEndAPIFactory.CreateFlightAccrual(bookingReference, bookingClass, activityDate, airlineCode,
                                                                                          ticketIssueDate, originAirport, flightNumber, destinationAirport,
                                                                                          memberNumber, customerId, createdOn, comments, ticketNumber),
                                   headers);
        }

        public static HttpResponseMessage SearchMembers(string accountNumber, string emailAddress)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Get($"{_frontendApiEndpoint}/members/SearchMembers?accountNumber={accountNumber}&emailAddress={emailAddress}", headers);
        }

        public static HttpResponseMessage GetMember(string accountNumber)
        {
            NameValueCollection headers = GetFrontendApiHeaders();

            return WebRequest.Get($"{_frontendApiEndpoint}/members/{accountNumber}", headers);
        }

        public static HttpResponseMessage GetMemberVouchers(string customerId)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Get($"{_frontendApiEndpoint}/members/{customerId}/vouchers", headers);
        }

        public static HttpResponseMessage UpdateMemberPhone(string operationIdentifier, string customerId, string memberNumber, string phoneId, string phoneNumber, string updatedBy, string status)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/ProfileUpdate",
                LoopsFrontEndAPIFactory.UpdateMemberPhone(operationIdentifier, customerId, memberNumber, phoneId, phoneNumber, updatedBy, status),
                headers);
        }

        public static HttpResponseMessage UpdateMemberEmail(string operationIdentifier, string customerId, string memberNumber, string emailContactId, string emailAddress, string updatedBy, string updatedOn)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/ProfileUpdate",
                LoopsFrontEndAPIFactory.UpdateMemberEmail(operationIdentifier, customerId, memberNumber, emailContactId, emailAddress, updatedBy, updatedOn),
                headers);
        }

        public static HttpResponseMessage UpdateMemberAddress(string operationIdentifier, string customerId, string memberNumber, string addressId, string addressLine1, string addressLine2, string city, string country, string updatedBy)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/ProfileUpdate",
                LoopsFrontEndAPIFactory.UpdateMemberAddress(operationIdentifier, customerId, memberNumber, updatedBy, addressId, addressLine1, addressLine2, city, country),
                headers);
        }

        public static HttpResponseMessage AddOrUpdateAwardToTheMember(string operationIdentifier, string customerId, string memberNumber, string updatedBy, string status,
            string vaPrefCardName, string dateOfBirth, string firstName, string lastName, string title, string formalSalutation, string awardProductName, string startDate)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/ProfileUpdate",
                LoopsFrontEndAPIFactory.AddOrUpdateAwardToTheMember(operationIdentifier, customerId, memberNumber, updatedBy, status, vaPrefCardName, dateOfBirth, firstName, lastName, title, formalSalutation, awardProductName, startDate),
                headers);
        }

        public static HttpResponseMessage AwardVoucher(string customerId, string voucherName, string startDate, string endDate, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/{customerId}/vouchers",
                LoopsFrontEndAPIFactory.AwardVoucher(voucherName, startDate, endDate, transactionSource, createdByEmailId, createdByUserName),
                headers);
        }

        public static HttpResponseMessage RedeemVoucher(string customerId, string voucherId, string voucherNumber, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Put($"{_frontendApiEndpoint}/members/{customerId}/vouchers",
                LoopsFrontEndAPIFactory.RedeemVoucher(voucherId, voucherNumber, transactionSource, createdByEmailId, createdByUserName),
                headers);
        }

        public static HttpResponseMessage UpdateVoucher(string customerId, string voucherId, string voucherNumber, string startDate, string expiryDate, string status, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Patch($"{_frontendApiEndpoint}/members/{customerId}/vouchers",
                LoopsFrontEndAPIFactory.UpdateVoucher(voucherNumber, voucherId, startDate, expiryDate, status, transactionSource, createdByEmailId, createdByUserName),
                headers);
        }

        public static HttpResponseMessage TransferVoucher(string customerId, string senderAccountNumber, string recipientAccountNumber, string recipientCustomerId, string voucherNumber, string voucherId, string voucherName, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/{customerId}/vouchers/transfers",
                LoopsFrontEndAPIFactory.TransferVoucher(senderAccountNumber, recipientAccountNumber, recipientCustomerId, voucherNumber, voucherId, voucherName, transactionSource, createdByEmailId, createdByUserName),
                headers);
        }

        public static HttpResponseMessage PointsTransfer(string customerId, string senderAccountNumber, string recipientAccountNumber, string recipientCustomerId, int points, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/{customerId}/points/transfers",
                LoopsFrontEndAPIFactory.PointsTransfer(senderAccountNumber, recipientAccountNumber, recipientCustomerId, points, transactionSource, createdByEmailId, createdByUserName),
                headers);
        }

        public static HttpResponseMessage ManualCreditAdjustment(string accountNumber, string partnerCode, string productName, string activityDate, string comments, int points, int tierPoints, string transactionSource, string createdByEmailId, string createdByUserName, string customerId, string bookingReference = "")
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/{accountNumber}/creditAdjustments/manual",
                LoopsFrontEndAPIFactory.ManualCreditAdjustment(partnerCode, productName, activityDate, comments, points, tierPoints, transactionSource, createdByEmailId, createdByUserName, customerId, bookingReference),
                headers);
        }

        public static HttpResponseMessage CancellationTransaction(string customerId, string transactionId, int transactionID_AWS, string memberNumber, string activityDate, string transactionProcessedDate, string totalMiles, string points)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/transactions/cancellations",
                LoopsFrontEndAPIFactory.CancellationTransaction(customerId, transactionId, transactionID_AWS, memberNumber, activityDate, transactionProcessedDate, totalMiles, points),
                headers);
        }

        public static HttpResponseMessage MemberEnrolment(string title, string gender, string firstName, string lastName, string emailId, string addressType, string addressLine1, string city, string postCode, string country, string dateOfBirth, string phoneCountryCode, string phoneNumber, string guardianName, string guardianDateOfBirth, string guardianPhoneCountryCode, string guardianPhoneNumber, string guardianEmailId)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            headers.Add("channelUserId", "WEB");
            headers.Add("channelId", "POWERAPP");

            return WebRequest.Json.Post($"{_frontendApiEndpoint}/Enrolment/EnrolMembers",
                LoopsFrontEndAPIFactory.MemberEnrolment(title, gender, firstName, lastName, emailId, addressType, addressLine1, city, postCode, country, dateOfBirth, phoneCountryCode, phoneNumber, guardianName, guardianDateOfBirth, guardianPhoneCountryCode, guardianPhoneNumber, guardianEmailId),
                headers);
        }

        #endregion

        #region Tier Management API Endpoints

        public static HttpResponseMessage GetTierHistory(string customerId)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Get($"{_frontendApiEndpoint}/members/{customerId}/tierHistory", headers);
        }

        public static HttpResponseMessage CreateNewTier(string customerId, string tierName, string tierStartDate, string tierEndDate, string revision, string createdBy, string issueReason)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Post($"{_frontendApiEndpoint}/members/{customerId}/tierChange",
                LoopsFrontEndAPIFactory.CreateNewMemberTier(tierName, tierStartDate, tierEndDate, revision, createdBy, issueReason),
                headers);
        }

        public static HttpResponseMessage UpdateNewTier(string customerId, string docId, string tierName, string tierApprovalStatus, string revision, string updatedBy)
        {
            NameValueCollection headers = GetFrontendApiHeaders();
            return WebRequest.Json.Patch($"{_frontendApiEndpoint}/members/{customerId}/tierChange",
                LoopsFrontEndAPIFactory.UpdateNewMemberTier(docId, tierName, tierApprovalStatus, revision, updatedBy),
                headers);
        }

        #endregion

    }
}
