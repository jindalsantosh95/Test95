using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using Application = VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;
using GlobalTraits = VAA.EAPI.Automation.Core.Constants.GlobalTraits;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.PartnerRetro
{
    public class SkyPartnerRetroTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture _tokenFixture;
        private readonly CosmosFixture _cosmosFixture;

        private Random _random = new();

        private readonly string customerTransactionCosmosDb;
        private readonly string customerCosmosDb;

        private readonly string _env = Config.Environment;

        public SkyPartnerRetroTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            _tokenFixture = tokenFixture;

            _cosmosFixture = cosmosFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerCosmosDb = _env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = _env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            _cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }

        [Trait("Category", GlobalTraits.LocalRun)]
        [Theory, RunOrder(1)]
        [InlineData("LA", "002", "300")]
        public async Task CreateSkyPartnerRetroTransaction_SuccessScenario(string operatedAirlineCode, string operatedFlightNumber, string marketingFlightNumber)
        {

            string customerId = "c1999f24-c558-4828-91ce-7df193c3e480";
            string documentFirstName = "Vinay";
            string documentSurname = "Dhawan";
            string memberNumber = "1078783451";

            string boardingNumber = "12";
            string gdsRecordLocator = "DFDSF";
            string seatNumber = "01";
            string ticketIssueDate = "12-Jan-2024";
            string ticketNumber = "1234123213";

            string marketedCabin = "Business";
            string flightActivityDate = "15-Feb-2024";
            string destinationAirportCode = "LHR";
            string marketingAirlineCode = "VS";
            string marketingBookingClass = "J";
            string operatedbookingClass = "J";
            string originAirportCode = "JFK";

            string codeShareFlag = "Y";
            string vsComments = "Testing Only";
            string productName = Application.FlightProduct;
            string createdBy = Application.AutomationScriptCreatedBy;
            string transactionChannel = Application.PowerAppsTransactionChannel;

            HttpResponseMessage response = GraphQL.SkyPartnerRetroTransactionCreate(_tokenFixture.GetToken("graphqlv2"), customerId, documentFirstName, documentSurname, memberNumber, boardingNumber, gdsRecordLocator, seatNumber, ticketIssueDate, ticketNumber, marketedCabin, flightActivityDate, destinationAirportCode, originAirportCode, marketingAirlineCode, marketingBookingClass, marketingFlightNumber, operatedAirlineCode, operatedFlightNumber, operatedbookingClass, codeShareFlag, vsComments, productName, createdBy, transactionChannel);

            dynamic partnerRetrievalResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string message = "Transaction Created Successfully";

            Assert.True(partnerRetrievalResult is not null, $"The Partner Retrieval response returned with error message{partnerRetrievalResult.ToString()}");

            Assert.True(partnerRetrievalResult.ToString().Contains(message), $"Expected the response message to be {message} but found {partnerRetrievalResult}");
        }

        [Theory, RunOrder(2)]
        [Trait("Category", "SkyTeamRetro-Regression")]
        [InlineData("DocumentFirstName")]
        [InlineData("DocumentSurName")]
        [InlineData("RecordLocator")]
        [InlineData("TicketNumber")]
        [InlineData("OperatedAirlineCode")]
        [InlineData("OperatedFlightNumber")]
        [InlineData("OriginAirportCode")]
        [InlineData("DestinationAirportCode")]
        [InlineData("TransactionChannel")]
        [InlineData("CreatedBy")]
        public async Task SkyPartnerRetroTransaction_MandatoryFieldValidations(string mandatoryField)
        {

            string queryForFlyingClubDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFlyingClubDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember flyingClubDocument = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubDocument is not null, "Failed to Retrieve Test Data .");

            string customerId = flyingClubDocument.CustomerId;

            string memberNumber = flyingClubDocument.MemberNumber;

            string queryForCustomerSearchDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForCustomerSearchDoc).WithParameter("@docType", "CustomerSearch").WithParameter("@partitionKey", flyingClubDocument.MemberNumber);

            CustomerSearch customerSearchDoc = _cosmosFixture.CosmosQueryItem<CustomerSearch>(query);

            Assert.True(customerSearchDoc is not null, "Failed to Retrieve Customer Search Document.");

            string documentFirstName = customerSearchDoc.FirstName;

            string documentSurName = customerSearchDoc.LastName;

            string recordLocator = DataGenerator.RandomString(6, true).ToUpper();

            string seatNumber = DataGenerator.RandomNumber(1, 99, true);

            dynamic activityDate = DataGenerator.RandomPastDate(180);

            while (activityDate > DateTime.UtcNow.Date.AddDays(-7))
            {
                activityDate = DataGenerator.RandomPastDate(180);
            }

            string ticketIssueDate = activityDate.AddDays(-10).ToString("dd-MMM-yyyy");

            activityDate = activityDate.ToString("dd-MMM-yyyy");

            string ticketNumber = DataGenerator.RandomNumber(11111111, 999999999);

            string marketedCabin = Application.CabinClasses[_random.Next(Application.CabinClasses.Length)];

            string originAirportCode = Application.Origins[_random.Next(Application.Origins.Length)];

            string destinationAirportCode = Application.Destinations[_random.Next(Application.Destinations.Length)];

            string operatingAirlineCode = Application.SkyTeamPartnerCodes[_random.Next(Application.SkyTeamPartnerCodes.Length)];

            string marketingAirlineCode = operatingAirlineCode;

            string operatingBookingClass = DataGenerator.RandomString(1).ToUpper();

            string marketingBookingClass = operatingBookingClass;

            string operatingFlightNumber = DataGenerator.RandomNumber(1, 999, true);

            string marketingFlightNumber = operatingFlightNumber;

            string createdBy = Application.AutomationScriptCreatedBy;

            string txnChannel = Application.PowerAppsTransactionChannel;

            string errorMessage = string.Empty;

            switch (mandatoryField)
            {
                case "DocumentFirstName":
                    documentFirstName = string.Empty;
                    errorMessage = "Document First Name' must not be empty";
                    break;
                case "DocumentSurName":
                    documentSurName = string.Empty;
                    errorMessage = "Document Surname' must not be empty";
                    break;
                case "RecordLocator":
                    recordLocator = string.Empty;
                    errorMessage = "Gds Record Locator' must not be empty";
                    break;
                case "TicketNumber":
                    ticketNumber = string.Empty;
                    errorMessage = "Ticket Number' must not be empty";
                    break;
                case "OperatedAirlineCode":
                    operatingAirlineCode = string.Empty;
                    errorMessage = "Operated Airline Code' must not be empty";
                    break;
                case "OperatedFlightNumber":
                    operatingFlightNumber = string.Empty;
                    break;
                case "OriginAirportCode":
                    originAirportCode = string.Empty;
                    errorMessage = "Origin Airport Code' must not be empty";
                    break;
                case "DestinationAirportCode":
                    destinationAirportCode = string.Empty;
                    errorMessage = "Destination Airport Code' must not be empty";
                    break;
                case "TransactionChannel":
                    txnChannel = string.Empty;
                    errorMessage = "'Transaction Channel' must not be empty";
                    break;
                case "CreatedBy":
                    createdBy = string.Empty;
                    errorMessage = "'Created By' must not be empty";
                    break;
                default:
                    break;
            }


            HttpResponseMessage response = GraphQL.SkyPartnerRetroTransactionCreate(_tokenFixture.GetToken("graphqlv2"), customerId, documentFirstName, documentSurName, memberNumber, "", recordLocator, seatNumber, ticketIssueDate, ticketNumber, marketedCabin, activityDate, destinationAirportCode, originAirportCode, marketingAirlineCode, marketingBookingClass, marketingFlightNumber, operatingAirlineCode, operatingFlightNumber, operatingBookingClass, "Y", "Test Transaction", Application.FlightProduct, createdBy, txnChannel
                );

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string actualErrorMessage = result?.errors[0]?.extensions?.validationRule.ToString();

            Assert.True(actualErrorMessage.Has(errorMessage), $"The Error Message is not as expected. \n The expected error message is \n {errorMessage} but the response returned was \n {result}");

        }

    }
}
