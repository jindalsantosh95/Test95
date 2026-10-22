using System.Xml;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoyaltySync
{

    public class SFPDTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>, IClassFixture<CertificateFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly CertificateFixture certificateFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        public SFPDTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture, CertificateFixture certificateFixture)
        {
            this.certificateFixture = certificateFixture;
            this.certificateFixture.ImportCertificate("certificate", Vault.Default, "DlCertificate");

            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
            this.certificateFixture = certificateFixture;
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact]
        public async Task TC01_UpdateSecurePassengerDataUsingGraphqlandValidateUsingAIR4Service()
        {

            #region Test data Retrieval          

            string queryForFlyingClubmemberDoc = "SELECT TOP 1 * FROM CustomerTransaction c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition query = new QueryDefinition(query: queryForFlyingClubmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember flyingClubDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

            Assert.True(flyingClubDoc is not null, "Unable to fetch flying club document. Cannot proceed with test execution");

            string customerId = flyingClubDoc.CustomerId;

            string fcNumber = flyingClubDoc.MemberNumber;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c.CreatedOn desc";

            QueryDefinition queryForCustomerDetails = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer customerDocument = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDetails);

            Assert.True(customerDocument is not null, "No Customer documents found. Cannot proceed with test execution");

            string enrollmentChannel = customerDocument.EnrollmentChannel;

            string firstName = DataGenerator.RandomFirstName();

            string lastName = DataGenerator.RandomLastName();

            string dateOfBirth = DataGenerator.RandomDateOfBirth().ToString("yyyy-MM-dd");

            string gender = customerDocument.Gender ?? "M";

            #endregion

            #region Add loyaltySyncSFPD GraphQl API Call and Validations

            HttpResponseMessage response = GraphQL.loyaltySyncSFPD(tokenFixture.GetToken("graphqlv2"), customerId, dateOfBirth, enrollmentChannel, fcNumber, firstName, gender, lastName).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result?.ToString().Contains("Request failed"), $"Expected a valid response for loyalty Sync - Update SFPD operation but found {result?.ToString()}");

            Assert.True(result?.data?.loyaltySyncSFPD?.psdsSyncStatus?.ToString().Equals("True"), $"Expected psdsSyncStatus equals True but found {result?.ToString()} ");

            Assert.True(!string.IsNullOrEmpty(result.data.loyaltySyncSFPD.requestId.ToString()), $"Expected a valid request id in the response but found {result?.data?.loyaltySyncSFPD?.requestId.ToString()}");

            #endregion

            #region AIR4 XML request for DL Call - Retrieve SFPD details and Validations
            RetryHelper.RunWithRetry(() =>
            {

                SoftAssertHelper softAssert = new();

                string requestSFPDRetrieve;

                string transactionId = DataGenerator.RandomNumber(10000, 999999, true);

                XmlDocument sfpdRetrieveResponseDocument = AIR4Helper.SFPDRetrieve(certificateFixture.GetCertificate("certificate"), "VS", transactionId, fcNumber, out requestSFPDRetrieve);

                XmlNamespaceManager sfpdRetrieveResponseManager = sfpdRetrieveResponseDocument.CreateXmlNamespaceManager();

                string sfpdRetrieveResponseInfoMessage = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/@isRecordRetrieved", sfpdRetrieveResponseManager)?.InnerText;

                softAssert.True(sfpdRetrieveResponseInfoMessage.Is("true"), $"Expected SFPD Retrieve response true was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveResponseInfoMessage}");

                string sfpdRetrieveAirlineCodeResponse = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/passenger:SFPDInfo/@AirlineCode", sfpdRetrieveResponseManager)?.InnerText;

                softAssert.True(sfpdRetrieveAirlineCodeResponse.Is("VS"), $"Expected SFPD Retrieve response true was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveAirlineCodeResponse}");

                string sfpdRetrieveDateOfBirthResponse = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/passenger:SFPDInfo/@DateOfBirth", sfpdRetrieveResponseManager)?.InnerText;

                softAssert.True(sfpdRetrieveDateOfBirthResponse.Equals(DateTime.Parse(dateOfBirth).ToString("yyyy-MM-dd")), $"Expected SFPD Retrieve response {dateOfBirth} was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveDateOfBirthResponse}");

                string sfpdRetrieveFirstNameResponse = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/passenger:SFPDInfo/@FirstName", sfpdRetrieveResponseManager)?.InnerText;

                softAssert.True(sfpdRetrieveFirstNameResponse.Equals(firstName), $"Expected SFPD Retrieve response firstname {firstName} was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveFirstNameResponse}");

                string sfpdRetrieveLastNameResponse = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/passenger:SFPDInfo/@LastName", sfpdRetrieveResponseManager)?.InnerText;

                softAssert.True(sfpdRetrieveLastNameResponse.Equals(lastName), $"Expected SFPD Retrieve response lastname {lastName} was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveLastNameResponse}");

                softAssert.AssertAll();
            }, 5);

            #endregion

        }
    }
}
