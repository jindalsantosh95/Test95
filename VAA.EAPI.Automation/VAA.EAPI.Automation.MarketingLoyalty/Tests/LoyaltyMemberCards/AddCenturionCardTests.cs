using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoyaltyMemberCards
{
    [Collection("LoyaltyMemberCardLinkTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class AddCenturionCardTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        public AddCenturionCardTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;
            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<PartnerMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerProfileAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);
            this.cosmosFixture.RegisterType<CardLinkingAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory, RunOrder(2)]
        [InlineData("AMXCUK")]
        [InlineData("AMXIC1")]
        public async Task TC01_AddAmexCenturionCardToFCMember_SuccessScenario(string cardType)
        {

            #region PreRequisite to retrieve member with no AMEX card linked to the account 

            string queryForFlyingClubmemberDoc = "SELECT TOP 10 * FROM CustomerTransaction c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.DowngradeProhibitFlag = @flag order by c.CreatedOn desc";

            QueryDefinition query = new QueryDefinition(query: queryForFlyingClubmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@flag", "N");

            IEnumerable<FlyingClubMember> flyingClubDocuments = cosmosFixture.CosmosQueryItems<FlyingClubMember>(query);

            Assert.True(flyingClubDocuments.Any(), "No flying club documents found. Cannot proceed with test execution");

            string customerId = string.Empty;

            string memberNumber = string.Empty;

            string tierCode = string.Empty;

            bool flag = false;

            foreach (FlyingClubMember flyingClubMemberDoc in flyingClubDocuments)
            {
                customerId = flyingClubMemberDoc.CustomerId;

                memberNumber = flyingClubMemberDoc.MemberNumber;

                tierCode = flyingClubMemberDoc.TierCode;

                string queryForAllPartnerMembershipDoc = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.PartnerId = @partnerId and c.PartnerStatusCode = @partnerStatusCode order by c.CreatedOn desc ";

                QueryDefinition queryForAllDocs = new QueryDefinition(query: queryForAllPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@customerId", customerId).WithParameter("@partnerId", "1-2LHC").WithParameter("@partnerStatusCode", cardType);

                IEnumerable<PartnerMembership> partnerMembershipDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(queryForAllDocs);

                if (!partnerMembershipDocs.Any())
                {
                    flag = true;
                    break;
                }

            }

            Assert.True(flag, $"The test Data does not meet the test case requirement. Currently we are not able to fetch any test member with no associated AMEX centurion card aassociated with them.");

            #endregion

            #region Add Card GraphQl API Call and Validations

            string partnerCode = "ACENT";

            string partnerId = "1-2LHC";

            DateTime currentDate = DateTime.UtcNow.Date;

            string startDate = DataGenerator.RandomPastDate(10).ToString("yyyy-MM-dd");

            HttpResponseMessage response = GraphQL.CreatePartnerMembership(tokenFixture.GetToken("graphqlv2"), cardType, customerId, memberNumber, partnerCode, partnerId, startDate).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"), $"Expected a valid response for Create PartnerMembership Document operation but found {result.ToString()}");

            Assert.True(result.data.loyaltyCreatePartnerMembership.id.ToString() is not null, $"Expected a valid partner membership id but found {result.ToString()} ");

            Assert.True(result.data.loyaltyCreatePartnerMembership.partnerId.ToString().Equals(partnerId), $"Expected the partner ID to be {partnerId} in the  partner membership document but found {result.data.loyaltyCreatePartnerMembership.partnerId.ToString()}");

            Assert.True(result.data.loyaltyCreatePartnerMembership.partnerStatusCode.ToString().Equals(cardType), $"Expected the partnerStatusCode to be {cardType} in the  partner membership document but found {result.data.loyaltyCreatePartnerMembership.partnerStatusCode.ToString()}");

            string startDateInResponse = result.data.loyaltyCreatePartnerMembership.startDate is not null ? DateTime.Parse(result.data.loyaltyCreatePartnerMembership.startDate.ToString()).ToString("yyyy-MM-dd") : "";

            Assert.True(startDateInResponse.Equals(startDate), $"Expected the start Date / Linked date of the card  to be {startDate} in the  partner membership document but found {startDateInResponse}");

            #endregion

            #region PartnerMembership Doc Validation

            string queryForPartnerMembershipDoc = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.PartnerId = @partnerId and c.PartnerStatusCode = @partnerStatusCode order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@customerId", customerId).WithParameter("@partnerId", "1-2LHC").WithParameter("@partnerStatusCode", cardType);

            IEnumerable<PartnerMembership> partnerMembershipCosmosDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(paramQuery);

            int retry = 5;

            while (!partnerMembershipCosmosDocs.Any() && retry > 0)
            {
                await Task.Delay(1000);

                partnerMembershipCosmosDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(paramQuery);

                retry--;
            }

            Assert.True(partnerMembershipCosmosDocs.Count() == 1, $"Expected only one PartnerMembership doc for the customer {customerId} for the card type {cardType} but found {partnerMembershipCosmosDocs.Count()}");

            PartnerMembership partnerMembership = partnerMembershipCosmosDocs.First();

            Assert.True(partnerMembership.PartnerId == partnerId, $"Expected the PartnerId to be {partnerId} but found {partnerMembership.PartnerId}");

            Assert.True(partnerMembership.PartnerStatusCode == cardType, $"Expected the Partner Status Code to be {cardType} but found {partnerMembership.PartnerStatusCode}");

            Assert.True(partnerMembership.PartnerCode == partnerCode, $"Expected the Partner Code to be {partnerCode} but found {partnerMembership.PartnerCode}");

            string actualStartDate = partnerMembership.StartDate.HasValue ? partnerMembership.StartDate.Value.ToString("yyyy-MM-dd") : "";

            Assert.True(actualStartDate == startDate, $"Expected the Card StartDate to be {startDate} but found {actualStartDate}");

            string actualEndDate = partnerMembership.EndDate.HasValue ? partnerMembership.EndDate.Value.ToString("yyyy-MM-dd") : "";

            Assert.True(actualEndDate.IsNullOrEmpty(), $"Expected the Card EndDate to be Null/Empty but found {actualEndDate}");

            Assert.True(partnerMembership.MemberNumber == memberNumber, $"Expected the Member Number to be {memberNumber} but found {partnerMembership.MemberNumber}");

            Assert.True(partnerMembership.CreatedBy == "AutomationScript@test.com", $"Expected the CreatedBy to be AutomationScript@test.com but found {partnerMembership.CreatedBy}");

            Assert.True(partnerMembership.LastUpdatedBy == "AutomationScript@test.com", $"Expected the LastUpdatedBy to be AutomationScript@test.com but found {partnerMembership.LastUpdatedBy}");

            #endregion

            #region Card Linking Audit Doc Validation

            string queryForCardLinkingAuditDoc = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.StatusEnhancementCode = @cardType and c.CreatedBy = @createdBy order by c.CreatedOn desc ";

            QueryDefinition cardLinkingAuditDocQuery = new QueryDefinition(query: queryForCardLinkingAuditDoc).WithParameter("@docType", "CardLinkingAudit").WithParameter("@customerId", customerId).WithParameter("@cardType", cardType).WithParameter("@createdBy", "AutomationScript@test.com");

            CardLinkingAudit cardLinkingAuditDoc = cosmosFixture.CosmosQueryItem<CardLinkingAudit>(cardLinkingAuditDocQuery);

            retry = 5;

            while (cardLinkingAuditDoc is null && retry > 0)
            {
                await Task.Delay(2000);

                cardLinkingAuditDoc = cosmosFixture.CosmosQueryItem<CardLinkingAudit>(cardLinkingAuditDocQuery);

                retry--;
            }

            Assert.True(cardLinkingAuditDoc is not null, $"After the Card Link of Card Type {cardType} ,Failed to Retrieve Card Linking Audit Document after retry of 10 seconds for the customer {customerId}");

            Assert.True(cardLinkingAuditDoc.PartnerCode == partnerCode, $"In the Card Linking Audit Doc ,Expected the PartnerCode to be {partnerCode} but found {cardLinkingAuditDoc.PartnerCode}");

            Assert.True(cardLinkingAuditDoc.StatusEnhancementCode == cardType, $"In the Card Linking Audit Doc ,Expected the StatusEnhancementCode to be {cardType} but found {cardLinkingAuditDoc.StatusEnhancementCode}");

            string actualCardStartDate = cardLinkingAuditDoc.StartDate.HasValue ? cardLinkingAuditDoc.StartDate.Value.ToString("yyyy-MM-dd") : "";

            Assert.True(actualCardStartDate == startDate, $"In the Card Linking Audit Doc ,Expected the StartDate to be {startDate} but found {actualCardStartDate}");

            string actualCardEndDate = cardLinkingAuditDoc.EndDate.HasValue ? cardLinkingAuditDoc.EndDate.Value.ToString("yyyy-MM-dd") : "";

            Assert.True(actualCardEndDate.IsNullOrEmpty(), $"In the Card Linking Audit Doc ,Expected the Card EndDate to be Null/Empty but found {actualCardEndDate}");

            Assert.True(cardLinkingAuditDoc.CardLinkingUpdatedBy == "AutomationScript@test.com", $"In the Card Linking Audit Doc ,Expected the UpdatedBy to be AutomationScript@test.com but found {cardLinkingAuditDoc.CardLinkingUpdatedBy}");

            Assert.True(cardLinkingAuditDoc.CreatedOn.Value.ToString("yyyy-MM-dd") == currentDate.Date.ToString("yyyy-MM-dd"), $"In the Card Linking Audit Doc ,Expected the CreatedOn  Date to be {DateTime.UtcNow.Date.ToString("yyyy-MM-dd")} but found {cardLinkingAuditDoc.CreatedOn.Value.ToString("yyyy-MM-dd")}");

            Assert.True(cardLinkingAuditDoc.CreatedOn.Value.ToString("yyyy-MM-dd") == cardLinkingAuditDoc.CardLinkingUpdatedOn.Value.ToString("yyyy-MM-dd"), $"In the Card Linking Audit Doc ,Expected the CreatedOn and LastUpdatedOn Date to be same but found different ");

            #endregion

            #region Flying Club Member Doc Validation

            queryForFlyingClubmemberDoc = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.DowngradeProhibitFlag = @flag ";

            query = new QueryDefinition(query: queryForFlyingClubmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId).WithParameter("@flag", "Y");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

            retry = 5;

            while (fcmDoc is null && retry > 0)
            {
                await Task.Delay(2000);

                fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

                retry--;
            }

            Assert.True(fcmDoc is not null, $"Failed to retrieve the FCM Document for the Customer {customerId}");

            Assert.True(fcmDoc.DowngradeProhibitFlag == "Y", $"For the Customerd {customerId} Expected the Downgrade Prohibit Flag value in FCM Doc to be Y after addition of new Card but found {fcmDoc.DowngradeProhibitFlag}");

            #endregion

            #region Customer Profile Audit Doc Validation

            string customerProfileUpdatedBy = "LOOPS";

            string queryForCustProfAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.CreatedBy = @createdBy order by c.CreatedOn desc ";

            QueryDefinition custProfAuditDocQuery = new QueryDefinition(query: queryForCustProfAuditDocs).WithParameter("@docType", "CustomerProfileAudit").WithParameter("@customerId", customerId).WithParameter("@createdBy", customerProfileUpdatedBy);

            CustomerProfileAudit custProfAuditCosmosDoc = cosmosFixture.CosmosQueryItem<CustomerProfileAudit>(custProfAuditDocQuery);

            retry = 5;

            while (custProfAuditCosmosDoc is null && retry > 0)
            {
                await Task.Delay(2000);

                custProfAuditCosmosDoc = cosmosFixture.CosmosQueryItem<CustomerProfileAudit>(custProfAuditDocQuery);

                retry--;
            }

            Assert.True(custProfAuditCosmosDoc is not null, $"After the Card Link of Card Type {cardType} ,Failed to Retrieve Customer Profile Audit Document after retry of 10 seconds for the customer {customerId}");

            // Getting the Previous Audit Document inorder to Validate the data fields with that of New Audit Document
            //Expecting all other fields to be same except DPFlag value , createdBy/On and UpdatedBy/On values in new Audit Document
            string queryForCustProfPrevAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition custProfPrevAuditDocQuery = new QueryDefinition(query: queryForCustProfPrevAuditDocs).WithParameter("@docType", "CustomerProfileAudit").WithParameter("@customerId", customerId);

            CustomerProfileAudit custProfPrevAuditCosmosDoc = cosmosFixture.CosmosQueryItems<CustomerProfileAudit>(custProfPrevAuditDocQuery).OrderByDescending(x => x.CreatedOn).Skip(1).First(); //First doc will be latest and the second one will be old so skip 1st and take the second

            SoftAssertHelper softAssert = new();

            if (custProfPrevAuditCosmosDoc is not null)
            {

                softAssert.True(custProfPrevAuditCosmosDoc.FirstName == custProfAuditCosmosDoc.FirstName,
                   $"For the Customer {customerId} , In the new Audit Doc, Expected the Customer Profile Doc FirstName to be {custProfPrevAuditCosmosDoc.FirstName} but found {custProfAuditCosmosDoc.FirstName} ");

                softAssert.True(custProfPrevAuditCosmosDoc.LastName == custProfAuditCosmosDoc.LastName,
                  $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc LastName to be {custProfPrevAuditCosmosDoc.LastName} but found {custProfAuditCosmosDoc.LastName} ");

                softAssert.True(custProfPrevAuditCosmosDoc.Title == custProfAuditCosmosDoc.Title,
                  $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Title to be {custProfPrevAuditCosmosDoc.Title} but found {custProfAuditCosmosDoc.Title} ");

                softAssert.True(custProfPrevAuditCosmosDoc.Gender == custProfAuditCosmosDoc.Gender,
                  $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Gender to be {custProfPrevAuditCosmosDoc.Gender} but found {custProfAuditCosmosDoc.Gender} ");

                softAssert.True(custProfPrevAuditCosmosDoc.Status == custProfAuditCosmosDoc.Status,
                 $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Status to be {custProfPrevAuditCosmosDoc.Status} but found {custProfAuditCosmosDoc.Status} ");

                custProfPrevAuditCosmosDoc.DoB = custProfPrevAuditCosmosDoc.DoB == DateTime.MinValue ? null : custProfPrevAuditCosmosDoc.DoB;

                softAssert.True(custProfPrevAuditCosmosDoc.DoB == custProfAuditCosmosDoc.DoB,
                 $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Date Of Birth to be {custProfPrevAuditCosmosDoc.DoB} but found {custProfAuditCosmosDoc.DoB} ");

                softAssert.True(custProfPrevAuditCosmosDoc.NameChangeReason == custProfAuditCosmosDoc.NameChangeReason,
                 $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Name Change Reason to be {custProfPrevAuditCosmosDoc.NameChangeReason} but found {custProfAuditCosmosDoc.NameChangeReason} ");

            }

            softAssert.True(custProfAuditCosmosDoc.CustomerProfileUpdatedBy == customerProfileUpdatedBy, $"For the Customer {customerId} ,In the New Customer Profile Audit Doc ,Expected the UpdatedBy to be {customerProfileUpdatedBy}  but found {custProfAuditCosmosDoc.CustomerProfileUpdatedBy}");

            softAssert.True(custProfAuditCosmosDoc.CreatedOn.ToString("yyyy-MM-dd") == currentDate.ToString("yyyy-MM-dd"), $"For the Customer {customerId} ,In the New Customer Profile Audit Doc ,Expected the CreatedOn  Date to be {currentDate.ToString("yyyy-MM-dd")} but found {custProfAuditCosmosDoc.CreatedOn.ToString("yyyy-MM-dd")}");

            softAssert.True(custProfAuditCosmosDoc.CustomerProfileUpdatedOn.ToString("yyyy-MM-dd") == custProfAuditCosmosDoc.CustomerProfileUpdatedOn.ToString("yyyy-MM-dd"), "For the Customer {customerId} ,In the New Customer Profile Audit Doc ,Expected the CreatedOn and LastUpdatedOn Date to be same but found different ");

            softAssert.True(custProfAuditCosmosDoc.DowngradeProhibitFlag == "Y", $"For the Customer {customerId} ,In the New Customer Profile Audit Doc, Expected the DowngradeProhibitFlag in Customer Profile Audit Doc to be Y but found {custProfAuditCosmosDoc.DowngradeProhibitFlag} ");

            softAssert.AssertAll();

            #endregion

            string expectedNewTierStartDate = currentDate.AddDays(+1).Date.ToString("yyyy-MM-dd");

            string queryForMembershipChangeDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.Active = @activeFlag and c.TierCode = @tierCode and c.StartDate like @startDate and c.CreatedBy = @createdBy order by c.LastUpdatedOn desc";

            QueryDefinition membershipChangeQuery = new QueryDefinition(query: queryForMembershipChangeDoc).WithParameter("@docType", "MembershipChange").WithParameter("@customerId", customerId).WithParameter("@activeFlag", "Y").WithParameter("@tierCode", "Gold").WithParameter("@startDate", $"{expectedNewTierStartDate}%").WithParameter("@createdBy", "LOOPS");

            MembershipChange membershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(membershipChangeQuery);

            retry = 10;

            while (membershipChangeDoc == null && retry > 0)
            {
                await Task.Delay(1000);

                membershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(membershipChangeQuery);

                retry--;
            }

            if (tierCode.IsAny("Red", "Silver"))
            {

                Assert.True(membershipChangeDoc is not null, $"Failed to retrieve the new membershipchange doc for new Gold Tier as Member current tierCode is  {tierCode} for the customerId {customerId}");

                Assert.True(membershipChangeDoc.StartDate.Value.ToString("yyyy-MM-dd") == currentDate.AddDays(+1).Date.ToString("yyyy-MM-dd"), $"Expected the Active Doc StartDate to be {DateTime.UtcNow.AddDays(+1).Date.ToString("yyyy-MM-dd")} but found {membershipChangeDoc.StartDate.Value.ToString("yyyy-MM-dd")}.");

                string expectedTierEndDate = currentDate.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date.ToString("yyyy-MM-dd");

                Assert.True(membershipChangeDoc.EndDate.Value.ToString("yyyy-MM-dd") == expectedTierEndDate, $"Expected the Active Doc EndDate to be {expectedTierEndDate} but found {membershipChangeDoc.EndDate.Value.ToString("yyyy-MM-dd")}.");

                fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

                Assert.True(fcmDoc is not null, $"Failed to retrieve the FCM Document for the Customer {customerId}");

                Assert.True(fcmDoc.TierCode == "Gold", $"Expected the TierCode in FCM Doc to be Gold but found {fcmDoc.TierCode}");

                Assert.True(fcmDoc.CenturianFlag == "Y", $"Expected the CenturionFlag in FCM Doc to be Y but found {fcmDoc.CenturianFlag}");
            }

            else
            {
                Assert.True(membershipChangeDoc is null, $"Expected No New membershipchange doc as Member current tierCode is already {tierCode} for the customerId {customerId}");

                fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

                Assert.True(fcmDoc is not null, $"Failed to retrieve the FCM Document for the Customer {customerId}");

                Assert.True(fcmDoc.CenturianFlag == "Y", $"Expected the CenturionFlag in FCM Doc to be Y but found {fcmDoc.CenturianFlag}");
            }

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory, RunOrder(3)]
        [InlineData("AMXCUK")]
        [InlineData("AMXIC1")]
        public async Task TC02_DuplicateCheckAmexCenturionMemberCardValidationScenario(string cardType)
        {

            string centurionPartnerId = "1-2LHC";

            string centurionPartnerCode = "ACENT";

            string startDate = DataGenerator.RandomPastDate(10).ToString("yyyy-MM-dd");

            #region PreRequisite to retrieve member with an AMEX card linked to a FC account 

            string queryForPartnerMembershipDoc = "SELECT TOP 1 * FROM CustomerTransaction c where c.DocType = @docType and c.PartnerId = @partnerId and c.PartnerStatusCode = @partnerStatusCode and NOT IS_DEFINED(c.EndDate) order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@partnerId", centurionPartnerId).WithParameter("@partnerStatusCode", cardType);

            PartnerMembership partnerMembershipDoc = cosmosFixture.CosmosQueryItem<PartnerMembership>(paramQuery);

            string customerId = partnerMembershipDoc?.CustomerId ?? "";

            string memberNumber = partnerMembershipDoc?.MemberNumber ?? "";

            #endregion

            if (partnerMembershipDoc is null)
            {
                #region PreRequisite to retrieve member with no AMEX card linked to the account 

                string queryForFlyingClubmemberDoc = "SELECT TOP 10 * FROM CustomerTransaction c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.DowngradeProhibitFlag = @flag order by c.CreatedOn desc";

                QueryDefinition query = new QueryDefinition(query: queryForFlyingClubmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@flag", "N");

                IEnumerable<FlyingClubMember> flyingClubDocuments = cosmosFixture.CosmosQueryItems<FlyingClubMember>(query);

                Assert.True(flyingClubDocuments.Any(), "No flying club documents found. Cannot proceed with test execution");

                bool flag = false;

                foreach (FlyingClubMember flyingClubMemberDoc in flyingClubDocuments)
                {
                    customerId = flyingClubMemberDoc.CustomerId;

                    memberNumber = flyingClubMemberDoc.MemberNumber;

                    string queryForAllPartnerMembershipDoc = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.PartnerId = @partnerId and c.PartnerStatusCode = @partnerStatusCode order by c.CreatedOn desc ";

                    QueryDefinition queryForAllDocs = new QueryDefinition(query: queryForAllPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@customerId", customerId).WithParameter("@partnerId", "1-2LHC").WithParameter("@partnerStatusCode", cardType);

                    IEnumerable<PartnerMembership> partnerMembershipDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(queryForAllDocs);

                    if (!partnerMembershipDocs.Any())
                    {
                        flag = true;
                        break;
                    }

                }

                Assert.True(flag, $"The test Data does not meet the test case requirement. Currently we are not able to fetch any test member with no associated AMEX centurion card aassociated with them.");

                #endregion

                #region API Call

                HttpResponseMessage response = GraphQL.CreatePartnerMembership(tokenFixture.GetToken("graphqlv2"), cardType, customerId, memberNumber, centurionPartnerCode, centurionPartnerId, startDate).EnsureIsSuccessStatusCode();

                dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                Assert.True(!result.ToString().Contains("Request failed"), $"Expected a valid response for Create PartnerMembership Document operation but found {result.ToString()}");

                #endregion

            }

            #region Trying to add a duplicate Amex Card or second card to the same member account

            HttpResponseMessage updatedResponse = GraphQL.CreatePartnerMembership(tokenFixture.GetToken("graphqlv2"), cardType, customerId, memberNumber, centurionPartnerCode, centurionPartnerId, startDate).EnsureIsSuccessStatusCode();

            dynamic updatedResult = JsonConvert.DeserializeObject<dynamic>(await updatedResponse.Content.ReadAsStringAsync());

            Assert.True(updatedResult.ToString().Contains("Partner Membership mutation request failed"), $"Expected Request failed response while adding  a duplicate or second Amex centurion card for the same member but found {updatedResult.ToString()}");

            Assert.True(updatedResult.errors[0]?.extensions?.code?.ToString() == "G031", $"Expected a valid response code as 422 - unprocessible entity but found {updatedResult.ToString()} ");

            Assert.True(updatedResult.errors[0]?.extensions?.message?.ToString().Contains("A member can have only one active card of a particular type at a time"), $"Expected a valid response that A member can have only one active card of a particular type at a time but found {updatedResult.ToString()} ");

            #endregion

        }
    }
}
