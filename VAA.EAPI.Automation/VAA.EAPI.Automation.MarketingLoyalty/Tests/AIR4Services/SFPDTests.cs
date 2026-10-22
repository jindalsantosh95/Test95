using System.Xml;
using Microsoft.Azure.Cosmos;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.AIR4Services
{
    public class SFPDTests : IClassFixture<CertificateFixture>, IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly CertificateFixture _certificateFixture;
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;


        public SFPDTests(CertificateFixture certificateFixture, CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            _certificateFixture = certificateFixture;
            _certificateFixture.ImportCertificate("certificate", Vault.Default, "DlCertificate");

            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }

        [Trait("Category", Traits.Air4ServicesRegression)]
        [Fact]
        public void UpdateSecureFlightData()
        {
            #region PreRequisite to retrieve historical member with no entry in the SFPD 

            string enrolmentDate = DataGenerator.RandomPastDate(20).ToString("yyyy-MM-dd");

            string queryForFlyingClubmemberDoc = "SELECT TOP 1 * FROM CustomerTransaction c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.EnrollmentDate like @enrollmentdate order by c.CreatedOn desc";

            QueryDefinition query = new QueryDefinition(query: queryForFlyingClubmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@enrollmentdate", $"{enrolmentDate}%");

            FlyingClubMember flyingClubDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

            Assert.True(flyingClubDoc is not null, "No flying club documents found. Cannot proceed with test execution");

            string customerId = flyingClubDoc.CustomerId;

            string fcNumber = flyingClubDoc.MemberNumber;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c.CreatedOn desc";

            QueryDefinition queryForCustomerDetails = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer customerDocument = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDetails);

            Assert.True(customerDocument is not null, "No Customer documents found. Cannot proceed with test execution");

            string firstName = customerDocument.FirstName;

            string lastName = customerDocument.LastName;

            string dateOfBirth = customerDocument.DateOfBirth?.ToString("dd-MMM-yyyy");

            string gender = customerDocument.Gender;

            #endregion

            #region AIR4 XML request for DL Call and Validations

            string requestSFPDUpdate;

            XmlDocument air4SFPDUpdateResponseDocument = AIR4Helper.SFPDUpdate(_certificateFixture.GetCertificate("certificate"), "VS", dateOfBirth, firstName, lastName, gender, fcNumber, out requestSFPDUpdate);

            XmlNamespaceManager air4SFPDUpdateResponseManager = air4SFPDUpdateResponseDocument.CreateXmlNamespaceManager();

            string? air4SFPDResponseMessage = air4SFPDUpdateResponseDocument.SelectSingleNode("//passenger:UpdateSFPDResponse/@MQPostingSuccessIndicator", air4SFPDUpdateResponseManager)?.InnerText;

            Assert.True(air4SFPDResponseMessage?.Equals("true"), $"Expected SFPD Update response true was not returned for member number {fcNumber}. Actual xml response returned :  {air4SFPDResponseMessage.ToString()}");

            #endregion

        }

        [Trait("Category", Traits.Air4ServicesRegression)]
        [Fact]
        public void UpdateAndRetrieveSecureFlightData()
        {
            #region PreRequisite to retrieve historical member with no entry in the SFPD 

            string enrolmentDate = DataGenerator.RandomPastDate(20).ToString("yyyy-MM-dd");

            string queryForFlyingClubmemberDoc = "SELECT TOP 1 * FROM CustomerTransaction c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.EnrollmentDate like @enrollmentdate order by c.CreatedOn desc";

            QueryDefinition query = new QueryDefinition(query: queryForFlyingClubmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@enrollmentdate", $"{enrolmentDate}%");

            FlyingClubMember flyingClubDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

            Assert.True(flyingClubDoc is not null, "No flying club documents found. Cannot proceed with test execution");

            string customerId = flyingClubDoc.CustomerId;

            string fcNumber = flyingClubDoc.MemberNumber;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c.CreatedOn desc";

            QueryDefinition queryForCustomerDetails = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer customerDocument = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDetails);

            Assert.True(customerDocument is not null, "No Customer documents found. Cannot proceed with test execution");

            string firstName = customerDocument.FirstName;

            string lastName = customerDocument.LastName;

            string dateOfBirth = customerDocument.DateOfBirth?.ToString("dd-MMM-yyyy");

            string gender = customerDocument.Gender;

            #endregion

            #region AIR4 XML request for DL Call - Update SFPD and Validations

            string requestSFPDUpdate;

            XmlDocument air4SFPDUpdateResponseDocument = AIR4Helper.SFPDUpdate(_certificateFixture.GetCertificate("certificate"), "VS", dateOfBirth, firstName, lastName, gender, fcNumber, out requestSFPDUpdate);

            XmlNamespaceManager air4SFPDUpdateResponseManager = air4SFPDUpdateResponseDocument.CreateXmlNamespaceManager();

            string? air4SFPDResponseMessage = air4SFPDUpdateResponseDocument.SelectSingleNode("//passenger:UpdateSFPDResponse/@MQPostingSuccessIndicator", air4SFPDUpdateResponseManager)?.InnerText;

            Assert.True(air4SFPDResponseMessage?.Equals("true"), $"Expected SFPD Update response true was not returned for member number {fcNumber}. Actual xml response returned :  {air4SFPDResponseMessage.ToString()}");

            #endregion

            #region AIR4 XML request for DL Call - Retrieve SFPD details and Validations

            string requestSFPDRetrieve;

            string transactionId = DataGenerator.RandomNumber(10000, 999999);

            XmlDocument sfpdRetrieveResponseDocument = AIR4Helper.SFPDRetrieve(_certificateFixture.GetCertificate("certificate"), "VS", transactionId, fcNumber, out requestSFPDRetrieve);

            XmlNamespaceManager sfpdRetrieveResponseManager = sfpdRetrieveResponseDocument.CreateXmlNamespaceManager();

            string? sfpdRetrieveResponseInfoMessage = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/@isRecordRetrieved", sfpdRetrieveResponseManager)?.InnerText;

            Assert.True(sfpdRetrieveResponseInfoMessage?.Equals("true"), $"Expected SFPD Retrieve response true was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveResponseInfoMessage.ToString()}");

            string? sfpdRetrieveAirlineCodeResponse = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/passenger:SFPDInfo/@AirlineCode", sfpdRetrieveResponseManager)?.InnerText;

            Assert.True(sfpdRetrieveAirlineCodeResponse?.Equals("VS"), $"Expected SFPD Retrieve response true was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveAirlineCodeResponse.ToString()}");

            string? sfpdRetrieveDateOfBirthResponse = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/passenger:SFPDInfo/@DateOfBirth", sfpdRetrieveResponseManager)?.InnerText;

            Assert.True(sfpdRetrieveDateOfBirthResponse?.Equals(DateTime.Parse(dateOfBirth).ToString("yyyy-MM-dd")), $"Expected SFPD Retrieve response {dateOfBirth} was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveDateOfBirthResponse.ToString()}");

            string? sfpdRetrieveFirstNameResponse = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/passenger:SFPDInfo/@FirstName", sfpdRetrieveResponseManager)?.InnerText;

            Assert.True(sfpdRetrieveFirstNameResponse?.Equals(firstName), $"Expected SFPD Retrieve response firstname {firstName} was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveFirstNameResponse.ToString()}");

            string? sfpdRetrieveLastNameResponse = sfpdRetrieveResponseDocument.SelectSingleNode("//passenger:RetrieveSFPDResponse/passenger:RetrieveSFPDResponseInfo/passenger:SFPDInfo/@LastName", sfpdRetrieveResponseManager)?.InnerText;

            Assert.True(sfpdRetrieveLastNameResponse?.Equals(lastName), $"Expected SFPD Retrieve response lastname {lastName} was not returned for member number {fcNumber}. Actual xml response returned :  {sfpdRetrieveLastNameResponse.ToString()}");

            #endregion
        }
    }
}
