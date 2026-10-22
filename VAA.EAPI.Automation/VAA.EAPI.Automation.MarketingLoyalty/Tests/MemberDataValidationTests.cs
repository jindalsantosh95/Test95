using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests
{

    public class MemberDataValidationTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture _tokenFixture;

        private readonly CosmosFixture _cosmosFixture;

        private static string env = Config.Environment;

        private ExcelOperations excelOperations;

        private static string ocpSubKey = string.Empty;

        private readonly string customerTransactionCosmosDb = env.Is("Prod") ? Application.CustomerTransactionCosmosDbProd : env.Is("Sit") ? Application.CustomerTransactionCosmosDbSIT : Application.CustomerTransactionCosmosDbUAT;

        private readonly string cosmosEndpointForCustomerTrx = env.Is("Prod") ? Application.CustomerTransactionCosmosDbEndpointProd : Application.CustomerTransactionCosmosDbEndpoint;

        private readonly string currentWorkingDirectory = Path.GetFullPath(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName);

        public MemberDataValidationTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            _tokenFixture = tokenFixture;

            _cosmosFixture = cosmosFixture;

            ocpSubKey = Vault.Default.GetSecretValue("Testing-Ocp-Apim-SubscriptionKey");

            tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public async Task ValidatePointBalance()
        {
            string fileName = string.Empty; //fileName will be added during Local Test Run

            string filePath = $"{currentWorkingDirectory}\\{fileName}";

            using (excelOperations = new ExcelOperations(filePath))
            {

                string sheetName = "Sheet1";

                int noOfRows = excelOperations.GetWorkSheetRowCount(sheetName);

                Assert.True(noOfRows > 0, "No Rows Found in the worksheet");

                for (int i = 2; i <= noOfRows; i++)
                {

                    string memberNumber = excelOperations.ReadData(sheetName, i, 1);

                    string pointBalance = excelOperations.ReadData(sheetName, i, 2);

                    HttpResponseMessage memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                    dynamic result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

                    string memberPointBalance = result?.Usable?.ToString() ?? "";

                    if (memberPointBalance.Is(pointBalance))
                    {
                        excelOperations.WriteData(sheetName, i, 5, "Yes");
                    }
                    else
                    {
                        excelOperations.WriteData(sheetName, i, 5, "No");
                    }

                    excelOperations.WriteData(sheetName, i, 4, memberPointBalance);

                    await Task.Delay(1500); // To Prevent API Overwhelming
                }
            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public async Task ValidateFutureFlights()
        {
            string fileName = string.Empty;

            string filePath = $"{currentWorkingDirectory}\\{fileName}";

            using (excelOperations = new ExcelOperations(filePath))
            {

                string sheetName = "Sheet1";

                int noOfRows = excelOperations.GetWorkSheetRowCount(sheetName);

                Assert.True(noOfRows > 0, "No Rows Found in the worksheet");

                for (int i = 2; i <= noOfRows; i++)
                {

                    string emailAddress = excelOperations.ReadData(sheetName, i, 1);

                    HttpResponseMessage futureFlightsResponse = Microservice.RetrieveFutureFlights(emailAddress).EnsureIsSuccessStatusCode();

                    dynamic result = JsonConvert.DeserializeObject<dynamic>(await futureFlightsResponse?.Content?.ReadAsStringAsync());

                    if (string.IsNullOrEmpty(result?.message?.ToString()))
                    {

                        IEnumerable<FutureFlightResponse> response = JsonConvert.DeserializeObject<FutureFlightResponse[]>(result?.flightBookingsDetails?.ToString() ?? "[]");

                        if (response.Any())
                        {
                            excelOperations.WriteData(sheetName, i, 3, "Yes");
                        }
                        else
                        {
                            excelOperations.WriteData(sheetName, i, 3, "No");
                        }
                    }
                    else
                    {
                        excelOperations.WriteData(sheetName, i, 3, "No");

                        await Task.Delay(100);

                        excelOperations.WriteData(sheetName, i, 4, "Yes");
                    }

                    await Task.Delay(1000);

                }
            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void ValidateTransaction()
        {
            string fileName = string.Empty; //fileName will be added during Local Test Run

            string filePath = $"{currentWorkingDirectory}\\{fileName}";

            using (excelOperations = new ExcelOperations(filePath))
            {

                string sheetName = "Sheet1";

                int noOfRows = excelOperations.GetWorkSheetRowCount(sheetName);

                Assert.True(noOfRows > 0, "No Rows Found in the worksheet");

                for (int i = 2; i <= noOfRows; i++)
                {

                    string memberNumber = excelOperations.ReadData(sheetName, i, 1);

                    string transactionId = excelOperations.ReadData(sheetName, i, 2);

                    string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType  and c.MemberNumber = @memberNumber and c.TransactionId = @transactionId ";

                    QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction").WithParameter("@memberNumber", memberNumber).WithParameter("@transactionId", transactionId);

                    Transaction transaction = _cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

                    if (transaction is not null)
                    {
                        excelOperations.WriteData(sheetName, i, 12, "Yes");
                    }
                    else
                    {
                        excelOperations.WriteData(sheetName, i, 12, "No");
                    }

                    Thread.Sleep(1500);
                }
            }
        }
    }
}


