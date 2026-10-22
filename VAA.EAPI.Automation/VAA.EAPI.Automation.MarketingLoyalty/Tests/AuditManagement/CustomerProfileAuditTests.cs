using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.AuditManagement
{
    [Collection("RelationshipTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CustomerProfileAuditTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture _tokenFixture;
        private readonly CosmosFixture _cosmosFixture;

        public string env = Config.Environment;

        PreRequisiteTestData preRequisiteTestData;

        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        public CustomerProfileAuditTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            _tokenFixture = tokenFixture;
            _cosmosFixture = cosmosFixture;

            preRequisiteTestData = new();

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            _cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<CustomerProfileAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            _cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(43)]
        public async Task TC01_ValidateCustomerProfileAuditHistory_SuccessScenario()
        {

            #region Test Data           

            string queryForCustomerProfileAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForCustomerProfileAuditDoc).WithParameter("@docType", "CustomerProfileAudit");

            CustomerProfileAudit customerProfAuditDoc = _cosmosFixture.CosmosQueryItem<CustomerProfileAudit>(query);

            Assert.True(customerProfAuditDoc is not null, "No Customer Profile Audit Document Found.");

            string customerId = customerProfAuditDoc.CustomerId;

            string queryForCustProfAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustProfAuditDocs).WithParameter("@docType", "CustomerProfileAudit").WithParameter("@customerId", customerId);

            IEnumerable<CustomerProfileAudit> custProfAuditCosmosDocs = _cosmosFixture.CosmosQueryItems<CustomerProfileAudit>(paramQuery).OrderByDescending(x => x.CreatedOn);

            #endregion

            #region API Call

            HttpResponseMessage response = GraphQL.RetrieveCustomerProfileAudit(_tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve Customer Profile audit graphql operation but found {result.ToString()}");

            CustomerProfileAuditResponse[] custProfileAuditDocs = JsonConvert.DeserializeObject<CustomerProfileAuditResponse[]>(result?.data.retrieveCustomerProfileAuditHistory.customerProfileHistoryAudit.ToString());

            custProfileAuditDocs = custProfileAuditDocs.OrderByDescending(x => x.createdOn).ToArray();

            #endregion

            #region Validation

            int i = 0;

            foreach (CustomerProfileAudit customerProfileAudit in custProfAuditCosmosDocs)
            {
                string docId = customerProfileAudit.id;

                Assert.True(customerProfileAudit.CreatedBy == custProfileAuditDocs[i].createdBy,
                    $"For the Doc Id {docId} ,Expected the Customer Profile Doc CreatedBy to be {customerProfileAudit.CreatedBy} but found {custProfileAuditDocs[i].createdBy} ");


                Assert.True(customerProfileAudit.CreatedOn == custProfileAuditDocs[i].createdOn,
                    $"For the Doc Id {docId} ,Expected the Customer Profile Doc CreatedOn to be {customerProfileAudit.CreatedOn} but found {custProfileAuditDocs[i].createdOn} ");

                Assert.True(customerProfileAudit.FirstName == custProfileAuditDocs[i].firstName,
                   $"For the Doc Id {docId} ,Expected the Customer Profile Doc FirstName to be {customerProfileAudit.FirstName} but found {custProfileAuditDocs[i].firstName} ");

                Assert.True(customerProfileAudit.LastName == custProfileAuditDocs[i].lastName,
                  $"For the Doc Id {docId} ,Expected the Customer Profile Doc LastName to be {customerProfileAudit.LastName} but found {custProfileAuditDocs[i].lastName} ");

                Assert.True(customerProfileAudit.Title == custProfileAuditDocs[i].title,
                  $"For the Doc Id {docId} ,Expected the Customer Profile Doc Title to be {customerProfileAudit.Title} but found {custProfileAuditDocs[i].title} ");

                Assert.True(customerProfileAudit.Gender == custProfileAuditDocs[i].gender,
                  $"For the Doc Id {docId} ,Expected the Customer Profile Doc Gender to be {customerProfileAudit.Gender} but found {custProfileAuditDocs[i].gender} ");

                Assert.True(customerProfileAudit.Status == custProfileAuditDocs[i].status,
                 $"For the Doc Id {docId} ,Expected the Customer Profile Doc Status to be {customerProfileAudit.Status} but found {custProfileAuditDocs[i].status} ");

                Assert.True(customerProfileAudit.GoldStartDate == custProfileAuditDocs[i].goldStartDate,
                $"For the Doc Id {docId} ,Expected the Customer Profile Doc Gold Start Date override to be {customerProfileAudit.GoldStartDate} but found {custProfileAuditDocs[i].goldStartDate} ");

                Assert.True(customerProfileAudit.TelephonePassword == custProfileAuditDocs[i].telephonePassword,
                 $"For the Doc Id {docId} ,Expected the Customer Profile Telephone Password to be {customerProfileAudit.TelephonePassword} but found {custProfileAuditDocs[i].telephonePassword} ");

                customerProfileAudit.DoB = customerProfileAudit.DoB == DateTime.MinValue ? null : customerProfileAudit.DoB;

                Assert.True(customerProfileAudit.DoB == custProfileAuditDocs[i].doB,
                 $"For the Doc Id {docId} ,Expected the Customer Profile Doc Date Of Birth to be {customerProfileAudit.DoB} but found {custProfileAuditDocs[i].doB} ");

                Assert.True(customerProfileAudit.NameChangeReason == custProfileAuditDocs[i].nameChangeReason,
                 $"For the Doc Id {docId} ,Expected the Customer Profile Doc Name Change Reason to be {customerProfileAudit.NameChangeReason} but found {custProfileAuditDocs[i].nameChangeReason} ");

                i++;

            }

            #endregion

        }



        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(44)]
        public async Task TC02_ValidateCustomerProfileAuditHistoryForNewEnrolment_SuccessScenario()
        {
            #region PreRequisite to create an enrolment before validations for the enrolment

            string[] titles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
            Random random = new();
            string title = titles[random.Next(titles.Length)];
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";
            string postCode = "RH11 9LG";
            string country = "GB";
            string channelID = "POWERAPP";
            string enroledBy = DataGenerator.RandomFirstName();

            string dateOfBirth = DateTime.UtcNow.AddDays(-6600).ToString("yyyy-MM-dd");

            (string memberNumber, string customerId) = await preRequisiteTestData.MemberEnrolment(channelID, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            #region Retrieving data from cosmos collection

            string queryForCustProfAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustProfAuditDocs).WithParameter("@docType", "CustomerProfileAudit").WithParameter("@customerId", customerId);

            IEnumerable<CustomerProfileAudit> custProfAuditCosmosDocs = _cosmosFixture.CosmosQueryItems<CustomerProfileAudit>(paramQuery).OrderByDescending(x => x.CreatedOn);

            int retry = 10;

            while (!custProfAuditCosmosDocs.Any() && retry > 0)
            {
                custProfAuditCosmosDocs = _cosmosFixture.CosmosQueryItems<CustomerProfileAudit>(paramQuery).OrderByDescending(x => x.CreatedOn);

                await Task.Delay(2000);

                retry--;
            }

            Assert.True(custProfAuditCosmosDocs.Any(),
                $"After the Enrolment ,Failed to Retrieve Customer Profile Audit Document after retry of 10 seconds for the customer {customerId}");

            Assert.True(custProfAuditCosmosDocs.Count() == 1,
                $"Expected only one Customer Profile Audit Document after the enrolment but found {custProfAuditCosmosDocs.Count()}.");

            #endregion

            #region API Call

            HttpResponseMessage response = GraphQL.RetrieveCustomerProfileAudit(_tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve Customer Profile audit graphql operation but found {result.ToString()}");

            CustomerProfileAuditResponse[] custProfileAuditDocs = JsonConvert.DeserializeObject<CustomerProfileAuditResponse[]>(result?.data.retrieveCustomerProfileAuditHistory.customerProfileHistoryAudit.ToString());

            custProfileAuditDocs = custProfileAuditDocs.OrderByDescending(x => x.createdOn).ToArray();

            Assert.True(custProfileAuditDocs.Count() == 1,
                $"For the CustomerId {customerId} ,Expected only one Customer Profile Audit in the GraphQL Response after the enrolment but found {custProfileAuditDocs.Count()} ");

            #endregion

            #region Validation

            CustomerProfileAudit customerProfileAudit = custProfAuditCosmosDocs.First();

            DateTime currentDate = DateTime.UtcNow.Date;

            Assert.True(customerProfileAudit.CreatedBy == enroledBy,
                $"In the Customer Profile Audit Doc, Expected the  CreatedBy to be {enroledBy} but found {customerProfileAudit.CreatedBy} ");

            Assert.True(customerProfileAudit.CreatedOn.ToString("dd-MMM-yyyy") == currentDate.ToString("dd-MMM-yyyy"),
                $"In the Customer Profile Audit Doc, Expected the CreatedOn to be {currentDate.ToString("dd-MMM-yyyy")} but found {customerProfileAudit.CreatedOn.ToString("dd-MMM-yyyy")} ");

            Assert.True(customerProfileAudit.FirstName == firstName,
               $"In the Customer Profile Audit Doc, Expected the FirstName to be {firstName} but found {customerProfileAudit.FirstName} ");

            Assert.True(customerProfileAudit.LastName == lastName,
              $"In the Customer Profile Audit Doc, Expected the LastName to be {lastName} but found {customerProfileAudit.LastName} ");

            Assert.True(customerProfileAudit.Title == title,
              $"In the Customer Profile Audit Doc, Expected the title {title} but found {customerProfileAudit.Title} ");

            Assert.True(customerProfileAudit.Status == "Active",
             $"In the Customer Profile Audit Doc, Expected the Status to be Active but found {customerProfileAudit.Status} ");

            Assert.True(customerProfileAudit.DoB?.ToString("yyyy-MM-dd") == dateOfBirth,
             $"In the Customer Profile Audit Doc, Expected dateOfBirth to be {dateOfBirth} but found {customerProfileAudit.DoB?.ToString("yyyy-MM- dd")} ");

            int i = 0;

            string docId = customerProfileAudit.id;

            Assert.True(customerProfileAudit.CreatedBy == custProfileAuditDocs[i].createdBy,
                $"For the Doc Id {docId} ,Expected the Customer Profile Doc CreatedBy to be {customerProfileAudit.CreatedBy} but found {custProfileAuditDocs[i].createdBy} ");

            Assert.True(customerProfileAudit.CreatedOn == custProfileAuditDocs[i].createdOn,
                $"For the Doc Id {docId} ,Expected the Customer Profile Doc CreatedOn to be {customerProfileAudit.CreatedOn} but found {custProfileAuditDocs[i].createdOn} ");

            Assert.True(customerProfileAudit.FirstName == custProfileAuditDocs[i].firstName,
               $"For the Doc Id {docId} ,Expected the Customer Profile Doc FirstName to be {customerProfileAudit.FirstName} but found {custProfileAuditDocs[i].firstName} ");

            Assert.True(customerProfileAudit.LastName == custProfileAuditDocs[i].lastName,
              $"For the Doc Id {docId} ,Expected the Customer Profile Doc LastName to be {customerProfileAudit.LastName} but found {custProfileAuditDocs[i].lastName} ");

            Assert.True(customerProfileAudit.Title == custProfileAuditDocs[i].title,
              $"For the Doc Id {docId} ,Expected the Customer Profile Doc Title to be {customerProfileAudit.Title} but found {custProfileAuditDocs[i].title} ");

            Assert.True(customerProfileAudit.Gender == custProfileAuditDocs[i].gender,
              $"For the Doc Id {docId} ,Expected the Customer Profile Doc Gender to be {customerProfileAudit.Gender} but found {custProfileAuditDocs[i].gender} ");

            Assert.True(customerProfileAudit.Status == custProfileAuditDocs[i].status,
             $"For the Doc Id {docId} ,Expected the Customer Profile Doc Status to be {customerProfileAudit.Status} but found {custProfileAuditDocs[i].status} ");

            Assert.True(customerProfileAudit.DoB == custProfileAuditDocs[i].doB,
             $"For the Doc Id {docId} ,Expected the Customer Profile Doc Date Of Birth to be {customerProfileAudit.DoB} but found {custProfileAuditDocs[i].doB} ");

            Assert.True(customerProfileAudit.NameChangeReason == custProfileAuditDocs[i].nameChangeReason,
             $"For the Doc Id {docId} ,Expected the Customer Profile Doc Name Change Reason to be {customerProfileAudit.NameChangeReason} but found {custProfileAuditDocs[i].nameChangeReason} ");


            #endregion

        }
    }

}

