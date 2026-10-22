using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests
{
    public class InruleTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly CosmosFixture _cosmosFixture;
        private readonly TokenFixture _tokenFixture;

        public string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        DateTime currentDate = DateTime.UtcNow.Date;

        public InruleTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            _cosmosFixture = cosmosFixture;
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<TargetedMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }


        [Theory]
        [Trait("Category", Traits.InRulePromotionRulesRegression)]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "InRuleMS")]
        [Trait("SharedServiceConsumer", "MarketingLoyalty")]
        [InlineData("Gold After First Flight")]
        [InlineData("Silver After 1st Flight + FTG")]
        [InlineData("Silver After First Flight")]
        public async Task AssignCampaignAndCreateFlightTransaction(string campaignType)
        {

            // To assign the campaign of type SAFF,SAFF+FTG the tier should be Red , for GAFF it can be Red/Silver only
            string tierCode = campaignType.StartsWith("Silver") ? "Red" : campaignType.StartsWith("Gold") ? "Silver" : "Red";

            string queryForCampaignDetails = $"Select TOP 1 CampaignCode,RowId from dbo.Campaigns where Status = 'Active' and StartDate <= '{currentDate}' and EndDate >= '{currentDate.AddDays(+2)}' and Type = '{campaignType}' and RegistrationRequired = 'Y' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignCode = campaignDetails[0][0]?.ToString();

            string campaignId = campaignDetails[0][1]?.ToString();

            Assert.True(!campaignCode.IsNullOrEmpty() && !campaignId.IsNullOrEmpty(), $"Failed to retrieve a valid campaign of type {campaignType}");

            string queryForFCMDoc = "SELECT Top 10 * FROM CustomerDetailsCollection c where c.DocType = @docType  and c.Status = @status and c.TierCode = @tierCode and c.CreatedBy = @createdBy order by c.CreatedOn desc";

            QueryDefinition query = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@status", "Active").WithParameter("@tierCode", tierCode).WithParameter("@createdBy", Application.AutomationScriptCreatedBy);

            IEnumerable<FlyingClubMember> flyingClubMemberDocs = _cosmosFixture.CosmosQueryItems<FlyingClubMember>(query);

            string customerId = string.Empty;

            string memberNumber = string.Empty;

            memberNumber = flyingClubMemberDocs?.FirstOrDefault(x =>
            {
                customerId = x.CustomerId;

                string queryForTargetedMembershipDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

                QueryDefinition campaignQuery = new QueryDefinition(query: queryForTargetedMembershipDoc).WithParameter("@docType", "TargetedMembership").WithParameter("@customerId", customerId);

                IEnumerable<TargetedMembership> targetedMembershipDocs = _cosmosFixture.CosmosQueryItems<TargetedMembership>(campaignQuery);

                return !targetedMembershipDocs.Any();

            })?.MemberNumber;


            HttpResponseMessage response = GraphQL.AssignCampaign(_tokenFixture.GetToken("graphqlv2"), customerId, "", campaignCode, campaignId, Application.AutomationScriptCreatedBy).EnsureIsSuccessStatusCode();

            string assignCampaignResult = await response.Content.ReadAsStringAsync();

            string expectedResponse = "Campaign assign initiated successfully";

            Assert.True(assignCampaignResult.Has(expectedResponse), $"Text value for assign Campaign is not as expected. Expected {expectedResponse} but was {assignCampaignResult}");

            string queryForFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFCMDocument).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

            FlyingClubMember flyingClubMemberDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, $"Failed to retrieve FCM DOC for the memberNumber {memberNumber}.");

            bool isCampaignAssigned = campaignType.StartsWith("Silver") ? !flyingClubMemberDoc.InstantSilverCampaignId.IsNullOrEmpty() : !flyingClubMemberDoc.VaInstantGoldCampaignId.IsNullOrEmpty();

            int retry = 5;

            while (!isCampaignAssigned && retry > 0)
            {
                await Task.Delay(1500);

                flyingClubMemberDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

                isCampaignAssigned = campaignType.StartsWith("Silver") ? !flyingClubMemberDoc.InstantSilverCampaignId.IsNullOrEmpty() : !flyingClubMemberDoc.VaInstantGoldCampaignId.IsNullOrEmpty();

                retry--;
            }

            Assert.True(isCampaignAssigned, $"The ID's are not updated in FCM Doc. The Campaign assignment process was not succesful for the memberNumber {memberNumber}");

            string ticketIssueDate = currentDate.AddDays(-1).ToString("MM/dd/yyyy");

            string activityDate = currentDate.ToString("MM/dd/yyyy");

            string createdOn = currentDate.ToString("MM/dd/yyyy");

            string bookingReference = DataGenerator.RandomString(6, true).ToUpper();

            string ticketNumber = $"93234{DataGenerator.RandomNumber(11111111, 99999999, true)}";

            string flightNumber = DataGenerator.RandomNumber(1, 999, true);

            HttpResponseMessage txnresponse = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(bookingReference, "J", activityDate, "VS", ticketIssueDate, "LHR", flightNumber, "JFK", memberNumber, customerId, createdOn, "Test Transaction", ticketNumber).EnsureIsSuccessStatusCode();

            string txnResult = await txnresponse.Content.ReadAsStringAsync();

            FlightAccrualResponse flightAccrualResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(txnResult);

            Assert.True(flightAccrualResponse?.transactionId is not null, $"The Flight Accrual Transaction Creation failed. The response was {txnResult}");

            string expectedNewTier = campaignType.StartsWith("Silver") ? "Silver" : "Gold";

            retry = 5;

            flyingClubMemberDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            bool isMemberTierUpgraded = flyingClubMemberDoc.TierCode.Equals(expectedNewTier);

            while (!isMemberTierUpgraded && retry > 0)
            {
                await Task.Delay(3000);

                flyingClubMemberDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

                isMemberTierUpgraded = flyingClubMemberDoc.TierCode.Equals(expectedNewTier);

                retry--;

            }

            string memberAttributeName = campaignType.StartsWith("Silver") ? "InstSilverFirstVSFlight" : "InstGoldFirstVSFlight";

            string memberAttributeFlagValue = flyingClubMemberDoc?.MemberAttributes?.FirstOrDefault(x => x.AttributeName == memberAttributeName)?.Value ?? "N";

            Assert.True(isMemberTierUpgraded, $"The Member {memberNumber} Tier is not upgraded to {expectedNewTier} but was {flyingClubMemberDoc.TierCode} \n post the assignment of campaign of type {campaignType}  and a VS Transaction creation. \n The  Member Attribute - {memberAttributeName} flag value is {memberAttributeFlagValue} ");
        }
    }
}
