using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.CustomerTransaction
{
    [Collection("AwardDocTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class RetrieveAwardDocTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public RetrieveAwardDocTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");

        }


        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_RetrieveAwards_SuccessScenario()
        {

            #region Pre-requisite Test Data for Award Document 

            string queryForAwardDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDocument).WithParameter("@docType", "Award");

            Award awardDocument = cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

            Assert.True(awardDocument is not null, $" No Award Audit Document Found.");

            string customerId = awardDocument.CustomerId;

            #endregion

            #region fetching all award documents for the customer from the cosmos DB

            string queryForAwardDocs = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDocs).WithParameter("@docType", "Award").WithParameter("@customerId", customerId);

            IEnumerable<Award> awardDocs = cosmosFixture.CosmosQueryItems<Award>(paramQuery).OrderByDescending(x => x.CreatedOn);

            #endregion

            #region API call

            HttpResponseMessage response = Microservice.RetrieveAwardDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            dynamic retrieveAwardResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!retrieveAwardResponse.ToString().Contains("Request failed"), $"Expected a valid response for retrieve award operation but found {retrieveAwardResponse.ToString()}");

            Assert.True(retrieveAwardResponse.customerId.ToString().Equals(customerId), $"Expected the CustomerId as {customerId} but found {retrieveAwardResponse.customerId}");

            AwardList[] retrieveAwardResp = JsonConvert.DeserializeObject<AwardList[]>(retrieveAwardResponse?.awards.ToString());

            AwardList[] awardApiResponses = retrieveAwardResp.OrderByDescending(x => x.createdOn).ToArray();

            #endregion

            #region Valdations for Audit Document from COSMOS vs API response

            int i = 0;

            foreach (Award awardDoc in awardDocs)
            {

                Assert.True(awardDoc.ProductId == awardApiResponses[i].productId, $"For the Customer {customerId} , Expected the {awardDoc.ProductId} Award ProductId to be {awardDoc.ProductId} but found {awardApiResponses[i].productId} ");

                Assert.True(awardDoc.CustomerId == awardApiResponses[i].customerId, $"For the Customer {customerId}, Expected the CustomerId to be {awardDoc.CustomerId} but found {awardApiResponses[i].customerId}");

                Assert.True(awardDoc.ProductId == awardApiResponses[i].productId, $"For the Customer {customerId}, Expected the AwardProductId to be {awardDoc.ProductId} but found {awardApiResponses[i].productId}");

                Assert.True(awardDoc.CreatedOn.ToString("yyyy-MM-dd") == awardApiResponses[i].createdOn.ToString("yyyy-MM-dd"), $"For the Customer {customerId}, Expected the CreatedOn to be {awardDoc.CreatedOn} but found {awardApiResponses[i].createdOn}");

                Assert.True(awardDoc.Status == awardApiResponses[i].status, $"For the Customer {customerId}, Expected the Status to be {awardDoc.Status} but found {awardApiResponses[i].status}");

                Assert.True(awardDoc.AwardId == awardApiResponses[i].awardId, $"For the Customer {customerId}, Expected the Status to be {awardDoc.AwardId} but found {awardApiResponses[i].awardId}");

                Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd") == awardApiResponses[i].startDate.ToString("yyyy-MM-dd"), $"For the Customer {customerId}, Expected the StartDate to be {awardDoc.StartDate} but found {awardApiResponses[i].startDate}");

                string expiryDate = awardDoc.ExpiryDate == DateTime.MinValue ? null : awardDoc.ExpiryDate.ToString("yyyy-MM-dd");

                Assert.True(expiryDate == awardApiResponses[i].expiryDate?.ToString("yyyy-MM-dd"), $"For the Customer {customerId}, Expected the ExpiryDate to be {awardDoc.ExpiryDate} but found {awardApiResponses[i].expiryDate}");

                i++;
            }
            #endregion
        }

        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Theory, RunOrder(2)]
        [InlineData("Flying Club reward voucher - Gold renewal", "1-1Z1D")]
        [InlineData("Gold Lounge Pass", "1-1ZKH")]
        [InlineData("Gold Upgrade Reward", "1-29QH")]
        [InlineData("Flying Club reward voucher - Gold 2000 Tier Points", "1-2A1P")]
        [InlineData("Flying Club reward voucher - UK Reward credit card", "1-2MG5VE1")]
        [InlineData("Flying Club reward voucher - US BoA credit card", "1-41G2YIN")]
        [InlineData("Flying Club reward voucher - UK Reward+ credit card", "1718881074393")]
        [InlineData("Flying Club reward voucher - Virgin Red US credit card", "1718882825098")]
        public async Task TC02_CreateAwardOrVoucher_SuccessScenario(string productName, string productId)
        {

            #region Pre-requisite Test Data for Award Document 

            string queryForFlyingClubMemberDocument = "SELECT TOP 10 * FROM c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFlyingClubMemberDocument).WithParameter("@docType", "FlyingClubMember");

            IEnumerable<FlyingClubMember> flyingClubMemberDocuments = cosmosFixture.CosmosQueryItems<FlyingClubMember>(parameterizedQuery);

            Assert.True(flyingClubMemberDocuments is not null, $" No FlyingClubMember Documents Found.");

            bool flag = false;

            string customerId = string.Empty;

            foreach (FlyingClubMember flyingClubMember in flyingClubMemberDocuments)
            {

                if (flyingClubMember.CustomerId is not null)
                {

                    customerId = flyingClubMember.CustomerId;

                    #region fetching if the award document already exists for the customer from the cosmos DB 

                    string queryForAwardDocs = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.ProductId= @productId order by c.CreatedOn desc ";

                    QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDocs).WithParameter("@docType", "Award").WithParameter("@customerId", customerId).WithParameter("@productId", productId);

                    IEnumerable<Award> awardDocs = cosmosFixture.CosmosQueryItems<Award>(paramQuery).OrderByDescending(x => x.CreatedOn);

                    if (!awardDocs.Any())
                    {
                        flag = true;
                        break;
                    }

                    #endregion
                }
            }

            Assert.True(flag, $"The test Data does not meet the test case requirement. Currently we are not able to fetch any test member with no associated {productName} and {productId} award documents");

            #endregion

            #region API call for assigning the award to the member

            string createdBy = "Automation";

            string awardNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            string startDate = DateTime.Now.ToString("yyyy-MM-dd");

            string expiryDate = DateTime.Now.AddYears(2).ToString("yyyy-MM-dd");

            string status = "Available";

            HttpResponseMessage createAwardResponse = Microservice.CreateAwardDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId, createdBy, awardNumber, productId, startDate, expiryDate, status).EnsureIsAcceptedStatusCode();

            string responseUrl = createAwardResponse?.Headers?.Location?.ToString();

            HttpResponseMessage actualCreateAwardResponse = Microservice.GetCreateAwardDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualCreateAwardResponse?.Content?.ReadAsStringAsync();

            dynamic actualCreateAwardResp = JsonConvert.DeserializeObject<dynamic>(content);

            Assert.True(!actualCreateAwardResp.ToString().Contains("Request failed"), $"Expected a valid response for create award operation but found {actualCreateAwardResp.ToString()}");

            string commandStatusResponse = JsonDocument.Parse(content).RootElement.GetProperty("commandStatus").ToString();

            string awardDocumentSectionInResponse = JsonDocument.Parse(content).RootElement.GetProperty("award").ToString();

            string productIdResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("productId").ToString();

            string createdOnResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("createdOn").ToString();

            createdOnResponse = DateTimeOffset.Parse(createdOnResponse).ToString("yyyy-MM-dd");

            string awardIdResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("awardId").ToString();

            string startDateResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("startDate").ToString();

            startDateResponse = DateTimeOffset.Parse(startDateResponse).ToString("yyyy-MM-dd");

            string expiryDateResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("expiryDate").ToString();

            expiryDateResponse = DateTimeOffset.Parse(expiryDateResponse).ToString("yyyy-MM-dd");

            string customerIdResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("customerId").ToString();

            string statusResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("status").ToString();

            Assert.True(JsonDocument.Parse(content).RootElement.GetProperty("commandStatus").ToString().Equals("4"), $"Expected the CommandStatus as 4 but found {content}");

            #endregion

            #region Valdations for Audit Document from COSMOS vs API response

            string queryForNewlyCreatedAwardDocs = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.ProductId= @productId order by c.CreatedOn desc ";

            QueryDefinition paramAwardQuery = new QueryDefinition(query: queryForNewlyCreatedAwardDocs).WithParameter("@docType", "Award").WithParameter("@customerId", customerId).WithParameter("@productId", productId);

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramAwardQuery);

            int retry = 5;

            while (awardDoc is null & retry > 0)
            {
                await Task.Delay(1000);

                awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramAwardQuery);

                retry--;
            }

            Assert.True(awardDoc is not null, $" No Award Audit Document Found.");

            Assert.True(awardDoc.CustomerId.Equals(customerIdResponse), $"For the Customer {customerId} , Expected the {awardDoc.CustomerId}but found {customerIdResponse} ");

            Assert.True(awardDoc.ProductId.Equals(productIdResponse), $"For the Customer {customerId}, Expected the {awardDoc.ProductId} Award ProductId to be {productIdResponse} but found {actualCreateAwardResp.award.productId} ");

            Assert.True(awardDoc.CreatedOn.ToString("yyyy-MM-dd").Equals(createdOnResponse), $"For the Customer {customerId}, Expected the CreatedOn to be {awardDoc.CreatedOn} but found {createdOnResponse}");

            Assert.True(awardDoc.AwardId.Equals(awardIdResponse), $"For the Customer {customerId}, Expected the Status to be {awardDoc.AwardId} but found {awardIdResponse}");

            Assert.True(awardDoc.Status.Equals(statusResponse), $"For the Customer {customerId}, Expected the Status to be {awardDoc.Status} but found {statusResponse}");

            Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd").Equals(startDateResponse), $"For the Customer {customerId}, Expected the StartDate to be {awardDoc.StartDate} but found {startDateResponse}");

            string expiryDateFromCosmos = awardDoc.ExpiryDate == DateTime.MinValue ? null : awardDoc.ExpiryDate.ToString("yyyy-MM-dd");

            Assert.True(expiryDateFromCosmos.Equals(expiryDateResponse), $"For the Customer {customerId}, Expected the ExpiryDate to be {awardDoc.ExpiryDate} but found {expiryDateResponse}");

            #endregion

        }

        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Theory, RunOrder(3)]
        [InlineData("Flying Club reward voucher - Gold renewal", "1-1Z1D")]
        [InlineData("Gold Lounge Pass", "1-1ZKH")]
        [InlineData("Gold Upgrade Reward", "1-29QH")]
        [InlineData("Flying Club reward voucher - Gold 2000 Tier Points", "1-2A1P")]
        [InlineData("Flying Club reward voucher - UK Reward credit card", "1-2MG5VE1")]
        [InlineData("Flying Club reward voucher - US BoA credit card", "1-41G2YIN")]
        [InlineData("Flying Club reward voucher - UK Reward+ credit card", "1718881074393")]
        [InlineData("Flying Club reward voucher - Virgin Red US credit card", "1718882825098")]
        public async Task TC03_UpdateAward_SuccessScenario(string productName, string productId)
        {

            #region Pre-requisite Test Data for Award Document 

            string queryForAwardDocument = "SELECT * FROM c where c.DocType = @docType and c.ProductId = @productId and c.Status = 'Available' order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDocument).WithParameter("@docType", "Award").WithParameter("@productId", productId);

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

            Assert.True(awardDoc is not null, $" No Award document found.");

            #endregion

            #region API call for assigning the award to the member

            string lastUpdatedBy = "Automation";

            string customerId = awardDoc.CustomerId;

            string id = awardDoc.id;

            string status = "Expired";

            HttpResponseMessage updateAwardResponse = Microservice.UpdateAwardDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId, id, lastUpdatedBy, status, productId).EnsureIsAcceptedStatusCode();

            string responseUrl = updateAwardResponse?.Headers?.Location?.ToString();

            HttpResponseMessage actualUpdateAwardResponse = Microservice.GetUpdateAwardDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualUpdateAwardResponse?.Content?.ReadAsStringAsync();

            dynamic actualUpdateAwardResp = JsonConvert.DeserializeObject<dynamic>(content);

            Assert.True(!actualUpdateAwardResp.ToString().Contains("Request failed"), $"Expected a valid response for create award operation but found {actualUpdateAwardResp.ToString()}");

            string commandStatusResponse = JsonDocument.Parse(content).RootElement.GetProperty("commandStatus").ToString();

            string awardDocumentSectionInResponse = JsonDocument.Parse(content).RootElement.GetProperty("award").ToString();

            string productIdResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("productId").ToString();

            string lastUpdatedByResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("lastUpdatedBy").ToString();

            string lastUpdatedOnResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("lastUpdatedOn").ToString();

            lastUpdatedOnResponse = DateTimeOffset.Parse(lastUpdatedOnResponse).ToString("yyyy-MM-dd");

            string statusInResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("status").ToString();

            string startDateResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("startDate").ToString();

            startDateResponse = DateTimeOffset.Parse(startDateResponse).ToString("yyyy-MM-dd");

            string customerIdResponse = JsonDocument.Parse(awardDocumentSectionInResponse).RootElement.GetProperty("customerId").ToString();

            Assert.True(JsonDocument.Parse(content).RootElement.GetProperty("commandStatus").ToString().Equals("4"), $"Expected the CommandStatus as 4 but found {content}");

            #endregion

            #region Valdations for Audit Document from COSMOS vs API response

            string queryForNewlyCreatedAwardDocs = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.id= @id order by c.CreatedOn desc ";

            QueryDefinition paramAwardQuery = new QueryDefinition(query: queryForNewlyCreatedAwardDocs).WithParameter("@docType", "Award").WithParameter("@customerId", customerId).WithParameter("@id", id);

            Award awardCosmosDoc = cosmosFixture.CosmosQueryItem<Award>(paramAwardQuery);

            int retry = 5;

            while (awardCosmosDoc is null & retry > 0)
            {
                await Task.Delay(1000);

                awardCosmosDoc = cosmosFixture.CosmosQueryItem<Award>(paramAwardQuery);

                retry--;
            }

            Assert.True(awardCosmosDoc is not null, $" No Award Audit Document Found.");

            Assert.True(awardCosmosDoc.id.Equals(id), $"For the Customer {customerId}, Expected the Id to be {awardCosmosDoc.id} but found {id}");

            Assert.True(awardCosmosDoc.CustomerId.Equals(customerIdResponse), $"For the Customer {customerId} , Expected the {awardCosmosDoc.CustomerId}but found {customerIdResponse} ");

            Assert.True(awardCosmosDoc.ProductId.Equals(productIdResponse), $"For the Customer {customerId}, Expected the {awardCosmosDoc.ProductId} Award ProductId to be {productIdResponse} but found {actualUpdateAwardResp.award.productId} ");

            string LastUpdatedInCosomos = DateTime.Parse(awardCosmosDoc.LastUpdatedOn).ToString("yyyy-MM-dd");

            Assert.True(LastUpdatedInCosomos.Equals(lastUpdatedOnResponse), $"For the Customer {customerId}, Expected the CreatedOn to be {LastUpdatedInCosomos} but found {lastUpdatedOnResponse}");

            Assert.True(awardCosmosDoc.Status.Equals(statusInResponse), $"For the Customer {customerId}, Expected the Status to be {awardDoc.Status} but found {statusInResponse}");

            #endregion
        }
    }
}
