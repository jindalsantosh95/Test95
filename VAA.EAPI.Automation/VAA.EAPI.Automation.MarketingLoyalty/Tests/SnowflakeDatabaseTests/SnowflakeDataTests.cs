using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.SnowflakeDatabaseTests
{
    public class SnowflakeDataTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public SnowflakeDataTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
        }

        [Trait("Category", "Local-Run")]
        [Fact(Skip = "Not In Use")]
        public async Task A_SnowflakeConnectivityTest_Success()
        {
            string strQueryForCreditCardData = $"select COUNT(*) AS totalRecords from PROD_VIRGIN_ATLANTIC.VAA_EXPORT.CREDIT_CARD";

            string dbResponse = SnowFlakeConnectivity.SnowflakeQueryResponse(strQueryForCreditCardData).ToString();

            Assert.True(dbResponse is not null, "Snowflake database connectivity test passed");
        }

        [Trait("Category", "Local-Run")]
        [Fact(Skip = "Not In Use")]
        public async Task A_SnowflakeRowCountTest_Success()
        {

            string strQueryForCreditCardData = $"select COUNT(*) AS totalRecords from PROD_VIRGIN_ATLANTIC.VAA_EXPORT.CREDIT_CARD";

            string vredDBRowCount = SnowFlakeConnectivity.SnowflakeQueryResponse(strQueryForCreditCardData).ToString();

            string strQueryForRowCountForVREDRecords = $"Select COUNT(*) AS totalRecords from [STG].[STG_VRED_CREDITCARD_DATA]";

            object[][] vaaReptgDBRowCount = AzureReportingDBHelper.MultiColumnReptgDbQueryResponse(strQueryForRowCountForVREDRecords);

            Assert.True(vaaReptgDBRowCount[0][0] == vredDBRowCount, $"The rowCount at VRed snowflake database is{vredDBRowCount} and VAA Reporting DB end is ${vaaReptgDBRowCount}");
        }
    }
}
