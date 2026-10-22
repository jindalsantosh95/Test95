using Microsoft.Azure.Cosmos;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.ProductionSanity
{
    [Collection("ProductionSanityTests")]
    public class BirthdayBonusTest : IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture _cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb = Application.CustomerTransactionCosmosDbProd;

        private readonly string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpointProd;

        public BirthdayBonusTest(CosmosFixture cosmosFixture)
        {
            _cosmosFixture = cosmosFixture;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }

        [Trait("Category", "BirthdayBonusTest")]
        [Fact]
        public void ValidateBirthdayBonusPointsTransaction()
        {
            SoftAssertHelper softAssert = new();

            DateTime birthDateToRun = DateTime.UtcNow.AddDays(-1).Date;

            IEnumerable<BirthdayBonusSPData> eligibleMembersList = AzureReportingDBHelper.ExecuteBirthdayBonusStoredProcedure(Application.BirthdayBonusStoredProc, birthDateToRun);

            softAssert.True(eligibleMembersList.Any(), $"Birthday Bonus SP Returned empty List for the Date {birthDateToRun:yyyy-MM-dd}");

            string customerId = string.Empty;
            string memberNumber = string.Empty;
            string statusCode = string.Empty;

            foreach (BirthdayBonusSPData member in eligibleMembersList)
            {

                customerId = member.CustomerId;
                memberNumber = member.MemberNumber;
                statusCode = member.StatusCode;

                string queryForTransactionDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType  and c.PartitionKey = @customerId and c.ProductId = @productId and c.TransactionDate > @transactionCreationDate and c.TransactionValue = @transactionValue and c.StatusCode = @statusCode order by c.CreatedOn desc";

                QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", nameof(Application.CustomerTrasactionDBDocTypes.Transaction)).WithParameter("@customerId", customerId).WithParameter("@productId", Application.BirthdayBonusTransactionProductId).WithParameter("@transactionCreationDate", birthDateToRun.ToString("yyyy-MM-dd")).WithParameter("@transactionValue", Application.BirthdayBonusTransactionValue).WithParameter("@statusCode", Application.ProcessedTransactionStatusCode);

                IEnumerable<Transaction> transactionDocs = _cosmosFixture.CosmosQueryItems<Transaction>(parameterizedQuery);

                softAssert.True(transactionDocs.Any(), $"Transaction Doc Not found for the Member : {memberNumber} / Customer Id : {customerId} with Member StatusCode {statusCode} on {birthDateToRun:yyyy-MM-dd} ");

                int noOfBonusTransactions = transactionDocs.Count();

                softAssert.True(noOfBonusTransactions <= 1, $"More Than one : {noOfBonusTransactions} Transaction Doc found for the Member : {memberNumber} / Customer Id : {customerId} with Member StatusCode {statusCode} on {birthDateToRun:yyyy-MM-dd} ");

            }

            softAssert.AssertAll();

        }

    }
}
