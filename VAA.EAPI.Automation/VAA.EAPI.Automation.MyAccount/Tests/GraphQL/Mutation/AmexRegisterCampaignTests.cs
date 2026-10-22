using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation
{
    public class AmexRegisterCampaignTests : IClassFixture<CosmosFixture>, IClassFixture<TableStorageService<AmexPcmUniqueIdTableEntity>>
    {
        public static string env = Config.Environment;

        private readonly CosmosFixture cosmosFixture;
        private readonly string customerTransactionCosmosDb;
        private readonly TableStorageService<AmexPcmUniqueIdTableEntity> amexPcmUniqueIdTableEntityTableStorageService;
        private static string connectionString = Vault.Default.GetSecretValue("mya-avateunstcampaignmgmt-Config-CampaignStorageAccountConnectionString");

        public AmexRegisterCampaignTests(CosmosFixture cosmosFixture, TableStorageService<AmexPcmUniqueIdTableEntity> amexPcmUniqueIdTableEntityTableStorageService)
        {
            this.cosmosFixture = cosmosFixture;
            this.amexPcmUniqueIdTableEntityTableStorageService = amexPcmUniqueIdTableEntityTableStorageService;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<TargetedMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<CampaignEnrollment>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }

        [Fact]
        [Trait("Category", Traits.AmexPlatinumCardCampaignTests)]
        [Trait("Category", Traits.LocalRun)]
        public async Task TC01_RegistersAmexCampaignForANewMember()
        {
            // Select an unclaimed campaign ID so the shared campaign data remains valid for this mutation journey.
            AmexPcmUniqueIdTableEntity unclaimedCampaign = await GetAmexPcmUniqueIdDetails(amexPcmUniqueIdTableEntityTableStorageService, true);
            string uniqueId = unclaimedCampaign.UniqueId;
            Assert.True(!uniqueId.IsNullOrEmpty(), "An unclaimed Amex PCM unique ID is required for this test.");

            // Confirm the selected shared campaign ID is eligible before it is claimed by this test.
            System.Net.Http.HttpResponseMessage eligibilityResponse = GraphQL.ValidatePcmEligibility(env, uniqueId).EnsureGQLResponseSuccess();
            JObject eligibilityResult = JObject.Parse(await eligibilityResponse.Content.ReadAsStringAsync());
            JToken eligibility = eligibilityResult.RequireToken("data.validatePcmEligibility");
            SoftAssertHelper eligibilitySoftAssert = new ();
            eligibilitySoftAssert.True(eligibility.RequireToken("valid").Value<bool>(), $"Expected unique ID '{uniqueId}' to be eligible before registration.");
            eligibilitySoftAssert.True(!eligibility.RequireToken("claimed").Value<bool>(), $"Expected unique ID '{uniqueId}' to be unclaimed before registration.");
            eligibilitySoftAssert.True(eligibility.SelectToken("fcNumber")?.ToString().IsNullOrEmpty() ?? true, $"Expected no Flying Club number for unclaimed unique ID '{uniqueId}'.");
            eligibilitySoftAssert.True(eligibility.SelectToken("claimedDateTime") is null or { Type: JTokenType.Null }, $"Expected no claimed date for unclaimed unique ID '{uniqueId}'.");
            eligibilitySoftAssert.AssertAll();

            // Create an isolated member account for the campaign registration.
            (string emailId, string memberNumber, string customerId, string firstName, string lastName) = await EnrolmentHelper.GraphQLEnrolment("WEB-MYA", "WEB");
            Assert.True(!customerId.IsNullOrEmpty(), $"Customer ID returned from enrolment was empty for Flying Club member '{memberNumber}'.");
            string token = await EntraTokenHelper.GetIdToken(env, "", memberNumber, Application.LoginPassword);
            Assert.True(!token.IsNullOrEmpty(), $"Login token was empty for Flying Club member '{memberNumber}'.");

            // Claim the campaign ID and assign the Amex PCM campaign to the new member.
            string assignedBy = Application.MyAccountChannelId;
            System.Net.Http.HttpResponseMessage registerResponse = GraphQL.AmexRegisterCampaign(env, token, uniqueId, assignedBy).EnsureGQLResponseSuccess();
            JObject registerResult = JObject.Parse(await registerResponse.Content.ReadAsStringAsync());
            Assert.True(registerResult.RequireToken("data.amexRegisterCampaign").ToString().Is("Amex PCM Campaign updated successfully"), $"Expected campaign registration to succeed for Flying Club member '{memberNumber}'.");

            // Retrieve the asynchronously created campaign documents for the enrolled member.
            string queryText = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc";
            QueryDefinition targetedMembershipQuery = new QueryDefinition(queryText).WithParameter("@docType", Application.CustomerTrasactionDBDocTypes.TargetedMembership.ToString()).WithParameter("@customerId", customerId);
            QueryDefinition campaignEnrollmentQuery = new QueryDefinition(queryText).WithParameter("@docType", Application.CustomerTrasactionDBDocTypes.CampaignEnrollment.ToString()).WithParameter("@customerId", customerId);
            TargetedMembership? targetedMembership = null;
            CampaignEnrollment? campaignEnrollment = null;
            RetryHelper.RunWithRetry(() =>
            {
                targetedMembership = cosmosFixture.CosmosQueryItem<TargetedMembership>(targetedMembershipQuery);
                campaignEnrollment = cosmosFixture.CosmosQueryItem<CampaignEnrollment>(campaignEnrollmentQuery);
                Assert.True(targetedMembership is not null, $"TargetedMembership document was not created for Flying Club member '{memberNumber}'.");
                Assert.True(campaignEnrollment is not null, $"CampaignEnrollment document was not created for Flying Club member '{memberNumber}'.");
            }, 5, 2000);

            TargetedMembership validatedTargetedMembership = targetedMembership ?? new TargetedMembership();
            CampaignEnrollment validatedCampaignEnrollment = campaignEnrollment ?? new CampaignEnrollment();
            SoftAssertHelper cosmosSoftAssert = new ();
            cosmosSoftAssert.True(validatedTargetedMembership.CustomerId.Is(customerId), $"Expected TargetedMembership customer ID to be '{customerId}' for Flying Club member '{memberNumber}'.");
            cosmosSoftAssert.True(validatedTargetedMembership.CreatedBy.Is(assignedBy), $"Expected TargetedMembership CreatedBy to be '{assignedBy}' for Flying Club member '{memberNumber}'.");
            cosmosSoftAssert.True(validatedTargetedMembership.ProfilingCampaignSourceId.Is(Application.AmexPcmCampaignId), $"Expected TargetedMembership campaign ID to be '{Application.AmexPcmCampaignId}' for Flying Club member '{memberNumber}'.");
            cosmosSoftAssert.True(validatedCampaignEnrollment.CustomerId.Is(customerId), $"Expected CampaignEnrollment customer ID to be '{customerId}' for Flying Club member '{memberNumber}'.");
            cosmosSoftAssert.True(validatedCampaignEnrollment.MemberNumber.Is(memberNumber), $"Expected CampaignEnrollment member number to be '{memberNumber}'.");
            cosmosSoftAssert.True(validatedCampaignEnrollment.CreatedBy.Is(assignedBy), $"Expected CampaignEnrollment CreatedBy to be '{assignedBy}' for Flying Club member '{memberNumber}'.");
            cosmosSoftAssert.True(validatedCampaignEnrollment.PromotionId.Is(Application.AmexPcmPromotionId), $"Expected CampaignEnrollment promotion ID to be '{Application.AmexPcmPromotionId}' for Flying Club member '{memberNumber}'.");
            cosmosSoftAssert.AssertAll();

            // Confirm the campaign table records the newly claimed ID and member before validating the public eligibility response.
            AmexPcmUniqueIdTableEntity claimedCampaign = await GetAmexPcmUniqueIdDetails(amexPcmUniqueIdTableEntityTableStorageService, false, uniqueId);
            SoftAssertHelper tableStorageSoftAssert = new ();
            tableStorageSoftAssert.True(claimedCampaign.UniqueId.Is(uniqueId), $"Expected campaign storage to retain unique ID '{uniqueId}' for Flying Club member '{memberNumber}'.");
            tableStorageSoftAssert.True(claimedCampaign.FCNumber.Is(memberNumber), $"Expected campaign storage to contain Flying Club member '{memberNumber}' for unique ID '{uniqueId}'.");
            tableStorageSoftAssert.True(!claimedCampaign.UsedTimestamp.IsNullOrEmpty(), $"Expected campaign storage to record a used timestamp for Flying Club member '{memberNumber}'.");
            tableStorageSoftAssert.AssertAll();

            // Validate the published eligibility query reflects the campaign claim after storage has been updated.
            System.Net.Http.HttpResponseMessage claimedEligibilityResponse = GraphQL.ValidatePcmEligibility(env, uniqueId).EnsureGQLResponseSuccess();
            JObject claimedEligibilityResult = JObject.Parse(await claimedEligibilityResponse.Content.ReadAsStringAsync());
            JToken claimedEligibility = claimedEligibilityResult.RequireToken("data.validatePcmEligibility");
            SoftAssertHelper claimedEligibilitySoftAssert = new ();
            claimedEligibilitySoftAssert.True(claimedEligibility.RequireToken("valid").Value<bool>(), $"Expected unique ID '{uniqueId}' to remain valid for Flying Club member '{memberNumber}'.");
            claimedEligibilitySoftAssert.True(claimedEligibility.RequireToken("claimed").Value<bool>(), $"Expected unique ID '{uniqueId}' to be claimed for Flying Club member '{memberNumber}'.");
            claimedEligibilitySoftAssert.True(claimedEligibility.RequireToken("fcNumber").ToString().Is(memberNumber), $"Expected eligibility response to return Flying Club member '{memberNumber}' for unique ID '{uniqueId}'.");
            claimedEligibilitySoftAssert.True(claimedEligibility.SelectToken("claimedDateTime") is not null and not { Type: JTokenType.Null }, $"Expected eligibility response to return a claimed date for Flying Club member '{memberNumber}'.");
            claimedEligibilitySoftAssert.AssertAll();
        }

        private async Task<AmexPcmUniqueIdTableEntity> GetAmexPcmUniqueIdDetails(TableStorageService<AmexPcmUniqueIdTableEntity> tableStorageService, bool unclaimedUniqueId, string? uniqueId = null)
        {
            tableStorageService.ConnectionString = connectionString;
            tableStorageService.TableName = Application.AmexPcmUniqueIdStorageTable;
            System.Collections.Generic.IEnumerable<AmexPcmUniqueIdTableEntity> campaignIds = await tableStorageService.RetrieveItemsAsync(default);
            // Match campaign management: an ID is unclaimed only when both claim fields are empty or the literal "null".
            AmexPcmUniqueIdTableEntity? campaignId = uniqueId.IsNullOrEmpty()
                ? unclaimedUniqueId
                    ? campaignIds.FirstOrDefault(item => (item.FCNumber.IsNullOrWhiteSpace() || item.FCNumber.Is("null")) && (item.UsedTimestamp.IsNullOrWhiteSpace() || item.UsedTimestamp.Is("null")) && !item.UniqueId.IsNullOrWhiteSpace())
                    : campaignIds.FirstOrDefault(item => !item.FCNumber.IsNullOrEmpty() && !item.UniqueId.IsNullOrEmpty())
                : campaignIds.FirstOrDefault(item => item.UniqueId.Is(uniqueId));

            Assert.True(campaignId is not null, uniqueId.IsNullOrEmpty()
                ? $"Expected {(unclaimedUniqueId ? "an unclaimed" : "a claimed")} Amex PCM unique ID in campaign storage."
                : $"Expected Amex PCM unique ID '{uniqueId}' in campaign storage.");

            return campaignId ?? new AmexPcmUniqueIdTableEntity();
        }
    }
}
