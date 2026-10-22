using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.SupplementarySilver
{

    [Collection("RelationshipTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class SupplementarySilverLinkTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public SupplementarySilverLinkTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<MembershipCard>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        }



        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(21)]
        public async Task SupplementarySilverLinkAccount_Success()
        {

            string parentCustomerId = string.Empty;
            string parentMemberNumber = string.Empty;
            string childCustomerId = string.Empty;
            string childMemberNumber = string.Empty;
            string childFcmDocEtag = string.Empty;

            int count = 1;

            while (count < 4)//Running the test(Supp silver linking) for 3 times (diff test data)
            {


                #region Pre-Requisite for Fetching the Gold FC Number (Parent Account) for Supplementary Silver Linking 

                string queryForParentFCMDocuments = "SELECT TOP 10 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber = '' and c.CreatedBy != 'WEB-MYA' order by c.LastUpdatedOn desc";

                List<FlyingClubMember> flyingClubMember = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFCMDocuments).ToList();

                int retry = 5;

                while (retry > 0 && flyingClubMember == null)
                {
                    await Task.Delay(500);

                    flyingClubMember = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFCMDocuments).ToList();

                    retry--;
                }

                Assert.True(flyingClubMember is not null,
                    $"No Documents available for the Searched Query {queryForParentFCMDocuments}.");

                FlyingClubMember? flyingClubMemberDoc = null;

                foreach (FlyingClubMember fcmDoc in flyingClubMember)
                {

                    parentCustomerId = fcmDoc.CustomerId;
                    parentMemberNumber = fcmDoc.MemberNumber;

                    string queryForFCMDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.SSAMemberNumber=@ssaMemberNumber ";

                    QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@ssaMemberNumber", parentMemberNumber);

                    flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                    if (flyingClubMemberDoc is null)
                    {
                        break;
                    }

                }

                Assert.True(flyingClubMemberDoc is null,
                   "The Retrieved Parent FC Accounts are SSA Members to some other accounts.Test Data Criteria Didn't met");

                #endregion

                #region Pre-Requisite for Fetching the Red FC Numbers (Child Account) for Supplementary Silver Linking

                string queryForChildFcmDocs = "SELECT TOP 10 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Red' and c.Status='Active' and c.SSAMemberNumber = '' order by c.CreatedOn desc";

                flyingClubMember = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFcmDocs).ToList();

                retry = 5;

                while (retry > 0 && flyingClubMember == null)
                {
                    await Task.Delay(500);

                    flyingClubMember = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFcmDocs).ToList();

                    retry--;
                }

                Assert.True(flyingClubMember is not null,
                    $"No Documents available for the Searched Query {queryForChildFcmDocs}.");

                FlyingClubMember? flyingClubMemberDoc1 = null;

                foreach (FlyingClubMember fcmDoc in flyingClubMember)
                {

                    childCustomerId = fcmDoc.CustomerId;
                    childMemberNumber = fcmDoc.MemberNumber;
                    childFcmDocEtag = fcmDoc._etag;

                    string queryForFCMDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.SSAMemberNumber=@ssaMemberNumber ";

                    QueryDefinition query = new QueryDefinition(query: queryForFCMDoc).WithParameter("@ssaMemberNumber", childMemberNumber);

                    flyingClubMemberDoc1 = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

                    if (flyingClubMemberDoc1 is null)
                    {
                        break;
                    }

                }

                Assert.True(flyingClubMemberDoc1 is null,
                  "The Retrieved Child FC Accounts are SSA Members to some other accounts.Test Data Criteria Didn't met");

                #endregion

                #region Supplementary Account Linking using GraphQL endpoint

                HttpResponseMessage response = GraphQL.SupplementarySilverLink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                        , parentCustomerId, parentMemberNumber, "TestScript").EnsureIsSuccessStatusCode();

                dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                string expectedResponse = "Request successfully initiated";

                Assert.True(linkingResult.data.loyaltySupplementarySilverLinking.ToString().Contains(expectedResponse),
                    $"Expected {expectedResponse} in the response text but found {linkingResult.ToString()} - Iteration {count}");

                #endregion

                #region Child FlyingClubMember Document Validations After SSA Linking 

                string queryForChildFCMDocumentAfterLinking = "SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'FlyingClubMember' and c.PartitionKey=@partitionKey order by c._ts desc";

                QueryDefinition paramQueryChildFcmDoc = new QueryDefinition(query: queryForChildFCMDocumentAfterLinking).WithParameter("@partitionKey", childCustomerId);

                FlyingClubMember childFlyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQueryChildFcmDoc);

                int retryAgain = 5;

                while ((retryAgain > 0 && childFlyingClubMemberDocument == null) || childFcmDocEtag == childFlyingClubMemberDocument._etag)
                {
                    await Task.Delay(500);

                    childFlyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQueryChildFcmDoc);

                    retryAgain--;
                }

                Assert.True(childFlyingClubMemberDocument is not null,
                    $"No Document available for the Searched Query {queryForChildFCMDocumentAfterLinking}.");

                Assert.True((childFlyingClubMemberDocument.SSAMemberNumber).Equals(parentMemberNumber),
                    $"Expected ssaMemberNumber to be {parentMemberNumber} but found {childFlyingClubMemberDocument.SSAMemberNumber}");

                #endregion

                #region Child MembershipChange Document Validations After SSA Linking 

                string queryForChildMembershipChangeDocAfterLinking = "SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'MembershipChange' and c.PartitionKey=@partitionKey and c.Active='Y' order by c._ts desc";

                QueryDefinition paramQuery = new QueryDefinition(query: queryForChildMembershipChangeDocAfterLinking).WithParameter("@partitionKey", childCustomerId);

                MembershipChange childMembershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(paramQuery);

                retryAgain = 5;

                while ((retryAgain > 0 && childMembershipChangeDoc == null) || childMembershipChangeDoc.TierCode != "Silver")
                {
                    await Task.Delay(500);

                    childMembershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(paramQuery);

                    retryAgain--;
                }

                Assert.True(childMembershipChangeDoc is not null,
                    $"No Document available for the Searched Query {queryForChildMembershipChangeDocAfterLinking}.");

                Assert.True(childMembershipChangeDoc.TierCode.Equals("Silver"),
                    $"Expected the Active Tier Code to be Silver but found {childMembershipChangeDoc.TierCode}");

                string expectedTierStartDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

                string expectedTierEndDate = DateTime.UtcNow.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date.ToString("yyyy-MM-dd");

                Assert.True(childMembershipChangeDoc.StartDate.Value.DateTime.ToString("yyyy-MM-dd").Equals(expectedTierStartDate),
                    $"Expected the New Silver Tier StartDate to be {expectedTierStartDate} but found {childMembershipChangeDoc.StartDate.Value.DateTime.ToString("yyyy-MM-dd")}");

                Assert.True(childMembershipChangeDoc.EndDate.Value.DateTime.ToString("yyyy-MM-dd").Equals(expectedTierEndDate),
                    $"Expected the New Silver Tier EndDate to be {expectedTierEndDate} but found {childMembershipChangeDoc.EndDate.Value.DateTime.ToString("yyyy-MM-dd")}");


                #endregion

                #region Validating the MembershipCard Details Section of Child FCM Document After the Tier Change 

                childFlyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQueryChildFcmDoc);

                Assert.True(childFlyingClubMemberDocument.TierCode.Equals("Silver"),
                    $"Expected TierCode in child FCM Doc after linking to be Silver but found {childFlyingClubMemberDocument.TierCode}");

                MembershipCard membershipCard = childFlyingClubMemberDocument.MemberShipCardDetails;

                Assert.True(membershipCard.TierCode.Equals("Silver"),
                    $"Expected TierCode in child MembershipCardDetails section after linking to be Silver but found {membershipCard.TierCode}");

                Assert.True(membershipCard.StartDate.Value.DateTime.ToString("yyyy-MM-dd").Equals(expectedTierStartDate),
                    $"Expected  Tier StartDate in child MembershipCardDetails section after linking " +
                    $"to be to be {expectedTierStartDate} but found {membershipCard.StartDate.Value.DateTime.ToString("yyyy-MM-dd")}");

                Assert.True(Convert.ToDateTime(membershipCard.ExpiryDate).ToString("yyyy-MM-dd").Equals(expectedTierEndDate),
                    $"Expected Tier ExpiryDate in child MembershipCardDetails section after linking to be {expectedTierEndDate}" +
                    $" but found {Convert.ToDateTime(membershipCard.ExpiryDate).ToString("yyyy-MM-dd")}");

                #endregion

                count++;

                parentCustomerId = string.Empty;
                parentMemberNumber = string.Empty;
                childCustomerId = string.Empty;
                childMemberNumber = string.Empty;
                childFcmDocEtag = string.Empty;
            }


        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(22)]
        public async Task SupplementarySilverLinkAccount_InvalidMemberStatusError_WhenParentAccountStatusIsNotActive()
        {

            #region Pre-Requisite for Fetching the Gold FC Number (Parent Account) whose Status is not Active

            string queryForParentFCMDocuments = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Gold' and c.Status!='Active' and c.SSAMemberNumber = '' order by c.LastUpdatedOn desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForParentFCMDocuments);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForParentFCMDocuments);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForParentFCMDocuments}.");

            string parentCustomerId = flyingClubMember.CustomerId;

            string parentMemberNumber = flyingClubMember.MemberNumber;

            #endregion

            #region Pre-Requisite for Fetching the Red FC Numbers (Child Account) for Supplementary Silver Linking

            string queryForChildFcmDocs = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Red' and c.Status='Active' and c.SSAMemberNumber = '' and c.CreatedBy='DELTA.COM' order by c.CreatedOn desc";

            flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

            retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForChildFcmDocs}.");

            string childCustomerId = flyingClubMember.CustomerId;

            string childMemberNumber = flyingClubMember.MemberNumber;

            #endregion

            #region Supplementary Account Linking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverLink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , parentCustomerId, parentMemberNumber, "TestScript");

            dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "SSA - Invalid Parent Status";

            Assert.True(linkingResult.errors[0].extensions.response.ToString().Contains(expectedResponse),
               $"Expected {expectedResponse} in the response but found {linkingResult.ToString()}");

            #endregion
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(23)]
        public async Task SupplementarySilverLinkAccount_MandatoryFieldMissingError_WhenParentDetailsAreEmpty()
        {

            string parentCustomerId = string.Empty;

            string parentMemberNumber = string.Empty;

            #region Pre-Requisite for Fetching the Red FC Numbers (Child Account) for Supplementary Silver Linking

            string queryForChildFcmDocs = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Red' and c.Status='Active' and c.SSAMemberNumber = '' and c.CreatedBy='DELTA.COM' order by c.CreatedOn desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForChildFcmDocs}.");

            string childCustomerId = flyingClubMember.CustomerId;

            string childMemberNumber = flyingClubMember.MemberNumber;

            #endregion

            #region Supplementary Account Linking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverLink(tokenFixture.GetToken("graphqlv2")
                , childCustomerId, childMemberNumber, parentCustomerId, parentMemberNumber, "TestScript");

            dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)";

            Assert.True(linkingResult.errors[0].message.ToString().Contains(expectedResponse),
               $"Expected {expectedResponse} in the response but found {linkingResult.ToString()}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(24)]
        public async Task SupplementarySilverLinkAccount_MandatoryFieldMissingError_WhenChildDetailsAreEmpty()
        {

            #region Pre-Requisite for Fetching the Gold FC Number (Parent Account) whose Status is not Active

            string queryForParentFCMDocuments = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Gold' and c.Status!='Active' and c.SSAMemberNumber = '' order by c.LastUpdatedOn desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForParentFCMDocuments);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForParentFCMDocuments);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForParentFCMDocuments}.");

            string parentCustomerId = flyingClubMember.CustomerId;

            string parentMemberNumber = flyingClubMember.MemberNumber;

            #endregion

            string childCustomerId = string.Empty;

            string childMemberNumber = string.Empty;

            #region Supplementary Account Linking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverLink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , parentCustomerId, parentMemberNumber, "TestScript");

            dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)";

            Assert.True(linkingResult.errors[0].message.ToString().Contains(expectedResponse),
               $"Expected {expectedResponse} in the response but found {linkingResult.ToString()}");

            #endregion
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(25)]
        public async Task SupplementarySilverLinkAccount_MandatoryFieldMissingError_WhenInitiatedByFieldValueIsNullOrEmpty()
        {

            #region Pre-Requisite for Fetching the Gold FC Number (Parent Account) for Supplementary Silver Linking 

            string queryForParentFCMDocuments = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber = '' order by c.LastUpdatedOn desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForParentFCMDocuments);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForParentFCMDocuments);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForParentFCMDocuments}.");

            string parentCustomerId = flyingClubMember.CustomerId;
            string parentMemberNumber = flyingClubMember.MemberNumber;

            #endregion

            #region Pre-Requisite for Fetching the Red FC Numbers (Child Account) for Supplementary Silver Linking

            string queryForChildFcmDocs = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Red' and c.Status='Active' and c.SSAMemberNumber = '' and c.CreatedBy='DELTA.COM' order by c.CreatedOn desc";

            flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

            retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForChildFcmDocs}.");

            string childCustomerId = flyingClubMember.CustomerId;
            string childMemberNumber = flyingClubMember.MemberNumber;
            string initiatedBy = string.Empty;

            #endregion

            #region Supplementary Account Linking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverLink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , parentCustomerId, parentMemberNumber, initiatedBy);

            dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "'InitiatedBy'must be specified.";

            Assert.True(linkingResult.errors[0].message.ToString().Contains(expectedResponse),
                 $"Expected {expectedResponse} in the response but found {linkingResult.ToString()}");

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(26)]
        public async Task SupplementarySilverLinkAccount_InvalidMemberTierCodeError_WhenParentAccountTierIsNotGold()
        {

            #region  Fetching the Red and Silver FC Numbers (Parent Account) for Supplementary Silver Linking 

            string queryForParentFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode not in ('Gold','Glass') and c.Status='Active' and c.SSAMemberNumber = '' order by c.CreatedOn desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForParentFcmDoc);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForParentFcmDoc);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForParentFcmDoc}.");


            string parentCustomerId = flyingClubMember.CustomerId;

            string parentMemberNumber = flyingClubMember.MemberNumber;

            #endregion

            #region Pre-Requisite for Fetching the Red FC Numbers (Child Account) for Supplementary Silver Linking

            string queryForChildFcmDocs = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Red' and c.Status='Active' and c.SSAMemberNumber = ''  order by c.CreatedOn desc";

            flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

            retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForChildFcmDocs}.");

            string childCustomerId = flyingClubMember.CustomerId;

            string childMemberNumber = flyingClubMember.MemberNumber;

            #endregion

            #region Supplementary Account Linking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverLink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , parentCustomerId, parentMemberNumber, "TestScript");

            dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "SSA - Invalid Parent Tier Code";

            Assert.True(linkingResult.errors[0].extensions.response.ToString().Contains(expectedResponse),
               $"Expected {expectedResponse} in the response but found {linkingResult.ToString()}");

            #endregion
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(27)]
        public async Task SupplementarySilverLinkAccount_InvalidMemberTierCodeError_WhenChildAccountTierIsGold()
        {

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentCustomerId = string.Empty;

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.SSAMemberNumber=@ssaMemberNumber";

                QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@ssaMemberNumber", x.MemberNumber);

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = "SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey=@partitionKey";

                parentCustomerId = x.CustomerId;

                parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@partitionKey", x.CustomerId);

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            Assert.True(!string.IsNullOrEmpty(parentCustomerId),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion         

            #region Pre-Requisite for Fetching the Gold FC Numbers (Child Account) for Supplementary Silver Linking

            string queryForChildFcmDocs = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber = '' and c.CreatedBy='DELTA.COM' order by c.CreatedOn desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForChildFcmDocs}.");

            string childCustomerId = flyingClubMember.CustomerId;

            string childMemberNumber = flyingClubMember.MemberNumber;

            #endregion

            #region Supplementary Account Linking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverLink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , parentCustomerId, parentMemberNumber, "TestScript");

            dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "SSA - Invalid Child Tier Code";

            Assert.True(linkingResult.errors[0].extensions.response.ToString().Contains(expectedResponse),
               $"Expected {expectedResponse} in the response but found {linkingResult.ToString()}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(28)]
        public async Task SupplementarySilverLinkAccount_InvalidMemberStatusError_WhenChildAccountStatusIsNotValid()
        {

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentCustomerId = string.Empty;

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.SSAMemberNumber=@ssaMemberNumber";

                QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@ssaMemberNumber", x.MemberNumber);

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = "SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey=@partitionKey";

                parentCustomerId = x.CustomerId;

                parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@partitionKey", x.CustomerId);

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            Assert.True(!string.IsNullOrEmpty(parentCustomerId),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion         

            #region Pre-Requisite for Fetching the Red FC Numbers (Child Account) for Supplementary Silver Linking

            string queryForChildFcmDocs = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Red' and c.Status not in ('Active','Active Pending','Inactive','Frozen','Expired','ActivePending') and c.SSAMemberNumber = '' order by c.LastUpdatedOn desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForChildFcmDocs}.");

            string childCustomerId = flyingClubMember.CustomerId;

            string childMemberNumber = flyingClubMember.MemberNumber;

            #endregion

            #region Supplementary Account Linking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverLink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , parentCustomerId, parentMemberNumber, "TestScript");

            dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "SSA - Invalid Child Status";

            Assert.True(linkingResult.errors[0].extensions.response.ToString().Contains(expectedResponse),
               $"Expected {expectedResponse} in the response but found {linkingResult.ToString()}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(29)]
        public async Task SupplementarySilverLinkAccount_ParentAlreadyLinkedError_WhenParentAccountIsAlreadyLinkedToAnAccount()
        {
            #region Fetching a Supplementary Silver Account

            string queryForChildFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.SSAMemberNumber!='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> childFcmDocs = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFcmDoc);

            string parentCustomerId = string.Empty;

            string? parentMemberNumber = childFcmDocs.FirstOrDefault(x =>
            {

                string queryForParentFcmDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.MemberNumber=@ssaMemberNumber";

                QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForParentFcmDoc).WithParameter("@ssaMemberNumber", x.SSAMemberNumber);

                IEnumerable<FlyingClubMember> parentFcmDoc = cosmosFixture.CosmosQueryItems<FlyingClubMember>(parameterizedQuery);

                parentCustomerId = parentFcmDoc.FirstOrDefault().CustomerId;

                return parentFcmDoc.Any(c => c.TierCode.Equals("Gold"));

            })?.SSAMemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find a valid Parent Or Gold Member for Delinking");

            #endregion

            #region Pre-Requisite for Fetching the Red FC Numbers (Child Account) for Supplementary Silver Linking

            string queryForChildFcmDocs = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.TierCode='Red' and c.Status not in ('Active','Active Pending','Inactive','Frozen','Expired') and c.SSAMemberNumber = '' and c.CreatedBy='DELTA.COM' order by c.CreatedOn desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDocs);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Documents available for the Searched Query {queryForChildFcmDocs}.");

            string childMemberNumber = flyingClubMember.MemberNumber;

            string childCustomerId = flyingClubMember.CustomerId;

            #endregion

            #region Supplementary Account Linking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverLink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , parentCustomerId, parentMemberNumber, "TestScript");

            dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "SSA - Linked Gold ParentAccount - Parent account is already linked with another child account";

            Assert.True(linkingResult.errors[0].extensions.response.ToString().Contains(expectedResponse),
               $"Expected {expectedResponse} in the response but found {linkingResult.ToString()}");

            #endregion
        }

    }
}
