using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Infrastructure.AzureStorage;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using Application = VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;
using ContactDetails = VAA.EAPI.Automation.Core.DB_Model.Entities.ContactDetails;

namespace VAA.EAPI.Automation.MarketingLoyalty.Helpers
{
    public class PreRequisiteTestData
    {
        public static string env = Config.Environment;
        private readonly TokenFixture tokenFixture = new();
        private readonly CosmosFixture cosmosFixture = new();
        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private IWebDriver webDriver;
        private static TableStorageService<OtpRequestTableEntity> tableStorageService = new();

        public PreRequisiteTestData()
        {

            tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            tokenFixture.AddTokenForDynamicsEnrolmentFromKeyVault("enrolment");

            tokenFixture.AddTokenForVRedFromKeyVault("vred");

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            cosmosFixture.RegisterType<Group>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }


        public async Task<string> LinkingAChildToAParentAccount()
        {

            string headAccountCustomerId = string.Empty;
            string? headAccountMemberNumber = string.Empty;
            string subOrdinateAccountCustomerId = string.Empty;
            string? subOrdinateMemberNumber = string.Empty;

            bool isMemberAlreadyHeadOfHousehold = false;

            #region Fetching a Valid FC Member (Enabling all Red(New),Silver,Gold Members to be HOH)

            string queryForFCMDoc = "SELECT Top 10 * FROM  c where c.DocType='FlyingClubMember'  and c.Status='Active' and c.TierCode in ('Red', 'Gold','Silver') and c.MemberType='Individual' and c.HeadOfGroupMemberNumber='' and c.CreatedBy not in ('VRED','AXIS','SNAPP','WEB-MYA') order by c.LastUpdatedOn desc";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForFCMDoc);

            headAccountMemberNumber = flyingClubMemberDoc?.FirstOrDefault(x =>
           {
               headAccountCustomerId = x.CustomerId;

               isMemberAlreadyHeadOfHousehold = !x.GroupId.IsNullOrEmpty();

               string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{headAccountCustomerId}'";

               IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

               return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

           })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(headAccountMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");


            #endregion

            #region Fetching a Valid Child FC Member - Except Glass and VAC Tier Members

            queryForFCMDoc = $"SELECT Top 10 * FROM  c where c.DocType='FlyingClubMember' and c.Status='Active' and c.TierCode not in ('Glass','VAC') and c.MemberType='Individual' and c.HeadOfGroupMemberNumber='' and c.PartitionKey != '{headAccountCustomerId}' and c.CreatedBy != 'WEB-MYA' order by c.LastUpdatedOn desc";

            IEnumerable<FlyingClubMember> fcmDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForFCMDoc);

            string childStatus = string.Empty;

            string childTierCode = string.Empty;

            subOrdinateMemberNumber = fcmDoc?.FirstOrDefault(x =>
           {
               childTierCode = x.TierCode;

               subOrdinateAccountCustomerId = x.CustomerId;

               childStatus = x.Status;

               string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{subOrdinateAccountCustomerId}'";

               IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

               return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

           })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(subOrdinateMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            Assert.True(!string.IsNullOrEmpty(childStatus),
               "Expected childStatus to have a value but found null or empty value");

            Assert.True(!string.IsNullOrEmpty(childTierCode),
               "Expected child TierCode to have a value but found null or empty value");

            string initiatedBy = DataGenerator.RandomFirstName();

            #endregion

            #region Linking           

            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), subOrdinateAccountCustomerId, subOrdinateMemberNumber, headAccountCustomerId, headAccountMemberNumber, initiatedBy);

            dynamic householdLinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Request successfully initiated";

            Assert.True(householdLinkingResult.data.loyaltyHouseholdLinking.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response text but found {householdLinkingResult.ToString()} ");

            //Group Document Validations After HouseHoldLinking
            string queryForGroupDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE  c.DocType = @docType and c.HeadOfGroupCustomerId = @headAccountCustomerId and c.LastUpdatedBy = @lastUpdatedBy ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForGroupDocument).WithParameter("@docType", "Group").WithParameter("@headAccountCustomerId", headAccountCustomerId).WithParameter("@lastUpdatedBy", initiatedBy);

            Group group = cosmosFixture.CosmosQueryItem<Group>(parameterizedQuery);

            int retry = 10;

            while (group == null && retry > 0)
            {
                await Task.Delay(2000);

                group = cosmosFixture.CosmosQueryItem<Group>(parameterizedQuery);

                retry--;
            }

            Assert.True(group is not null, $"After Successful Linking, No Group Document Available for the Head CustomerId {headAccountCustomerId} .");

            Assert.True(group.HeadOfGroupCustomerId.Equals(headAccountCustomerId), $"Expected the HeadOfGroupCustomerId in Group doc to be {headAccountCustomerId} but found {group.HeadOfGroupCustomerId}");

            Assert.True(group.Status.Equals("Active"), $"Expected Group Documents Status as 'Active' but found {group.Status}");

            Assert.True(group.LastUpdatedBy.Equals(initiatedBy), $"Expected LastUpdatedBy as {initiatedBy} but found {group.LastUpdatedBy}");

            string groupDocId = group.GroupId;

            GroupHistory[] groupHistory = group.GroupHistory.OrderByDescending(x => x.LastUpdatedOn).ToArray();

            int c = groupHistory.Count(x => x.CustomerId.Equals(subOrdinateAccountCustomerId) && string.IsNullOrWhiteSpace(x.ExitDate));

            Assert.True(c == 1, $"Expected only one active household member with customerId {subOrdinateAccountCustomerId} details but found {c.ToString()}");

            GroupHistory linkedChildDetails = groupHistory.FirstOrDefault(x => x.CustomerId.Equals(subOrdinateAccountCustomerId));

            Assert.True(linkedChildDetails.CreatedBy.Equals(initiatedBy),
                $"Expected the CreatedBy in the Child Group History to be {initiatedBy} but found {linkedChildDetails.CreatedBy}");

            string actualLinkedDate = Convert.ToDateTime(linkedChildDetails.CreatedOn).ToString("dd-MMM-yyyy");

            string expectedLinkedDate = DateTime.UtcNow.Date.ToString("dd-MMM-yyyy");

            Assert.True(actualLinkedDate.Equals(expectedLinkedDate),
               $"Expected the CreatedOn in the Child  Group History to be {expectedLinkedDate} but found {actualLinkedDate}");

            //FlyingClubMember Document Validations After HouseHoldLinking

            //Parent FCM Document Validations
            string queryForFCMDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'FlyingClubMember' and c.PartitionKey='{headAccountCustomerId}' ";

            queryForFCMDocument = isMemberAlreadyHeadOfHousehold ? $"{queryForFCMDocument} and c.LastUpdatedBy = '{initiatedBy}' " : queryForFCMDocument;

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(1500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(flyingClubMember is not null, $"Failed to Retreive the Parent FCM Doc after linking for the Query {queryForFCMDocument} ");

            Assert.True(flyingClubMember.MemberType.Equals("Household"),
                $"After Successful Linking , Expected Parent/Hod MemberType as HouseHold but found {flyingClubMember.MemberType}");

            Assert.True(flyingClubMember.GroupId.Equals(groupDocId),
                $"After Successful Linking ,Expected the GroupId in Head FCM Doc as {groupDocId}  but found {flyingClubMember.GroupId}");

            Assert.True(string.IsNullOrEmpty(flyingClubMember.HeadOfGroupMemberNumber),
                $"Expected HeadOfHouseholdMember as null or empty in Head FCM Doc but found {flyingClubMember.HeadOfGroupMemberNumber}");

            //Child FCM Document Validations
            queryForFCMDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'FlyingClubMember' and c.PartitionKey='{subOrdinateAccountCustomerId}' and c.LastUpdatedBy = '{initiatedBy}' order by c._ts desc";

            flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(1500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(flyingClubMember is not null, $"Failed to Retrieve the Child FCM Doc after linking for the Query {queryForFCMDocument} ");

            Assert.True(flyingClubMember.MemberType.Equals("Individual"), $"Expected Child/SubOrdinate MemberType as Individual but found {flyingClubMember.MemberType}");

            Assert.True(string.IsNullOrEmpty(flyingClubMember.GroupId), $"Expected the GroupId in Child FCM Doc as null or empty  but found {flyingClubMember.GroupId}");

            Assert.True(flyingClubMember.HeadOfGroupMemberNumber.Equals(headAccountMemberNumber), $"Expected HeadOfHouseholdMember as {headAccountMemberNumber} in child FCM Doc but found {flyingClubMember.HeadOfGroupMemberNumber}");

            #endregion

            #region HeadAccountRetrievalAfterLinking


            response = Requests.Endpoint.GraphQL.RetrieveHouseHoldDetails(tokenFixture.GetToken("graphqlv2"), headAccountCustomerId, "");

            dynamic retrieveHouseHoldDetailsResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.ToString().Contains(subOrdinateAccountCustomerId),
                $"Expected Child Customer Id : {subOrdinateAccountCustomerId} and child Acc Details but found {retrieveHouseHoldDetailsResult.ToString()}");

            #endregion

            #region ChildAccountRetrievalAfterLinking

            response = Requests.Endpoint.GraphQL.RetrieveHouseHoldDetails(tokenFixture.GetToken("graphqlv2"), subOrdinateAccountCustomerId, "");

            retrieveHouseHoldDetailsResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.ToString().Contains(headAccountCustomerId),
                $"Expected Parent Customer Id : {headAccountCustomerId} and Head Acc Details but found {retrieveHouseHoldDetailsResult.ToString()}");

            #endregion

            return headAccountCustomerId;

        }

        public bool DeletingMultipleCampaignPromotionLinking()
        {
            #region Pre-Requsite to delete rows where same campaign is linked with multiple promotions (for matching the campaign counts)

            string duplicateCheckQuery = $"SELECT CampaignId, count(CampaignId)  FROM [dbo].[CampaignsPromotionsMapping] " +
                                        $"group by CampaignId HAVING COUNT(CampaignId) > 1";

            IEnumerable<CampaignPromotionDetails> duplicateCampaignDetails = AzureReferenceDBHelper.DuplicateCampaignPromotionQueryResponse(duplicateCheckQuery);

            if (duplicateCampaignDetails.Any())
            {
                foreach (CampaignPromotionDetails campaignPromotionDetails in duplicateCampaignDetails)
                {
                    string query = $"Delete from[dbo].[CampaignsPromotionsMapping] where RowId in " +
                                        $"(select top(1) RowId FROM[dbo].[CampaignsPromotionsMapping] where CampaignId = '{campaignPromotionDetails.CampaignId}')";

                    string? rowID = (string)AzureReferenceDBHelper.RefDbQueryResponse(query);

                    Assert.True(rowID is null, $"The data from the Campaign Promotion Detail table was not deleted for the CampaignId: {campaignPromotionDetails.CampaignId}");

                }
            }

            return true;

            #endregion Pre-Requsite checks for deleting rows where same campaign is linked with multiple promotions (for matching the campaign counts)
        }


        public async Task<(string memberNumber, string customerId)> MemberEnrolment(string channelID, string title, string firstName, string lastName, string emailId, string postCode, string country, string dateOfBirth, string enroledBy, string gender = "", string preferredPartnerFFP = "")
        {
            #region Creating an enrolment

            HttpResponseMessage enrolmentResponse = MyAccount.Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, dateOfBirth: dateOfBirth, title: title, channelUserId: enroledBy, output: null, gender: gender, preferredPartnerFFP: preferredPartnerFFP);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            string customerId = enrolmentResult["customerId"]?.ToString();

            Assert.True(!customerId.IsNullOrEmpty(), "customer id should be not blank");

            string memberNumber = enrolmentResult["memberdetails"]["memberNumber"]?.ToString();

            return (memberNumber, customerId);

            #endregion
        }

        public static async Task<HttpResponseMessage> MemberEnrolmentThrGraphql(string channelID, string title, string firstName, string lastName, string emailId, string postCode, string country, string dateOfBirth, string enroledBy, string countryCode = "GB")
        {

            HttpResponseMessage emailOtpResponse = MyAccount.Requests.Endpoint.GraphQL.GenerateEmailOtp(env, emailId).EnsureGQLResponseSuccess();

            dynamic emailOtpResult = JsonConvert.DeserializeObject<dynamic>(await emailOtpResponse.Content.ReadAsStringAsync());

            Assert.True(emailOtpResult["data"]["accountOTPGenerate"] != null, $"Email OTP Response should not be null, but found {emailOtpResult}");

            //get OTP from storage
            int otp = await EnrolmentHelper.GetOTPByEmailId(tableStorageService, emailId);

            Assert.True(otp > 0, $"OTP should be greater than zero, but found {otp}");

            Assert.True(otp.ToString().Length == 6, $"Expected OTP to be 6 digits, but found {otp}.");

            //get Token
            HttpResponseMessage validateOtpResponse = MyAccount.Requests.Endpoint.GraphQL.ValidateEmailOtp(env, emailId, otp).EnsureGQLResponseSuccess();

            dynamic validateOtpResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponse.Content.ReadAsStringAsync());

            string enrolmentToken = validateOtpResult["data"]["accountOTPValidate"]["enrolmentToken"];

            Assert.True(!enrolmentToken.IsNullOrEmpty(), $"Expected a validate enrolment token but found {validateOtpResult}");

            string enrollmentChannel = "WEB-MYA";
            string mediaCode = "Web";
            HttpResponseMessage enrolmentResponse = MyAccount.Requests.Endpoint.GraphQL.Enrolment(env, channelID, firstName, lastName, emailId, postCode, countryCode, enrolmentToken, enrollmentChannel, mediaCode);

            return enrolmentResponse;
        }

        public async Task<Dictionary<string, string>> CreateCreditAdjustmentTxnFromFrontEndApi()
        {

            Dictionary<string, string> queryResponse = new();

            #region Test Data

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType and c.StatusCode = @statusCode order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual").WithParameter("@statusCode", "Processed");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            Assert.True(txnDoc is not null,
              "Failed to Retrieve Test Data for Credit Transaction.");

            string memberNumber = txnDoc.MemberNumber;

            string customerId = txnDoc.CustomerId;

            string partnerCode = "VS";

            string productName = "Authorised Points Credit";

            string activityDate = DataGenerator.RandomPastDate(+365).ToString("dd-MMM-yyyy");

            int points = Convert.ToInt32(DataGenerator.RandomNumber(10, 100, true));

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = createdByUserName + "@testemail.co.ic";

            #endregion

            #region VRed Balance API Call for Member Before Manual Credit Txn

            HttpResponseMessage vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

            dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

            int usableBalance = Convert.ToInt32(vRedResult.usable.ToString());

            #endregion

            #region API Call


            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.ManualCreditAdjustment(memberNumber, partnerCode, productName, activityDate, "Test txn", points, 0, "AutomationScript", createdByEmailId, createdByUserName, customerId).EnsureIsSuccessStatusCode();


            #endregion

            #region retrieving the recent transaction created for the customer

            string queryForRecentTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.CustomerId = @customerId and c.TransactionType = @txnType and c.StatusCode = @statusCode order by c.CreatedOn desc";

            QueryDefinition paramRecentQuery = new QueryDefinition(query: queryForRecentTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId).WithParameter("@txnType", "Accrual").WithParameter("@statusCode", "Processed");

            Transaction recentTxnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramRecentQuery);

            Assert.True(txnDoc is not null,
              "Failed to Retrieve Test Data for Credit Transaction.");

            queryResponse.Add("transactionId", recentTxnDoc.TransactionId);

            queryResponse.Add("memberNumber", memberNumber);

            queryResponse.Add("customerId", customerId);

            return queryResponse;

            #endregion
        }


        public string GenerateLoginB2CToken(string fcNumber, string email, string firstName, string lastName, long expiryDate)
        {
            string subValue = Vault.Default.GetSecretValue("MYA-SubValue");
            string audValue = Vault.Default.GetSecretValue("MYA-AudValue");
            string tidValue = Vault.Default.GetSecretValue("MYA-TidValue");

            Claim[] customClaims = new[]
            {
                new Claim("ver","1.0"),
                new Claim("iss","https://avateunloyaltyfcb2c.b2clogin.com/1d08e914-58fb-4916-9a00-b7230ca7cb8e/v2.0/"),
                new Claim("sub", subValue),
                new Claim("aud", audValue),
                new Claim("exp", expiryDate.ToString()),
                new Claim("acr", "b2c_1a_va_digital_signup_signin"),
                new Claim("nonce", "defaultNonce"),
                new Claim("iat", 1699961809.ToString()),
                new Claim("auth_time", 1699961809.ToString()),
                new Claim("memberStatus", "ACTIVE"),
                new Claim("memberType", "Individual"),
                new Claim("EmailAddress", email),
                new Claim("name", firstName),
                new Claim("family_name", lastName),
                new Claim("flyingclubNumber", fcNumber),
                new Claim("tid", tidValue),
                new Claim("nbf", 1693409743.ToString()),
            };

            JwtSecurityToken token = new JwtSecurityToken(
                claims: customClaims
                );

            JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();

            string jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }


        public string GenerateLoginB2CTokenThroughMYAURL(string fcNumber, string password)
        {
            string endpoint = ConfigReader.Get("AzureB2CLoginToGenerateTheToken");

            webDriver = GetBrowser(BrowserType.Edge, endpoint);

            webDriver.TryFindElementAndClick("//button[@id='ensAcceptAll']", "Cookies Agree Button", 5);

            webDriver.FindElementAndEnterText("//input[@id='signInName']", fcNumber, "Enter FC Number from test case");

            webDriver.FindElementAndEnterText("//input[@id='password']", password, "Enter Password from test case");

            webDriver.FindElementAndClick("//button[@id='continue']", "Click on continue button");

            webDriver.WaitForElementToBeVisible("//div[@id='encodedToken']/span[@class='jwtHeader']", 5);

            string url = webDriver.Url;

            string token = url.Split("=")[1];

            webDriver.DriverCleanUp();

            return token;
        }

        public string GetValidFCNumber(out string customerId)
        {
            string queryForFCMDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.MemberType = @memberType and c.Status = @status and c.SSAMemberNumber = '' and c.HeadOfGroupMemberNumber = '' order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@memberType", "Individual").WithParameter("@status", "Active");

            IEnumerable<Core.DB_Model.Entities.FlyingClubMember> fcmDocs = cosmosFixture.CosmosQueryItems<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = fcmDocs.FirstOrDefault(x => !(x.RetainGoldCounter.Has(".") && x.RetainSilvercounter.Has(".")) && string.IsNullOrEmpty(x.TelephonePassword));

            customerId = fcmDoc?.CustomerId ?? "";

            return fcmDoc?.MemberNumber ?? "";
        }

        public static void TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode)
        {
            string[] titles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
            Random random = new();
            title = titles[random.Next(titles.Length)];
            firstName = DataGenerator.RandomFirstName();
            lastName = DataGenerator.RandomFirstName();
            emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";
            postCode = "RH11 9LG";
            country = "GB";
            channelId = null;
            enroledBy = Application.AutomationScriptCreatedBy;
            dateOfBirth = DateTime.UtcNow.AddDays(-6600).ToString("yyyy-MM-dd");
            enrollmentChannel = "WEB-MYA";
            mediaCode = "Web";
        }
    }
}
