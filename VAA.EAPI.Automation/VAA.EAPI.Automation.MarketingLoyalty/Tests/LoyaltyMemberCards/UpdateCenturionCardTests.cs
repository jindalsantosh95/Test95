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
    public class UpdateCenturionCardTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        public UpdateCenturionCardTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
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
        [Fact, RunOrder(4)]
        public async Task TC01_UpdateAmexCenturionCardOfaFCMember_SuccessScenario()
        {
            string centurionPartnerId = "1-2LHC";

            string centurionPartnerCode = "ACENT";

            string cardTypeUK = "AMXCUK";

            string cardTypeUS = "AMXIC1";

            string cardType = string.Empty;

            DateTime currentDate = DateTime.Now.Date;

            string startDate = DataGenerator.RandomPastDate(10).ToString("yyyy-MM-dd");

            #region PreRequisite to retrieve member with an AMEX card linked to a FC account 

            string queryForPartnerMembershipDocs = "SELECT TOP 1 * FROM CustomerTransaction c where c.DocType = @docType and c.PartnerId = @partnerId  and NOT IS_DEFINED(c.EndDate) order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForPartnerMembershipDocs).WithParameter("@docType", "PartnerMembership").WithParameter("@partnerId", centurionPartnerId);

            IEnumerable<PartnerMembership> partnerMembershipDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(paramQuery);

            PartnerMembership partnerMembershipDocUK = partnerMembershipDocs.FirstOrDefault(x => x.PartnerStatusCode == cardTypeUK);

            PartnerMembership partnerMembershipDocUS = partnerMembershipDocs.FirstOrDefault(x => x.PartnerStatusCode == cardTypeUS);

            bool hasMoreThanOneActiveCard = false;

            PartnerMembership partnerMembershipDoc = null;

            if (partnerMembershipDocUK is not null && partnerMembershipDocUS is not null)
            {
                hasMoreThanOneActiveCard = true;

                partnerMembershipDoc = partnerMembershipDocs.FirstOrDefault(x => x.PartnerStatusCode.IsAny(cardTypeUK, cardTypeUS));

                cardType = partnerMembershipDoc.PartnerStatusCode;
            }
            else if (partnerMembershipDocUK is not null)
            {
                partnerMembershipDoc = partnerMembershipDocUK;

                cardType = cardTypeUK;
            }
            else if (partnerMembershipDocUS is not null)
            {
                partnerMembershipDoc = partnerMembershipDocUS;

                cardType = cardTypeUS;
            }
            else
            {
                partnerMembershipDoc = null;
            }

            string customerId = partnerMembershipDoc?.CustomerId ?? "";

            string memberNumber = partnerMembershipDoc?.MemberNumber ?? "";

            #endregion

            HttpResponseMessage response = new HttpResponseMessage();

            dynamic result = null;

            if (partnerMembershipDoc is null)
            {

                #region PreRequisite to retrieve member with no AMEX card linked to the account 

                string queryForFlyingClubmemberDoc = "SELECT TOP 20 * FROM CustomerTransaction c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.DowngradeProhibitFlag = @flag order by c.CreatedOn desc";

                QueryDefinition query = new QueryDefinition(query: queryForFlyingClubmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@flag", "N");

                IEnumerable<FlyingClubMember> flyingClubDocuments = cosmosFixture.CosmosQueryItems<FlyingClubMember>(query);

                Assert.True(flyingClubDocuments.Any(), "No flying club documents found. Cannot proceed with test execution");

                bool flag = false;

                foreach (FlyingClubMember flyingClubMemberDoc in flyingClubDocuments)
                {
                    customerId = flyingClubMemberDoc.CustomerId;

                    memberNumber = flyingClubMemberDoc.MemberNumber;

                    string queryForAllPartnerMembershipDocs = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.PartnerId = @partnerId and c.PartnerStatusCode in ('AMXCUK','AMXIC1') order by c.CreatedOn desc ";

                    QueryDefinition queryForAllDocs = new QueryDefinition(query: queryForAllPartnerMembershipDocs).WithParameter("@docType", "PartnerMembership").WithParameter("@customerId", customerId).WithParameter("@partnerId", "1-2LHC").WithParameter("@partnerStatusCode", cardType);

                    IEnumerable<PartnerMembership> allPartnerMembershipDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(queryForAllDocs);

                    if (!allPartnerMembershipDocs.Any())
                    {
                        flag = true;
                        break;
                    }

                }

                Assert.True(flag, $"The test Data does not meet the test case requirement. Currently we are not able to fetch any test member with no associated AMEX centurion card associated with them.");

                #endregion

                #region Add Card GraphQl API Call and Validations

                response = GraphQL.CreatePartnerMembership(tokenFixture.GetToken("graphqlv2"), cardType, customerId, memberNumber, centurionPartnerCode, centurionPartnerId, startDate).EnsureIsSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                Assert.True(!result.ToString().Contains("Request failed"), $"Expected a valid response for Create PartnerMembership Document operation but found {result.ToString()}");

                Assert.True(result.data.loyaltyCreatePartnerMembership.id.ToString() is not null, $"Expected a valid partner membership id but found {result.ToString()} ");

                Assert.True(result.data.loyaltyCreatePartnerMembership.partnerId.ToString().Equals(centurionPartnerId), $"Expected the partner ID to be {centurionPartnerId} in the  partner membership document but found {result.data.loyaltyCreatePartnerMembership.partnerId.ToString()}");

                Assert.True(result.data.loyaltyCreatePartnerMembership.partnerStatusCode.ToString().Equals(cardType), $"Expected the partnerStatusCode to be {cardType} in the  partner membership document but found {result.data.loyaltyCreatePartnerMembership.partnerStatusCode.ToString()}");

                string startDateInResponse = DateTime.Parse(result.data.loyaltyCreatePartnerMembership.startDate.ToString()).ToString("yyyy-MM-dd");

                Assert.True(startDateInResponse.Equals(startDate), $"Expected the start Date / Linked date of the card  to be {startDate} in the  partner membership document but found {startDateInResponse}");

                #endregion

                #region Flying Club Member Doc Validation after adding card

                string queryForFlyingClubmember = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.DowngradeProhibitFlag = @flag order by c.CreatedOn desc";

                QueryDefinition queryAfterCardAdd = new QueryDefinition(query: queryForFlyingClubmember).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId).WithParameter("@flag", "Y");

                FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(queryAfterCardAdd);

                int retryCounter = 5;

                while (fcmDoc is null && retryCounter > 0)
                {
                    await Task.Delay(2000);

                    fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(queryAfterCardAdd);

                    retryCounter--;
                }

                Assert.True(fcmDoc is not null, $"Failed to retrieve the FCM Document for the Customer {customerId}");

                Assert.True(fcmDoc.DowngradeProhibitFlag == "Y", $"For the Customerd {customerId} Expected the Downgrade Prohibit Flag value in FCM Doc to be N after updating the end date of the Card but found {fcmDoc.DowngradeProhibitFlag}");

                #endregion

                #region PartnerMembership Doc Validation

                string queryForPartnerMembershipDoc = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.PartnerId = @partnerId and c.PartnerStatusCode = @partnerStatusCode order by c.CreatedOn desc ";

                paramQuery = new QueryDefinition(query: queryForPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@customerId", customerId).WithParameter("@partnerId", centurionPartnerId).WithParameter("@partnerStatusCode", cardType);

                IEnumerable<PartnerMembership> partnerMembershipCosmosDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(paramQuery);

                retryCounter = 5;

                while (!partnerMembershipCosmosDocs.Any() && retryCounter > 0)
                {
                    await Task.Delay(1000);

                    partnerMembershipCosmosDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(paramQuery);

                    retryCounter--;
                }

                Assert.True(partnerMembershipCosmosDocs.Count() == 1, $"Expected only one PartnerMembership doc for the customer {customerId} for the card type {cardType} but found {partnerMembershipCosmosDocs.Count()}");

                PartnerMembership partnerMembership = partnerMembershipCosmosDocs.First();

                Assert.True(partnerMembership.PartnerId == centurionPartnerId, $"Expected the PartnerId to be {centurionPartnerId} but found {partnerMembership.PartnerId}");

                Assert.True(partnerMembership.PartnerStatusCode == cardType, $"Expected the Partner Status Code to be {cardType} but found {partnerMembership.PartnerStatusCode}");

                Assert.True(partnerMembership.PartnerCode == centurionPartnerCode, $"Expected the Partner Code to be {centurionPartnerCode} but found {partnerMembership.PartnerCode}");

                string actualStartDate = partnerMembership.StartDate.HasValue ? partnerMembership.StartDate.Value.ToString("yyyy-MM-dd") : "";

                Assert.True(actualStartDate == startDate, $"Expected the Card StartDate to be {startDate} but found {actualStartDate}");

                string actualEndDate = partnerMembership.EndDate.HasValue ? partnerMembership.EndDate.Value.ToString("yyyy-MM-dd") : "";

                Assert.True(actualEndDate.IsNullOrEmpty(), $"Expected the Card EndDate to be Null/Empty but found {actualEndDate}");

                Assert.True(partnerMembership.MemberNumber == memberNumber, $"Expected the Member Number to be {memberNumber} but found {partnerMembership.MemberNumber}");

                Assert.True(partnerMembership.CreatedBy == "AutomationScript@test.com", $"Expected the CreatedBy to be AutomationScript@test.com but found {partnerMembership.CreatedBy}");

                Assert.True(partnerMembership.LastUpdatedBy == "AutomationScript@test.com", $"Expected the LastUpdatedBy to be AutomationScript@test.com but found {partnerMembership.LastUpdatedBy}");

                #endregion

            }

            #region Update PartnerMembership Document for End date 

            startDate = partnerMembershipDoc is not null ? partnerMembershipDoc?.StartDate.Value.ToString("yyyy-MM-dd") : startDate;

            string endDate = currentDate.ToString("yyyy-MM-dd");

            string updatedBy = "AutomationScript@test.com";

            string docId = partnerMembershipDoc is not null ? partnerMembershipDoc?.id : result?.data?.loyaltyCreatePartnerMembership?.id?.ToString();

            Assert.True(!docId.IsNullOrEmpty(), $"Failed to fetch the PartnerMembership Document Id for the Customer {customerId} to Update the EndDate. Cannot Proceed furthur");

            HttpResponseMessage responseForUpdateCard = GraphQL.UpdatePartnerMembership(tokenFixture.GetToken("graphqlv2"), cardType, customerId, memberNumber, centurionPartnerCode, centurionPartnerId, startDate, endDate, docId, updatedBy).EnsureIsSuccessStatusCode();

            dynamic resultForUpdatedCard = JsonConvert.DeserializeObject<dynamic>(await responseForUpdateCard.Content.ReadAsStringAsync());

            Assert.True(!resultForUpdatedCard.ToString().Contains("Request failed"), $"Expected a valid response for Update PartnerMembership Document operation but found {resultForUpdatedCard.ToString()}");

            Assert.True(resultForUpdatedCard.data?.loyaltyUpdatePartnerMembership?.id.ToString().Equals(docId), $"Expected a valid partner membership id {docId} but found {resultForUpdatedCard.ToString()} ");

            Assert.True(resultForUpdatedCard.data?.loyaltyUpdatePartnerMembership?.partnerId.ToString().Equals(centurionPartnerId), $"Expected the partner ID to be {centurionPartnerId} in the partner membership document but found {resultForUpdatedCard.ToString()}");

            string startDateInUpdateResponse = DateTime.Parse(resultForUpdatedCard?.data?.loyaltyUpdatePartnerMembership?.startDate?.ToString()).ToString("yyyy-MM-dd") ?? "";

            Assert.True(startDateInUpdateResponse.Equals(startDate), $"Expected the start Date / Linked date of the card  to be {startDate} in the partner membership document but found {startDateInUpdateResponse}");

            string endDateInUpdateResponse = DateTime.Parse(resultForUpdatedCard?.data?.loyaltyUpdatePartnerMembership?.endDate?.ToString()).ToString("yyyy-MM-dd") ?? "";

            Assert.True(endDateInUpdateResponse.Equals(endDate), $"Expected the start Date / Linked date of the card  to be {endDate} in the  partner membership document but found {endDateInUpdateResponse}");

            #endregion

            #region PartnerMembership Doc Validation after updating the endDate

            string queryForPartnerMembershipDocAfterUpdate = "SELECT TOP 1 * FROM CustomerTransaction c where c.DocType = @docType and c.id = @docId and c.EndDate <> @endDate ";

            QueryDefinition queryAfterUpdate = new QueryDefinition(query: queryForPartnerMembershipDocAfterUpdate).WithParameter("@docType", "PartnerMembership").WithParameter("@docId", docId).WithParameter("@endDate", "");

            PartnerMembership partnerMembershipDocAfterUpdate = cosmosFixture.CosmosQueryItem<PartnerMembership>(queryAfterUpdate);

            int retry = 5;

            while (partnerMembershipDocAfterUpdate is null && retry > 0)
            {
                await Task.Delay(1000);

                partnerMembershipDocAfterUpdate = cosmosFixture.CosmosQueryItem<PartnerMembership>(queryAfterUpdate);

                retry--;
            }

            Assert.True(partnerMembershipDocAfterUpdate is not null, $"Failed to Retrieve the PartnerMembershipDocument for the Doc Id {docId} after the Updating the EndDate");

            Assert.True(partnerMembershipDocAfterUpdate.EndDate.Value.ToString("yyyy-MM-dd") == endDate, $"After Updating the Card EndDate of type {cardType} for the customer {customerId}, the EndDate in the cosmos doc does not match.Expected the endDate to be {endDate} but found {partnerMembershipDocAfterUpdate.EndDate.Value.ToString("yyyy-MM-dd")}");

            #endregion

            #region Card Linking Audit Doc Validation After Update

            string queryForCardLinkingAuditDocWithEndDate = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.StatusEnhancementCode = @cardType and c.EndDate <> @endDate and c.CreatedBy = @createdBy order by c.CreatedOn desc ";

            QueryDefinition cardLinkingAuditDocQueryWithEndDate = new QueryDefinition(query: queryForCardLinkingAuditDocWithEndDate).WithParameter("@docType", "CardLinkingAudit").WithParameter("@customerId", customerId).WithParameter("@cardType", cardType).WithParameter("@endDate", "").WithParameter("@createdBy", updatedBy);

            CardLinkingAudit cardLinkingAuditDocWithEndDate = cosmosFixture.CosmosQueryItem<CardLinkingAudit>(cardLinkingAuditDocQueryWithEndDate);

            retry = 5;

            while (cardLinkingAuditDocWithEndDate is null && retry > 0)
            {
                await Task.Delay(2000);

                cardLinkingAuditDocWithEndDate = cosmosFixture.CosmosQueryItem<CardLinkingAudit>(cardLinkingAuditDocQueryWithEndDate);

                retry--;
            }

            Assert.True(cardLinkingAuditDocWithEndDate is not null, $"After the Card Link of Card Type {cardType} ,Failed to Retrieve Card Linking Audit Document after retry of 10 seconds for the customer {customerId}");

            Assert.True(cardLinkingAuditDocWithEndDate.PartnerCode == centurionPartnerCode, $"In the Card Linking Audit Doc ,Expected the PartnerCode to be {centurionPartnerCode} but found {cardLinkingAuditDocWithEndDate.PartnerCode}");

            Assert.True(cardLinkingAuditDocWithEndDate.StatusEnhancementCode == cardType, $"In the Card Linking Audit Doc ,Expected the StatusEnhancementCode to be {cardType} but found {cardLinkingAuditDocWithEndDate.StatusEnhancementCode}");

            string actualCardEndDate = cardLinkingAuditDocWithEndDate.EndDate.HasValue ? cardLinkingAuditDocWithEndDate.EndDate.Value.ToString("yyyy-MM-dd") : "";

            Assert.True(actualCardEndDate == endDate, $"In the Card Linking Audit Doc ,Expected the end date to be {endDate} but found {actualCardEndDate}");

            Assert.True(cardLinkingAuditDocWithEndDate.CardLinkingUpdatedBy == updatedBy, $"In the Card Linking Audit Doc ,Expected the UpdatedBy to be {updatedBy} but found {cardLinkingAuditDocWithEndDate.CardLinkingUpdatedBy}");

            Assert.True(cardLinkingAuditDocWithEndDate.CreatedOn.Value.ToString("yyyy-MM-dd") == currentDate.ToString("yyyy-MM-dd"), $"In the Card Linking Audit Doc ,Expected the CreatedOn  Date to be {currentDate.ToString("yyyy-MM-dd")} but found {cardLinkingAuditDocWithEndDate.CreatedOn.Value.ToString("yyyy-MM-dd")}");

            Assert.True(cardLinkingAuditDocWithEndDate.CreatedOn.Value.ToString("yyyy-MM-dd") == cardLinkingAuditDocWithEndDate.CardLinkingUpdatedOn.Value.ToString("yyyy-MM-dd"), $"In the Card Linking Audit Doc ,Expected the CreatedOn and LastUpdatedOn Date to be same but found different ");

            #endregion

            #region Flying Club Member Doc Validation

            string queryForFlyingClubmemberDocUpdated = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.DowngradeProhibitFlag = @flag ";

            QueryDefinition fcmQueryAfterUpdate = new QueryDefinition(query: queryForFlyingClubmemberDocUpdated).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId).WithParameter("@flag", "N");

            FlyingClubMember fcmDocAfterUpdate = cosmosFixture.CosmosQueryItem<FlyingClubMember>(fcmQueryAfterUpdate);

            retry = 5;

            while (fcmDocAfterUpdate is null && retry > 0)
            {
                await Task.Delay(2000);

                fcmDocAfterUpdate = cosmosFixture.CosmosQueryItem<FlyingClubMember>(queryAfterUpdate);

                retry--;
            }

            if (hasMoreThanOneActiveCard)
            {
                Assert.True(fcmDocAfterUpdate is null, $"After updating the EndDate to the Card Type {cardType} , Expected the FCM Doc Query to return Null as the customer {customerId} has two active Cards.");
            }
            else
            {
                Assert.True(fcmDocAfterUpdate is not null, $"Failed to retrieve the FCM Document for the Customer {customerId} after the EndDate update to the Card Type {cardType}");

                Assert.True(fcmDocAfterUpdate.DowngradeProhibitFlag == "N", $"For the Customerd {customerId} Expected the Downgrade Prohibit Flag value in FCM Doc to be N after updating the end date of the Card {cardType} but found {fcmDocAfterUpdate.DowngradeProhibitFlag}");

                Assert.True(fcmDocAfterUpdate.CenturianFlag == "N", $"For the Customerd {customerId} Expected the Centurion Flag value in FCM Doc to be N after updating the end date of the Card {cardType} but found {fcmDocAfterUpdate.CenturianFlag}");

            }

            #endregion       

            #region Customer Profile Audit Doc Validation

            string queryForCustProfAuditDocAfterUpdate = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.DowngradeProhibitFlag = @dFlag and c.CreatedBy = @createdBy order by c.CreatedOn desc ";

            QueryDefinition custProfAuditDocQueryAfterUpdate = new QueryDefinition(query: queryForCustProfAuditDocAfterUpdate).WithParameter("@docType", "CustomerProfileAudit").WithParameter("@customerId", customerId).WithParameter("@dFlag", "N").WithParameter("@createdBy", updatedBy);

            CustomerProfileAudit custProfAuditCosmosDocAfterUpdate = cosmosFixture.CosmosQueryItem<CustomerProfileAudit>(custProfAuditDocQueryAfterUpdate);

            retry = 5;

            while (custProfAuditCosmosDocAfterUpdate is null && retry > 0)
            {
                await Task.Delay(4000);

                custProfAuditCosmosDocAfterUpdate = cosmosFixture.CosmosQueryItem<CustomerProfileAudit>(custProfAuditDocQueryAfterUpdate);

                retry--;
            }

            if (hasMoreThanOneActiveCard)
            {
                Assert.True(custProfAuditCosmosDocAfterUpdate is null, $"After updating the EndDate to the Card Type {{cardType}} , Expected the CustomerProfileAudit Doc Query to return Null as the customer {customerId} has two active Cards.");
            }

            else
            {

                Assert.True(custProfAuditCosmosDocAfterUpdate is not null, $"After the Card Link of Card Type {cardType} ,Failed to Retrieve Customer Profile Audit Document after retry of 10 seconds for the customer {customerId}");

                string queryForCustProfPrevAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

                QueryDefinition custProfPrevAuditDocQuery = new QueryDefinition(query: queryForCustProfPrevAuditDocs).WithParameter("@docType", "CustomerProfileAudit").WithParameter("@customerId", customerId);

                CustomerProfileAudit custProfPrevAuditCosmosDoc = cosmosFixture.CosmosQueryItems<CustomerProfileAudit>(custProfPrevAuditDocQuery).OrderByDescending(x => x.CreatedOn).Skip(1).First(); //First doc will be latest and the second one will be old so skip 1st and take the second

                SoftAssertHelper softAssert = new();

                if (custProfPrevAuditCosmosDoc is not null)
                {

                    softAssert.True(custProfPrevAuditCosmosDoc.FirstName == custProfAuditCosmosDocAfterUpdate.FirstName,
                       $"For the Customer {customerId} , In the new Audit Doc, Expected the Customer Profile Doc FirstName to be {custProfPrevAuditCosmosDoc.FirstName} but found {custProfAuditCosmosDocAfterUpdate.FirstName} ");

                    softAssert.True(custProfPrevAuditCosmosDoc.LastName == custProfAuditCosmosDocAfterUpdate.LastName,
                      $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc LastName to be {custProfPrevAuditCosmosDoc.LastName} but found {custProfAuditCosmosDocAfterUpdate.LastName} ");

                    softAssert.True(custProfPrevAuditCosmosDoc.Title == custProfAuditCosmosDocAfterUpdate.Title,
                      $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Title to be {custProfPrevAuditCosmosDoc.Title} but found {custProfAuditCosmosDocAfterUpdate.Title} ");

                    softAssert.True(custProfPrevAuditCosmosDoc.Gender == custProfAuditCosmosDocAfterUpdate.Gender,
                      $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Gender to be {custProfPrevAuditCosmosDoc.Gender} but found {custProfAuditCosmosDocAfterUpdate.Gender} ");

                    softAssert.True(custProfPrevAuditCosmosDoc.Status == custProfAuditCosmosDocAfterUpdate.Status,
                     $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Status to be {custProfPrevAuditCosmosDoc.Status} but found {custProfAuditCosmosDocAfterUpdate.Status} ");

                    custProfPrevAuditCosmosDoc.DoB = custProfPrevAuditCosmosDoc.DoB == DateTime.MinValue ? null : custProfPrevAuditCosmosDoc.DoB;

                    softAssert.True(custProfPrevAuditCosmosDoc.DoB == custProfAuditCosmosDocAfterUpdate.DoB,
                     $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Date Of Birth to be {custProfPrevAuditCosmosDoc.DoB} but found {custProfAuditCosmosDocAfterUpdate.DoB} ");

                    softAssert.True(custProfPrevAuditCosmosDoc.NameChangeReason == custProfAuditCosmosDocAfterUpdate.NameChangeReason,
                     $"For the Customer {customerId} ,In the new Audit Doc,Expected the Customer Profile Doc Name Change Reason to be {custProfPrevAuditCosmosDoc.NameChangeReason} but found {custProfAuditCosmosDocAfterUpdate.NameChangeReason} ");

                }

                softAssert.True(custProfAuditCosmosDocAfterUpdate.CustomerProfileUpdatedBy == updatedBy, $"For the Customer {customerId} ,In the New Customer Profile Audit Doc ,Expected the UpdatedBy to be {updatedBy}  but found {custProfAuditCosmosDocAfterUpdate.CustomerProfileUpdatedBy}");

                softAssert.True(custProfAuditCosmosDocAfterUpdate.CreatedOn.ToString("yyyy-MM-dd") == currentDate.ToString("yyyy-MM-dd"), $"In the Customer Profile Audit Doc ,Expected the CreatedOn  Date to be {currentDate.ToString("yyyy-MM-dd")} but found {custProfAuditCosmosDocAfterUpdate.CreatedOn.ToString("yyyy-MM-dd")}");

                softAssert.True(custProfAuditCosmosDocAfterUpdate.DowngradeProhibitFlag == "N", $"For the Customerd {customerId} Expected the Downgrade Prohibit Flag value in Customer Profile Audit Document to be N after addition of new Card but found {custProfAuditCosmosDocAfterUpdate.DowngradeProhibitFlag}");

                softAssert.AssertAll();
            }

            #endregion

        }

    }
}
