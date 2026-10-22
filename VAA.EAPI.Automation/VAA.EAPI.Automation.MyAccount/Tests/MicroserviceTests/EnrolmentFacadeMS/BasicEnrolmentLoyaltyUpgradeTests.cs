using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Infrastructure.AzureStorage;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MyAccount.Tests.MicroserviceTests.EnrolmentFacadeMS
{
    [Collection("BasicEnrolmentLoyaltyUpgrade")]
    public class BasicEnrolmentLoyaltyUpgradeTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>, IClassFixture<TableStorageService<OtpRequestTableEntity>>
    {
        private readonly ITestOutputHelper output;
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        public static string env = Config.Environment;
        private readonly TableStorageService<OtpRequestTableEntity> otpRequestTableEntityTableStorageService;

        public BasicEnrolmentLoyaltyUpgradeTests(ITestOutputHelper output, TokenFixture tokenFixture, CosmosFixture cosmosFixture, TableStorageService<OtpRequestTableEntity> otpRequestTableEntityTableStorageService)
        {
            this.output = output;
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForDynamicsEnrolmentFromKeyVault("loyalty");

            string cosmosEndpoint = "https://avadeuncmdbcustomer.documents.azure.com:443/";
            string cosmosEndpointForCustomerTrx = "https://avadeuncmdbcustomertransactions.documents.azure.com:443/";

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerProfile>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<PartnerMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.otpRequestTableEntityTableStorageService = otpRequestTableEntityTableStorageService;
        }

        [Trait("Category", "BasicAccount-Regression")]
        [Fact]
        public async Task GenerationOfEnrolmentVRED()
        {
            string channelUserId = "";
            string channelId = "WEB";
            string channelIdUpgrade = "VRED";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            System.Net.Http.HttpResponseMessage enrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType, preferenceType, preferenceValue, usage, emailId, preferredFlag, description);
            dynamic enrolmentResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            //invoke enroment with the same email id and firstname, lastname as basic enrolment
            System.Net.Http.HttpResponseMessage enrolmentUpgradeResponse = Microservice.EnrolmentLoyaltyUpgrade(env, tokenFixture.GetToken("loyalty"), channelIdUpgrade, firstName, lastName, emailId);

            Assert.True(enrolmentUpgradeResponse.IsSuccessStatusCode, $"Success status code was not returned returned {enrolmentUpgradeResponse.StatusCode}");

            JObject enrolmentenrolmentUpgradeResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentUpgradeResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            Assert.True(enrolmentUpgradeResponse.StatusCode == System.Net.HttpStatusCode.Accepted, "202 Accepted was not returned from  enrolment");

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.EmailAddress = '" + emailId + "' and c.DocType = '" + Application.DocTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(2000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            string dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            Assert.True(enrolmentenrolmentUpgradeResult["customerId"].ToString().Equals(dbOPCustomerIdSearch), $"CustomerId - {enrolmentenrolmentUpgradeResult["customerId"].ToString()} did not equal customer id generated during basic account creation which is {dbOPCustomerIdSearch}");

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "' and c.CustomerId ='" + dbOPCustomerIdSearch + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objCustomer.EnrollmentChannel != Application.VRedChannelId)
            {
                Thread.Sleep(2000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 10 re tries");

            string customerChannel = objCustomer.EnrollmentChannel;
            Assert.True(customerChannel.Equals(Application.VRedChannelId), $"EnrolmentChannel - {customerChannel} should be {Application.VRedChannelId}");

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + Application.DocTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(2000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 10 re tries");

            string dbOPDocTypeFM = objFlyingClubMember.DocType;
            string dbOPCustomerIdFM = objFlyingClubMember.CustomerId;
            string dbOPTierCodeFM = objFlyingClubMember.TierCode;
            string flyingClubMemeberId = objFlyingClubMember.MemberNumber;
            Assert.True(enrolmentenrolmentUpgradeResult["memberdetails"]["memberNumber"].ToString().Equals(flyingClubMemeberId), $"Member Number - {flyingClubMemeberId} did not equal memeber number from enrolment ms response");
            Assert.True(dbOPDocTypeFM.Equals(Application.DocTypeFlyingClubMember), $"docType - {dbOPDocTypeFM} did not equal docType {Application.DocTypeFlyingClubMember}");
            Assert.True(dbOPCustomerIdFM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdFM} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeFM.Equals(Application.GlassTierCodeUpgrade), $"TierCode - {dbOPTierCodeFM} did not equal Tier code {Application.GlassTierCodeUpgrade}");
        }

        [Trait("Category", "BasicAccount-Regression")]
        [Fact]
        public async Task GenerationOfEnrolmentPowerapp()
        {
            string channelUserId = "";
            string channelId = "WEB";
            string channelIdUpgrade = "POWERAPP";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            System.Net.Http.HttpResponseMessage enrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);
            dynamic enrolmentResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            Dictionary<string, string> documentIds = GetDocumentIds(firstName);
            string contactIdBeforeUpgradingTheAccount = documentIds["ContactId"];
            string preferenceIdBeforeUpgradingTheAccount = documentIds["PreferenceId"];
            string cardIdBeforeUpgradingTheAccount = documentIds["CardId"];

            //invoke enroment with the same email id and firstname, lastname as basic enrolment
            System.Net.Http.HttpResponseMessage enrolmentUpgradeResponse = Microservice.EnrolmentLoyaltyUpgrade(env, tokenFixture.GetToken("loyalty"), channelIdUpgrade, firstName, lastName, emailId);

            Assert.True(enrolmentUpgradeResponse.IsSuccessStatusCode, $"Success status code was not returned returned {enrolmentUpgradeResponse.StatusCode}");

            Dictionary<string, string> documentIdsAfterUpgrade = GetDocumentIds(firstName);
            string contactIdAfterUpgradingTheAccount = documentIdsAfterUpgrade["ContactId"];
            string preferenceIdAfterUpgradingTheAccount = documentIdsAfterUpgrade["PreferenceId"];
            string cardIdAfterUpgradingTheAccount = documentIdsAfterUpgrade["CardId"];

            //Validation of the Unique document id after upgradation 
            Assert.True(contactIdBeforeUpgradingTheAccount.Equals(contactIdAfterUpgradingTheAccount), "Contact ID is not same after upgradation");

            Assert.True(preferenceIdBeforeUpgradingTheAccount.Equals(preferenceIdAfterUpgradingTheAccount), "Preference ID is not same after upgradation");

            Assert.True(cardIdBeforeUpgradingTheAccount.Equals(cardIdAfterUpgradingTheAccount), "Card ID is not same after upgradation");

            JObject enrolmentenrolmentUpgradeResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentUpgradeResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentUpgradeResponse.StatusCode == System.Net.HttpStatusCode.Accepted, "202 Accepted was not returned from  enrolment");

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.EmailAddress = '" + emailId + "' and c.DocType = '" + Application.DocTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(2000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            string dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            Assert.True(enrolmentenrolmentUpgradeResult["customerId"].ToString().Equals(dbOPCustomerIdSearch), $"CustomerId - {enrolmentenrolmentUpgradeResult["customerId"].ToString()} did not equal customer id generated during basic account creation which is {dbOPCustomerIdSearch}");

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "' and c.CustomerId ='" + dbOPCustomerIdSearch + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objCustomer.EnrollmentChannel != Application.PowerAppChannelId)
            {
                Thread.Sleep(2000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 10 re tries");

            string customerChannel = objCustomer.EnrollmentChannel;
            Assert.True(customerChannel.Equals(Application.PowerAppChannelId), $"EnrolmentChannel - {customerChannel} should be {Application.PowerAppChannelId}");

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + Application.DocTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(2000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 10 re tries");

            string dbOPDocTypeFM = objFlyingClubMember.DocType;
            string dbOPCustomerIdFM = objFlyingClubMember.CustomerId;
            string dbOPTierCodeFM = objFlyingClubMember.TierCode;
            string flyingClubMemeberId = objFlyingClubMember.MemberNumber;
            Assert.True(enrolmentenrolmentUpgradeResult["memberdetails"]["memberNumber"].ToString().Equals(flyingClubMemeberId), $"Member Number - {flyingClubMemeberId} did not equal memeber number from enrolment ms response");
            Assert.True(dbOPDocTypeFM.Equals(Application.DocTypeFlyingClubMember), $"docType - {dbOPDocTypeFM} did not equal docType {Application.DocTypeFlyingClubMember}");
            Assert.True(dbOPCustomerIdFM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdFM} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeFM.Equals(Application.GlassTierCodeUpgrade), $"TierCode - {dbOPTierCodeFM} did not equal Tier code {Application.GlassTierCodeUpgrade}");
        }

        private Dictionary<string, string> GetDocumentIds(string firstName)
        {
            Dictionary<string, string> documentIds = new();

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "'";

            Customer objCustomerSearch = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);

            int retry = 5;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(3000);
                objCustomerSearch = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 5 re tries");

            string contactId = objCustomerSearch.ContactDetails.FirstOrDefault().ContactId;

            documentIds.Add("ContactId", contactId);

            string customerId = objCustomerSearch.Id;

            string strInputQueryForCustomerProfile = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + customerId + "' and c.DocType = '" + Application.DocTypeCustomerProfile + "'";

            CustomerProfile objCustomerProfile = cosmosFixture.QueryItem<CustomerProfile>(strInputQueryForCustomerProfile);

            retry = 5;
            while (retry > 0 && objCustomerProfile == null)
            {
                Thread.Sleep(3000);
                objCustomerProfile = cosmosFixture.QueryItem<CustomerProfile>(strInputQueryForCustomerProfile);
                retry--;
            }

            Assert.True(objCustomerProfile != null, "Query from Cosmos DB for Customer Profile is null after 5 re tries");

            string preferenceId = objCustomerProfile.Preferences.FirstOrDefault().PreferenceId;

            documentIds.Add("PreferenceId", preferenceId);

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + customerId + "' and c.DocType = '" + Application.DocTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);

            retry = 5;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(3000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Flying Club member is null after 5 re tries");

            string cardId = objFlyingClubMember.MemberShipCardDetails.CardID;

            documentIds.Add("CardId", cardId);

            return documentIds;
        }

        [Trait("Category", "BasicAccount-Regression")]
        [Fact]
        public async Task GenerationOfEnrolmentSNAPP()
        {
            string channelUserId = "";
            string channelId = "WEB";
            string channelIdUpgrade = "SNAPP";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            System.Net.Http.HttpResponseMessage enrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);
            dynamic enrolmentResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            //invoke enroment with the same email id and firstname, lastname as basic enrolment
            System.Net.Http.HttpResponseMessage enrolmentUpgradeResponse = Microservice.EnrolmentLoyaltyUpgrade(env, tokenFixture.GetToken("loyalty"), channelIdUpgrade, firstName, lastName, emailId);

            Assert.True(enrolmentUpgradeResponse.IsSuccessStatusCode, $"Success status code was not returned returned {enrolmentUpgradeResponse.StatusCode}");

            JObject enrolmentenrolmentUpgradeResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentUpgradeResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            Assert.True(enrolmentUpgradeResponse.StatusCode == System.Net.HttpStatusCode.Accepted, "202 Accepted was not returned from  enrolment");

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.EmailAddress = '" + emailId + "' and c.DocType = '" + Application.DocTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(2000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            string dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            Assert.True(enrolmentenrolmentUpgradeResult["customerId"].ToString().Equals(dbOPCustomerIdSearch), $"CustomerId - {enrolmentenrolmentUpgradeResult["customerId"].ToString()} did not equal customer id generated during basic account creation which is {dbOPCustomerIdSearch}");

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "' and c.CustomerId ='" + dbOPCustomerIdSearch + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objCustomer.EnrollmentChannel != Application.SnappChannelId)
            {
                Thread.Sleep(2000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 10 re tries");

            string customerChannel = objCustomer.EnrollmentChannel;
            Assert.True(customerChannel.Equals(Application.SnappChannelId), $"EnrolmentChannel - {customerChannel} should be {Application.SnappChannelId}");

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + Application.DocTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(2000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 10 re tries");

            string dbOPDocTypeFM = objFlyingClubMember.DocType;
            string dbOPCustomerIdFM = objFlyingClubMember.CustomerId;
            string dbOPTierCodeFM = objFlyingClubMember.TierCode;
            string flyingClubMemeberId = objFlyingClubMember.MemberNumber;
            Assert.True(enrolmentenrolmentUpgradeResult["memberdetails"]["memberNumber"].ToString().Equals(flyingClubMemeberId), $"Member Number - {flyingClubMemeberId} did not equal memeber number from enrolment ms response");
            Assert.True(dbOPDocTypeFM.Equals(Application.DocTypeFlyingClubMember), $"docType - {dbOPDocTypeFM} did not equal docType {Application.DocTypeFlyingClubMember}");
            Assert.True(dbOPCustomerIdFM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdFM} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeFM.Equals(Application.GlassTierCodeUpgrade), $"TierCode - {dbOPTierCodeFM} did not equal Tier code {Application.GlassTierCodeUpgrade}");
        }

        [Trait("Category", "BasicAccount-Regression")]
        [Fact]
        public async Task GenerationOfEnrolmentDELTA()
        {
            string channelUserId = "";
            string channelId = "WEB";
            string channelIdUpgrade = "DELTA.COM";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            System.Net.Http.HttpResponseMessage enrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);
            dynamic enrolmentResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            //invoke enroment with the same email id and firstname, lastname as basic enrolment
            System.Net.Http.HttpResponseMessage enrolmentUpgradeResponse = Microservice.EnrolmentLoyaltyUpgrade(env, tokenFixture.GetToken("loyalty"), channelIdUpgrade, firstName, lastName, emailId);

            Assert.True(enrolmentUpgradeResponse.IsSuccessStatusCode, $"Success status code was not returned returned {enrolmentUpgradeResponse.StatusCode}");

            JObject enrolmentenrolmentUpgradeResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentUpgradeResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            Assert.True(enrolmentUpgradeResponse.StatusCode == System.Net.HttpStatusCode.Accepted, "202 Accepted was not returned from  enrolment");

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.EmailAddress = '" + emailId + "' and c.DocType = '" + Application.DocTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(2000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            string dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            Assert.True(enrolmentenrolmentUpgradeResult["customerId"].ToString().Equals(dbOPCustomerIdSearch), $"CustomerId - {enrolmentenrolmentUpgradeResult["customerId"].ToString()} did not equal customer id generated during basic account creation which is {dbOPCustomerIdSearch}");

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "' and c.CustomerId ='" + dbOPCustomerIdSearch + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objCustomer.EnrollmentChannel != Application.DeltaChannelId)
            {
                Thread.Sleep(2000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 10 re tries");

            string customerChannel = objCustomer.EnrollmentChannel;
            Assert.True(customerChannel.Equals(Application.DeltaChannelId), $"EnrolmentChannel - {customerChannel} should be {Application.DeltaChannelId}");

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + Application.DocTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(2000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 10 re tries");

            string dbOPDocTypeFM = objFlyingClubMember.DocType;
            string dbOPCustomerIdFM = objFlyingClubMember.CustomerId;
            string dbOPTierCodeFM = objFlyingClubMember.TierCode;
            string flyingClubMemeberId = objFlyingClubMember.MemberNumber;
            Assert.True(enrolmentenrolmentUpgradeResult["memberdetails"]["memberNumber"].ToString().Equals(flyingClubMemeberId), $"Member Number - {flyingClubMemeberId} did not equal memeber number from enrolment ms response");
            Assert.True(dbOPDocTypeFM.Equals(Application.DocTypeFlyingClubMember), $"docType - {dbOPDocTypeFM} did not equal docType {Application.DocTypeFlyingClubMember}");
            Assert.True(dbOPCustomerIdFM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdFM} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeFM.Equals(Application.GlassTierCodeUpgrade), $"TierCode - {dbOPTierCodeFM} did not equal Tier code {Application.GlassTierCodeUpgrade}");
        }

        [Trait("Category", "BasicAccount-Regression")]
        [Fact]
        public async Task GenerationOfEnrolmentAXIS()
        {
            string channelUserId = "";
            string channelId = "WEB";
            string channelIdUpgrade = "AXIS";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            System.Net.Http.HttpResponseMessage enrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);
            dynamic enrolmentResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            //invoke enroment with the same email id and firstname, lastname as basic enrolment
            System.Net.Http.HttpResponseMessage enrolmentUpgradeResponse = Microservice.EnrolmentLoyaltyUpgrade(env, tokenFixture.GetToken("loyalty"), channelIdUpgrade, firstName, lastName, emailId);

            Assert.True(enrolmentUpgradeResponse.IsSuccessStatusCode, $"Success status code was not returned returned {enrolmentUpgradeResponse.StatusCode}");

            JObject enrolmentenrolmentUpgradeResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentUpgradeResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            Assert.True(enrolmentUpgradeResponse.StatusCode == System.Net.HttpStatusCode.Accepted, "202 Accepted was not returned from  enrolment");

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.EmailAddress = '" + emailId + "' and c.DocType = '" + Application.DocTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(2000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            string dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            Assert.True(enrolmentenrolmentUpgradeResult["customerId"].ToString().Equals(dbOPCustomerIdSearch), $"CustomerId - {enrolmentenrolmentUpgradeResult["customerId"].ToString()} did not equal customer id generated during basic account creation which is {dbOPCustomerIdSearch}");

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "' and c.CustomerId ='" + dbOPCustomerIdSearch + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objCustomer.EnrollmentChannel != Application.AxisChannelId)
            {
                Thread.Sleep(2000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 10 re tries");

            string customerChannel = objCustomer.EnrollmentChannel;
            Assert.True(customerChannel.Equals(Application.AxisChannelId), $"EnrolmentChannel - {customerChannel} should be {Application.AxisChannelId}");

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + Application.DocTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(2000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 10 re tries");

            string dbOPDocTypeFM = objFlyingClubMember.DocType;
            string dbOPCustomerIdFM = objFlyingClubMember.CustomerId;
            string dbOPTierCodeFM = objFlyingClubMember.TierCode;
            string flyingClubMemeberId = objFlyingClubMember.MemberNumber;
            Assert.True(enrolmentenrolmentUpgradeResult["memberdetails"]["memberNumber"].ToString().Equals(flyingClubMemeberId), $"Member Number - {flyingClubMemeberId} did not equal memeber number from enrolment ms response");
            Assert.True(dbOPDocTypeFM.Equals(Application.DocTypeFlyingClubMember), $"docType - {dbOPDocTypeFM} did not equal docType {Application.DocTypeFlyingClubMember}");
            Assert.True(dbOPCustomerIdFM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdFM} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeFM.Equals(Application.GlassTierCodeUpgrade), $"TierCode - {dbOPTierCodeFM} did not equal Tier code {Application.GlassTierCodeUpgrade}");
        }

        [Trait("Category", "BasicAccount-Regression")]
        [Fact]
        public async Task GenerationOfEnrolmentPDM()
        {
            string channelUserId = "";
            string channelId = "WEB";
            string channelIdUpgrade = "PDM";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            System.Net.Http.HttpResponseMessage enrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);
            dynamic enrolmentResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            //invoke enroment with the same email id and firstname, lastname as basic enrolment
            System.Net.Http.HttpResponseMessage enrolmentUpgradeResponse = Microservice.EnrolmentLoyaltyUpgrade(env, tokenFixture.GetToken("loyalty"), channelIdUpgrade, firstName, lastName, emailId);

            Assert.True(enrolmentUpgradeResponse.IsSuccessStatusCode, $"Success status code was not returned returned {enrolmentUpgradeResponse.StatusCode}");

            JObject enrolmentenrolmentUpgradeResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentUpgradeResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            Assert.True(enrolmentUpgradeResponse.StatusCode == System.Net.HttpStatusCode.Accepted, "202 Accepted was not returned from  enrolment");

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.EmailAddress = '" + emailId + "' and c.DocType = '" + Application.DocTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(2000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            string dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            Assert.True(enrolmentenrolmentUpgradeResult["customerId"].ToString().Equals(dbOPCustomerIdSearch), $"CustomerId - {enrolmentenrolmentUpgradeResult["customerId"].ToString()} did not equal customer id generated during basic account creation which is {dbOPCustomerIdSearch}");

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "' and c.CustomerId ='" + dbOPCustomerIdSearch + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objCustomer.EnrollmentChannel != Application.PDMChannelId)
            {
                Thread.Sleep(2000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 10 re tries");

            string customerChannel = objCustomer.EnrollmentChannel;
            Assert.True(customerChannel.Equals(Application.PDMChannelId), $"EnrolmentChannel - {customerChannel} should be {Application.PDMChannelId}");

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + Application.DocTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(2000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 10 re tries");

            string dbOPDocTypeFM = objFlyingClubMember.DocType;
            string dbOPCustomerIdFM = objFlyingClubMember.CustomerId;
            string dbOPTierCodeFM = objFlyingClubMember.TierCode;
            string flyingClubMemeberId = objFlyingClubMember.MemberNumber;
            Assert.True(enrolmentenrolmentUpgradeResult["memberdetails"]["memberNumber"].ToString().Equals(flyingClubMemeberId), $"Member Number - {flyingClubMemeberId} did not equal memeber number from enrolment ms response");
            Assert.True(dbOPDocTypeFM.Equals(Application.DocTypeFlyingClubMember), $"docType - {dbOPDocTypeFM} did not equal docType {Application.DocTypeFlyingClubMember}");
            Assert.True(dbOPCustomerIdFM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdFM} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeFM.Equals(Application.GlassTierCodeUpgrade), $"TierCode - {dbOPTierCodeFM} did not equal Tier code {Application.GlassTierCodeUpgrade}");
        }

        [Trait("Category", "BasicAccount-Regression")]
        [Fact]
        public async Task GenerationOfEnrolmentRETAIL()
        {
            string channelUserId = "";
            string channelId = "WEB";
            string channelIdUpgrade = "RETAIL";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            System.Net.Http.HttpResponseMessage enrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);
            dynamic enrolmentResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            //invoke enroment with the same email id and firstname, lastname as basic enrolment
            System.Net.Http.HttpResponseMessage enrolmentUpgradeResponse = Microservice.EnrolmentLoyaltyUpgrade(env, tokenFixture.GetToken("loyalty"), channelIdUpgrade, firstName, lastName, emailId);

            Assert.True(enrolmentUpgradeResponse.IsSuccessStatusCode, $"Success status code was not returned returned {enrolmentUpgradeResponse.StatusCode}");

            JObject enrolmentenrolmentUpgradeResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentUpgradeResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            Assert.True(enrolmentUpgradeResponse.StatusCode == System.Net.HttpStatusCode.Accepted, "202 Accepted was not returned from  enrolment");

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.EmailAddress = '" + emailId + "' and c.DocType = '" + Application.DocTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(2000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            string dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            Assert.True(enrolmentenrolmentUpgradeResult["customerId"].ToString().Equals(dbOPCustomerIdSearch), $"CustomerId - {enrolmentenrolmentUpgradeResult["customerId"].ToString()} did not equal customer id generated during basic account creation which is {dbOPCustomerIdSearch}");

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "' and c.CustomerId ='" + dbOPCustomerIdSearch + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objCustomer.EnrollmentChannel != Application.RetailChannelId)
            {
                Thread.Sleep(2000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 10 re tries");

            string customerChannel = objCustomer.EnrollmentChannel;
            Assert.True(customerChannel.Equals(Application.RetailChannelId), $"EnrolmentChannel - {customerChannel} should be {Application.RetailChannelId}");

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + Application.DocTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(2000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 10 re tries");

            string dbOPDocTypeFM = objFlyingClubMember.DocType;
            string dbOPCustomerIdFM = objFlyingClubMember.CustomerId;
            string dbOPTierCodeFM = objFlyingClubMember.TierCode;
            string flyingClubMemeberId = objFlyingClubMember.MemberNumber;
            Assert.True(enrolmentenrolmentUpgradeResult["memberdetails"]["memberNumber"].ToString().Equals(flyingClubMemeberId), $"Member Number - {flyingClubMemeberId} did not equal memeber number from enrolment ms response");
            Assert.True(dbOPDocTypeFM.Equals(Application.DocTypeFlyingClubMember), $"docType - {dbOPDocTypeFM} did not equal docType {Application.DocTypeFlyingClubMember}");
            Assert.True(dbOPCustomerIdFM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdFM} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeFM.Equals(Application.GlassTierCodeUpgrade), $"TierCode - {dbOPTierCodeFM} did not equal Tier code {Application.GlassTierCodeUpgrade}");
        }

        [Trait("Category", "BasicAccount-Regression")]
        [Fact]
        public async Task BasicAccountDedupeCheck()
        {
            string channelUserId = "";
            string channelId = "WEB";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            System.Net.Http.HttpResponseMessage enrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);

            dynamic enrolmentResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            int retry = 10;
            while (retry > 0 && objCustomer == null)
            {
                Thread.Sleep(2000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 10 re tries");

            System.Net.Http.HttpResponseMessage enrolmentDedupeResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);

            dynamic enrolmentDedupeResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentDedupeResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentDedupeResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            string customerDocCountQuery = $"SELECT c.CustomerId FROM c where c.ContactDetails[0].ContactValue = '{emailId}'";

            IEnumerable<Customer> customerDocs = cosmosFixture.QueryItems<Customer>(customerDocCountQuery);

            Assert.True(customerDocs.Count() == 1, $"Expected customer doc count should be 1 but found {customerDocs.Count()}");
        }

        [Theory]
        [Trait("Category", "BasicAccount-Regression")]
        [InlineData("POWERAPP")]
        [InlineData("DELTA.COM")]
        public async Task ValidateErrorMessageInBasicEnrolmentAfterLoyaltyEnrolment(string enrolmentChannelId)
        {
            string channelUserId = "";
            string basicChannelId = "WEB";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            //invoke enroment with the same email id and firstname, lastname as basic enrolment
            System.Net.Http.HttpResponseMessage enrolmentResponse = Microservice.EnrolmentLoyaltyUpgrade(env, tokenFixture.GetToken("loyalty"), enrolmentChannelId, firstName, lastName, emailId);

            Assert.True(enrolmentResponse.IsSuccessStatusCode, $"Success status code was not returned returned {enrolmentResponse.StatusCode}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResponse.StatusCode == System.Net.HttpStatusCode.Accepted, "202 Accepted was not returned from  enrolment");

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.EmailAddress = '" + emailId + "' and c.DocType = '" + Application.DocTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(2000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            string dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            Assert.True(enrolmentResult["customerId"].ToString().Equals(dbOPCustomerIdSearch), $"CustomerId - {enrolmentResult["customerId"].ToString()} did not equal customer id generated during basic account creation which is {dbOPCustomerIdSearch}");

            System.Net.Http.HttpResponseMessage basicEnrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, basicChannelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);

            Assert.True(basicEnrolmentResponse.StatusCode == System.Net.HttpStatusCode.OK, "200 OK was not returned from basic enrolment GraphQL");

            dynamic basicEnrolmentResult = JsonConvert.DeserializeObject<dynamic>(await basicEnrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(basicEnrolmentResult["errors"][0]["extensions"]["response"].ToString().Contains("1011"), $"Error code is wrong expected 1011, actual {basicEnrolmentResult["errors"][0]["extensions"]["response"]}");
            Assert.True(basicEnrolmentResult["errors"][0]["extensions"]["response"].ToString().Contains("An account with that email address already exists"), $"Error message is different actual {basicEnrolmentResult["errors"][0]["extensions"]["response"]}");
        }

        [Trait("Category", "BasicAccount-Regression")]
        [Fact]
        public async Task GenerationOfEnrolmentWebMyaAsync()
        {
            string channelUserId = "";
            string channelId = "WEB";
            string channelIdUpgrade = "WEB-MYA";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string customerType = "Basic";
            string preferenceType = "VaaMarketingPermissions";
            string preferenceValue = "Y";
            string usage = "Personal";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string preferredFlag = "Y";
            string description = null;

            System.Net.Http.HttpResponseMessage enrolmentResponse = GraphQL.BasicAccountEnrolment(env, channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description);
            dynamic enrolmentResult = JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            // Entra enrolment upgrades the basic account with the existing loyalty enrolment-channel mapping.
            EntraLoyaltyAccount enrolledAccount = await EnrolmentHelper.CreateEntraLoyaltyAccountAsync(channelIdUpgrade, "App", firstName, lastName, emailId, DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-dd"), "MYA-APP");

            Assert.True(enrolmentResult.data.loyaltyEnrolCustomer == "Ok", "Ok was not returned from basic enrolment graphql");

            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.EmailAddress = '" + emailId + "' and c.DocType = '" + Application.DocTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(2000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            string dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            string enrolmentUpgradeCustomerId = enrolledAccount.CustomerId;
            Assert.True(enrolmentUpgradeCustomerId.Equals(dbOPCustomerIdSearch), $"CustomerId - {enrolmentUpgradeCustomerId} did not equal customer id generated during basic account creation which is {dbOPCustomerIdSearch}");

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType = '" + Application.DocTypeCustomer + "' and c.CustomerId ='" + dbOPCustomerIdSearch + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objCustomer.EnrollmentChannel != Application.MyAccountChannelId)
            {
                Thread.Sleep(2000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 10 re tries");

            string customerChannel = objCustomer.EnrollmentChannel;
            Assert.True(customerChannel.Equals(Application.MyAccountChannelId), $"EnrolmentChannel - {customerChannel} should be {Application.MyAccountChannelId}");

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + Application.DocTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(2000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 10 re tries");

            string dbOPDocTypeFM = objFlyingClubMember.DocType;
            string dbOPCustomerIdFM = objFlyingClubMember.CustomerId;
            string dbOPTierCodeFM = objFlyingClubMember.TierCode;
            string flyingClubMemeberId = objFlyingClubMember.MemberNumber;
            Assert.True(enrolledAccount.MemberNumber.Equals(flyingClubMemeberId), $"Member Number - {flyingClubMemeberId} did not equal member number from Entra enrolment response");
            Assert.True(dbOPDocTypeFM.Equals(Application.DocTypeFlyingClubMember), $"docType - {dbOPDocTypeFM} did not equal docType {Application.DocTypeFlyingClubMember}");
            Assert.True(dbOPCustomerIdFM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdFM} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeFM.Equals(Application.GlassTierCodeUpgrade), $"TierCode - {dbOPTierCodeFM} did not equal Tier code {Application.GlassTierCodeUpgrade}");
        }
    }
}
