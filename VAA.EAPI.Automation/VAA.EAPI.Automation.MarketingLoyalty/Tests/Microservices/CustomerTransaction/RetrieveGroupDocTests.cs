using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.CustomerTransaction
{

    public class RetrieveGroupDocTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public RetrieveGroupDocTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<Group>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<GroupHistory>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");

        }


        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact, RunOrder(1)]
        public async Task A_RetrieveGroupDocByCustomerId_SuccessScenario()
        {
            #region Test Data

            string queryForGroupDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForGroupDoc).WithParameter("@docType", "Group");

            Group group = cosmosFixture.CosmosQueryItem<Group>(parameterizedQuery);

            string customerId = group.HeadOfGroupCustomerId;

            #endregion

            HttpResponseMessage response = Microservice.RetrieveGroupDocByCustomerId(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            dynamic retrieveGroupDocResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            dynamic result = retrieveGroupDocResult.groupDocuments[0];

            //Validating Group Document Details
            Assert.True(group.HeadOfGroupCustomerId.Equals(customerId));

            Assert.True(result.status.ToString().Equals(group.Status),
                $"Expected Group Documents Status as {group.Status} but found {result.status.ToString()}");

            Assert.True(result.groupTypeCode.ToString().Equals(group.GroupTypeCode),
                $"Expected Group Type Code as {group.GroupTypeCode} but found {result.groupTypeCode.ToString()}");

            Assert.True(result.groupId.ToString().Equals(group.GroupId),
                $"Expected Group Doc Id as {group.GroupId} but found {result.groupId.ToString()}");

            Assert.True(result.groupName.ToString().Equals(group.GroupName),
                $"Expected Group Name as {group.GroupName} but found {result.groupName.ToString()}");

        }

        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact, RunOrder(2)]
        public async Task B_RetrieveGroupDocByGroupId_SuccessScenario()
        {

            #region Test Data

            string queryForGroupDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForGroupDoc).WithParameter("@docType", "Group");

            Group group = cosmosFixture.CosmosQueryItem<Group>(parameterizedQuery);

            string groupId = group.GroupId;

            #endregion

            HttpResponseMessage response = Microservice.RetrieveGroupDocByGroupId(tokenFixture.GetToken("customertxn"), ocpSubKey, groupId).EnsureIsSuccessStatusCode();

            dynamic retrieveGroupDocResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            dynamic result = retrieveGroupDocResult.groupDocuments[0];

            //Validating Group Document Details
            Assert.True(result.headOfGroupCustomerId.ToString().Equals(group.HeadOfGroupCustomerId),
                $"Expected Head Of Group CustomerId  as {group.HeadOfGroupCustomerId} " +
                $"but found {result.headOfGroupCustomerId.ToString()}");

            Assert.True(result.status.ToString().Equals(group.Status),
                $"Expected Group Documents Status as {group.Status} but found {result.status.ToString()}");

            Assert.True(result.groupTypeCode.ToString().Equals(group.GroupTypeCode),
                $"Expected Group Type Code as {group.GroupTypeCode} but found {result.groupTypeCode.ToString()}");

            Assert.True(result.groupId.ToString().Equals(group.GroupId),
                $"Expected Group Doc Id as {group.GroupId} but found {result.groupId.ToString()}");

            Assert.True(result.groupName.ToString().Equals(group.GroupName),
                $"Expected Group Name as {group.GroupName} but found {result.groupName.ToString()}");

        }

        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact, RunOrder(3)]
        public async Task C_RetrieveGroupHistoryByCustomerId_SuccessScenario()
        {

            #region Test Data

            string queryForGroupDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForGroupDoc).WithParameter("@docType", "Group");

            Group group = cosmosFixture.CosmosQueryItem<Group>(parameterizedQuery);

            string customerId = group.GroupHistory.First().CustomerId;

            #endregion

            HttpResponseMessage response = Microservice.RetrieveGroupHistoryByCustomerId(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            dynamic retrieveGroupDocResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            //Retrieving the Head Of Group CustomerId to which this customer was or is linked 
            string headofGroupCustomerId = retrieveGroupDocResult.groupDocuments[0].headOfGroupCustomerId;

            dynamic result = retrieveGroupDocResult.groupDocuments[0];

            //Validating Group Document Details
            Assert.True(group.HeadOfGroupCustomerId.Equals(headofGroupCustomerId),
                $"Expected Head Of Group CustomerId  as {headofGroupCustomerId} but found {group.HeadOfGroupCustomerId}");

            Assert.True(result.status.ToString().Equals(group.Status),
                $"Expected Group Documents Status as {group.Status} but found {result.status.ToString()}");

            Assert.True(result.groupTypeCode.ToString().Equals(group.GroupTypeCode),
                $"Expected Group Type Code as {group.GroupTypeCode} but found {result.groupTypeCode.ToString()}");

            Assert.True(result.groupId.ToString().Equals(group.GroupId),
                $"Expected Group Doc Id as {group.GroupId} but found {result.groupId.ToString()}");

            Assert.True(result.groupName.ToString().Equals(group.GroupName),
                $"Expected Group Name as {group.GroupName} but found {result.groupName.ToString()}");

            result = retrieveGroupDocResult.groupDocuments[0].groupHistory;

            bool flag = false;

            GroupHistory[] groupHistory = group.GroupHistory;

            if (result.Count.Equals(groupHistory.Count()))
            {
                for (int i = 0; i < groupHistory.Count(); i++)
                {
                    if (customerId.Equals(groupHistory[i].CustomerId))
                    {

                        Assert.True(DateTime.Parse(result[i].startDate.ToString()).Date.Equals(DateTime.Parse(groupHistory[i].StartDate).Date),
                       $"Expected the Association StartDate as {DateTime.Parse(groupHistory[i].StartDate).Date}" +
                       $" but found {DateTime.Parse(result[i].startDate.ToString()).Date}");

                        if (!groupHistory[i].ExitDate.IsNullOrEmpty())
                            Assert.True(DateTime.Parse(result[i].exitDate.ToString()).Date.Equals(DateTime.Parse(groupHistory[i].ExitDate).Date),
                            $"Expected the Association EndDate or ExitDate as {DateTime.Parse(groupHistory[i].ExitDate).Date}" +
                            $" but found {DateTime.Parse(result[i].exitDate.ToString()).Date}");

                        flag = true;

                        break;

                    }
                }
            }

            Assert.True(flag, $"Expected the Group History Details of the customer {customerId} in the Group History" +
                $"Document of the HeadOfGroupCustomerId {headofGroupCustomerId} but didn't find the details.");

        }
    }
}
