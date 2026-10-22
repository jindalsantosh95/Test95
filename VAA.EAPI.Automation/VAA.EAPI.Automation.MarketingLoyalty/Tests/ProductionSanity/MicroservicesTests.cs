using Microsoft.Azure.Cosmos;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.ProductionSanity
{
    [Collection("ProductionSanityTests")]
    public class MicroservicesTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {

        private readonly CosmosFixture _cosmosFixture;

        private readonly TokenFixture _tokenFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosDb;

        private static string ocpSubKey = string.Empty;

        public MicroservicesTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            _cosmosFixture = cosmosFixture;

            _tokenFixture = tokenFixture;

            customerCosmosDb = Application.CustomerCosmosDbProd;
            customerTransactionCosmosDb = Application.CustomerTransactionCosmosDbProd;
            auditCosmosDb = Application.AuditCosmosDbProd;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpointProd;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<ContactNote>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<CustomerProfile>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<TargetedMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<Group>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpointProd;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<AddressHistory>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<ContactHistory>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpointProd;

            _cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            _cosmosFixture.RegisterType<AwardAudit>(auditCosmosEndpoint, auditCosmosDb, Application.AuditCosmosCollection);

            _cosmosFixture.RegisterType<CustomerProfileAudit>(auditCosmosEndpoint, auditCosmosDb, Application.AuditCosmosCollection);

            _cosmosFixture.RegisterType<PreferenceAudit>(auditCosmosEndpoint, auditCosmosDb, Application.AuditCosmosCollection);

            _cosmosFixture.RegisterType<OtherAudit>(auditCosmosEndpoint, auditCosmosDb, Application.AuditCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("Testing-Ocp-Apim-SubscriptionKey") ?? "";

            _tokenFixture.AddTokenForAuditMicroserviceFromKeyVault("AuditMS");

            _tokenFixture.AddTokenForCampaignMicroserviceFromKeyVault("CampaignMS");

            _tokenFixture.AddTokenForCustomerMSFromKeyVault("CustomerMS");

            _tokenFixture.AddTokenForCustomerTxnFromKeyVault("CustTxnMS");

            _tokenFixture.AddTokenForLoyaltyMemberMicroserviceFromKeyVault("LoyaltyMbrMS");

            _tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMs");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveAwardAuditHistory()
        {
            string queryForAwardAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit");

            AwardAudit awardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            Assert.True(awardAuditDoc is not null, "Failed to retrieve an Award Audit Doc from Cosmos DB");

            HttpResponseMessage response = Microservice.RetrieveAwardAuditHistory(_tokenFixture.GetToken("AuditMS"), ocpSubKey, awardAuditDoc.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(awardAuditDoc.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveCustomerProfileAuditHistory()
        {
            string queryForCustProfileDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForCustProfileDoc).WithParameter("@docType", "CustomerProfileAudit");

            CustomerProfileAudit customerProfAuditDoc = _cosmosFixture.CosmosQueryItem<CustomerProfileAudit>(query);

            Assert.True(customerProfAuditDoc is not null, "Failed to retrieve a Cust Profile Audit Doc from Cosmos DB");

            HttpResponseMessage response = Microservice.RetrieveCustomerProfileAuditHistory(_tokenFixture.GetToken("AuditMS"), ocpSubKey, customerProfAuditDoc.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(customerProfAuditDoc.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrievePreferenceAuditHistory()
        {
            string queryForPreferenceAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForPreferenceAuditDoc).WithParameter("@docType", "PreferenceAudit");

            PreferenceAudit preferenceAuditDoc = _cosmosFixture.CosmosQueryItem<PreferenceAudit>(query);

            Assert.True(preferenceAuditDoc is not null, "Failed to retrieve a Preference Audit Doc from Cosmos DB");

            HttpResponseMessage response = Microservice.RetrievePreferenceAuditHistory(_tokenFixture.GetToken("AuditMS"), ocpSubKey, preferenceAuditDoc.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(preferenceAuditDoc.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveSupplementarySilverAuditHistory()
        {
            string queryForSuppSilverAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.AuditCategory = @auditCategory order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForSuppSilverAuditDoc).WithParameter("@docType", "OtherAudit").WithParameter("@auditCategory", "Supplementary Silver");

            OtherAudit suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(query);

            Assert.True(suppSilverAuditDoc is not null, "Failed to retrieve a Supp Silver Audit Doc from Cosmos DB");

            HttpResponseMessage response = Microservice.RetrieveSupplementarySilverAccountAuditHistory(_tokenFixture.GetToken("AuditMS"), ocpSubKey, suppSilverAuditDoc.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(suppSilverAuditDoc.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task CampaignsListDownload()
        {
            HttpResponseMessage response = Microservice.CampaignsListDownload(_tokenFixture.GetToken("CampaignMS"), ocpSubKey).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task PromotionsListDownload()
        {
            HttpResponseMessage response = Microservice.PromotionsListDownload(_tokenFixture.GetToken("CampaignMS"), ocpSubKey).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task CampaignTypeRetrieval()
        {
            HttpResponseMessage response = Microservice.CampaignTypesRetrieval(_tokenFixture.GetToken("CampaignMS"), ocpSubKey).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task MemberCampaignRetrieval()
        {
            string queryForTargetedMembershipDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE  c.DocType = 'TargetedMembership' order by c._ts desc";

            TargetedMembership targetedMembership = _cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

            Assert.True(targetedMembership is not null, "Failed to retrieve a Targeted Membership Doc from Cosmos DB");

            string customerId = targetedMembership.CustomerId;

            HttpResponseMessage response = Microservice.MemberCampaignListRetrieval(_tokenFixture.GetToken("CampaignMS"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(customerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task PromotionSearch()
        {
            HttpResponseMessage response = Microservice.PromotionSearch(_tokenFixture.GetToken("CampaignMS"), ocpSubKey).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task CampaignPromotionSearch()
        {
            string strQueryForPromotionId = "Select TOP 1 PromotionId from dbo.CampaignsPromotionsMapping order by NEWID()";

            string promotionId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForPromotionId);

            HttpResponseMessage response = Microservice.CampaignPromotionSearch(_tokenFixture.GetToken("CampaignMS"), ocpSubKey, promotionId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveCampaignDetails()
        {
            string queryForCampaignSearch = "SELECT TOP 1 CampaignCode from [dbo].[Campaigns] order by NEWID()";

            string campCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            HttpResponseMessage response = Microservice.RetrieveCampaignDetails(_tokenFixture.GetToken("CampaignMS"), ocpSubKey, campCode).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(campCode), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task CampaignSearch()
        {
            HttpResponseMessage response = Microservice.CampaignSearch(_tokenFixture.GetToken("CampaignMS"), ocpSubKey).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            //Assert.True(!result.Has("Error", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveAddressAuditHistory()
        {
            string queryForAddressHistoryDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForAddressHistoryDoc).WithParameter("@docType", "AddressHistory");

            AddressHistory addressHistoryDoc = _cosmosFixture.CosmosQueryItem<AddressHistory>(query);

            Assert.True(addressHistoryDoc is not null, "Failed to retrieve an AddressHistory doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveAddressHistory(_tokenFixture.GetToken("CustomerMS"), ocpSubKey, addressHistoryDoc.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(addressHistoryDoc.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveContactAuditHistory()
        {
            string queryForContactHistoryDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForContactHistoryDoc).WithParameter("@docType", "ContactHistory");

            ContactHistory contactHistoryDoc = _cosmosFixture.CosmosQueryItem<ContactHistory>(query);

            Assert.True(contactHistoryDoc is not null, "Failed to retrieve an Contact History doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveContactHistory(_tokenFixture.GetToken("CustomerMS"), ocpSubKey, contactHistoryDoc.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(contactHistoryDoc.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveFlyingClubMemberDocument()
        {
            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember flyingClubMember = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(flyingClubMember is not null, "Failed to retrieve a Flying Club Member doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveFlyingClubMemberDoc(_tokenFixture.GetToken("CustTxnMS"), ocpSubKey, flyingClubMember.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(flyingClubMember.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveMembershipChangeDocument()
        {
            string queryForMembershipChangeDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForMembershipChangeDoc).WithParameter("@docType", "MembershipChange").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            MembershipChange membershipChange = _cosmosFixture.CosmosQueryItem<MembershipChange>(parameterizedQuery);

            Assert.True(membershipChange is not null, "Failed to retrieve a MembershipChange  doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveMembershipChangeDoc(_tokenFixture.GetToken("CustTxnMS"), ocpSubKey, membershipChange.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(membershipChange.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveTargetedMembershipDocument()
        {
            string queryForTargetedMembershipDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType  order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTargetedMembershipDoc).WithParameter("@docType", "TargetedMembership");

            TargetedMembership targetedMembership = _cosmosFixture.CosmosQueryItem<TargetedMembership>(parameterizedQuery);

            Assert.True(targetedMembership is not null, "Failed to retrieve a TargetedMembership doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveTargetedMembershipDoc(_tokenFixture.GetToken("CustTxnMS"), ocpSubKey, targetedMembership.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(targetedMembership.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveTransactionDocument()
        {
            string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType  order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction");

            Transaction transaction = _cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            Assert.True(transaction is not null, "Failed to retrieve a Transaction doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveTransactionDoc(_tokenFixture.GetToken("CustTxnMS"), ocpSubKey, transaction.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            //Assert.True(!result.Has("Error", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(transaction.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveGroupDocumentByCustomerId()
        {
            string queryForGroupDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForGroupDoc).WithParameter("@docType", "Group");

            Group group = _cosmosFixture.CosmosQueryItem<Group>(parameterizedQuery);

            Assert.True(group is not null, "Failed to retrieve a Group doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveGroupDocByCustomerId(_tokenFixture.GetToken("CustTxnMS"), ocpSubKey, group.HeadOfGroupCustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(group.HeadOfGroupCustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveGroupDocumentByGroupId()
        {
            string queryForGroupDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForGroupDoc).WithParameter("@docType", "Group");

            Group group = _cosmosFixture.CosmosQueryItem<Group>(parameterizedQuery);

            Assert.True(group is not null, "Failed to retrieve a Group doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveGroupDocByGroupId(_tokenFixture.GetToken("CustTxnMS"), ocpSubKey, group.GroupId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(group.GroupId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveGroupHistoryByCustomerId()
        {
            string queryForGroupDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForGroupDoc).WithParameter("@docType", "Group");

            Group group = _cosmosFixture.CosmosQueryItem<Group>(parameterizedQuery);

            Assert.True(group is not null, "Failed to retrieve a Group doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveGroupHistoryByCustomerId(_tokenFixture.GetToken("CustTxnMS"), ocpSubKey, group.GroupHistory.First().CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(group.HeadOfGroupCustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveContactNotesByCustomerId()
        {
            string queryForNotesDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForNotesDoc).WithParameter("@docType", "ContactNote");

            ContactNote contactNoteDoc = _cosmosFixture.CosmosQueryItem<ContactNote>(parameterizedQuery);

            Assert.True(contactNoteDoc is not null, "Failed to retrieve a contactNoteDoc  from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveContactNotesByCustomerId(_tokenFixture.GetToken("CustTxnMS"), ocpSubKey, contactNoteDoc.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(contactNoteDoc.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveHouseholdMemberDetails()
        {
            string queryForGroupDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForGroupDoc).WithParameter("@docType", "Group");

            Group group = _cosmosFixture.CosmosQueryItem<Group>(parameterizedQuery);

            Assert.True(group is not null, "Failed to retrieve a Group doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(_tokenFixture.GetToken("LoyaltyMbrMS"), ocpSubKey, group.HeadOfGroupCustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrievePartnerTypes()
        {

            HttpResponseMessage response = Microservice.RetrievePartnerTypes(_tokenFixture.GetToken("LoyaltyMbrMS"), ocpSubKey).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveMemberAccoutDetails()
        {
            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction");

            Transaction txnDoc = _cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            Assert.True(txnDoc is not null, "Failed to retrieve a Txn doc from cosmos Db");

            HttpResponseMessage response = Microservice.RetrieveMemberAccountDetails(_tokenFixture.GetToken("LoyaltyMbrMS"), ocpSubKey, txnDoc.CustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");

            Assert.True(result.Has(txnDoc.CustomerId), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveListOfMembersEnrolledToAPromotion()
        {
            string strQueryForPromotionId = "Select TOP 1 PromotionId from dbo.CampaignsPromotionsMapping order by NEWID()";

            string promotionId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForPromotionId);

            HttpResponseMessage response = Microservice.RetrieveListOfPromotionEnrolledMembers(_tokenFixture.GetToken("LoyaltyMbrMS"), ocpSubKey, promotionId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveSuppSilverMemberDetails()
        {
            string queryForChildFCMDocument = "SELECT TOP 10 * FROM CustomerDetailsCollection c where c.DocType = @docType and  c.SSAMemberNumber <>'' order by c._ts desc";

            QueryDefinition query = new QueryDefinition(query: queryForChildFCMDocument).WithParameter("@docType", "FlyingClubMember");

            IEnumerable<FlyingClubMember> childFlyingClubMemberDoc = _cosmosFixture.CosmosQueryItems<FlyingClubMember>(query);

            Assert.True(childFlyingClubMemberDoc.Any(),
                "The Test Data did not meet the test case requirement .Unable to find Supp Silver Members.");

            string parentCustomerId = string.Empty;
            string parentMemberNumber = string.Empty;

            FlyingClubMember parentFlyingClubMemberDoc = null;

            foreach (FlyingClubMember childFcmDoc in childFlyingClubMemberDoc)
            {

                string queryForParentFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.MemberNumber=@ssaMemberNumber";

                QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForParentFCMDocument).WithParameter("@ssaMemberNumber", childFcmDoc.SSAMemberNumber);

                parentFlyingClubMemberDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                if (parentFlyingClubMemberDoc is not null)
                {
                    if (parentFlyingClubMemberDoc.TierCode.Equals("Gold"))
                    {
                        parentMemberNumber = parentFlyingClubMemberDoc.MemberNumber;

                        parentCustomerId = parentFlyingClubMemberDoc.CustomerId;

                        break;
                    }
                }

            }

            Assert.True(!string.IsNullOrEmpty(parentCustomerId) || !string.IsNullOrEmpty(parentMemberNumber),
                $"The Test Data did not meet the test case requirement .The Parent Member Number (SSAMemberNumber) associated to a child account is not a Gold Tier Member.");

            HttpResponseMessage response = Microservice.RetrieveSupplementarySilverMemberDetails(_tokenFixture.GetToken("LoyaltyMbrMS"), ocpSubKey, parentCustomerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveMemberPointBalance()
        {
            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction");

            Transaction txnDoc = _cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            Assert.True(txnDoc is not null, "Failed to retrieve a Txn doc from cosmos Db");

            HttpResponseMessage response = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMs"), ocpSubKey, txnDoc.MemberNumber).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionMicroservicesSmokeRegression)]
        [Fact]
        public async Task RetrieveMemberTransactionStatement()
        {
            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction");

            Transaction txnDoc = _cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            Assert.True(txnDoc is not null, "Failed to retrieve a Txn doc from cosmos Db");

            HttpResponseMessage response = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMs"), ocpSubKey, txnDoc.MemberNumber).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Has("Error"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(!result.Has("Failed"), $"Response was not as expected . Response returned is \n {result}");
            Assert.True(result.Has(txnDoc.MemberNumber), $"Response was not as expected . Response returned is \n {result}");
        }
    }
}
