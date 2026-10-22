using Azure.Messaging.ServiceBus;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.IntegrationTests.SupplementarySilver
{

    [Collection("IntegrationTests")]
    public class SupplementarySilverDissolveEventTests : IClassFixture<CosmosFixture>
    {

        private readonly CosmosFixture cosmosFixture;

        public static string env = ConfigReader.Get("env");

        private readonly string customerTransactionCosmosDb;

        public SupplementarySilverDissolveEventTests(CosmosFixture cosmosFixture)
        {
            this.cosmosFixture = cosmosFixture;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }


        public static string serviceBusConnectionString = Vault.Default.GetSecretValue("MarketingLoyalty-Test-ServiceBusConnectionString");
        public static string serviceBusTopicName = Application.ServiceBusTopicNameLoyaltyMS;
        public static string cloudEventDate;


        [Trait("Category", "IntegrationTests-Regression")]
        [Fact]
        public async Task SupplementarySilverDissolveOnMemberStatusUpdate_SuccessAsync()
        {

            #region Searching for the Flying Club Member Document for the SSA Member Number

            string queryForFCMDocument = " SELECT Top 1 * FROM CustomerDetailsCollection c where c.DocType='FlyingClubMember'  and c.SSAMemberNumber <> '' order by c._ts desc";

            FlyingClubMember fcmDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            int retry = 5;

            while (retry > 0 && fcmDoc == null)
            {
                await Task.Delay(500);

                fcmDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(fcmDoc is not null, $"No Document is available for the searched Query {queryForFCMDocument}.");

            string ssaMemberNumber = fcmDoc.SSAMemberNumber;
            string initiatedBy = "AutomationScript";
            string childAccountCustomerId = fcmDoc.CustomerId;
            string childDocEtag = fcmDoc._etag;

            #endregion

            #region  Fetching member details from FCM Document of SSA Member

            string queryForFlyingClubDocument = $"SELECT Top 1 * FROM  c where c.DocType='FlyingClubMember' and c.MemberNumber='{ssaMemberNumber}'";

            FlyingClubMember memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

            retry = 5;

            while (retry > 0 && memberProfileDocument == null)
            {
                await Task.Delay(500);

                memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

                retry--;
            }

            string memberType = memberProfileDocument.MemberType;

            string headAccountcustomerId = memberProfileDocument.CustomerId;

            string oldStatus = memberProfileDocument.Status;

            Random random = new();

            //List of new status values for which supplementary silver auto dissolve is expected
            List<string> dissolveStatusList = new()
            { "Inactive", "Fraud", "Active Pending", "Deceased", "Expired", "Cancelled" };

            string newStatus = dissolveStatusList[random.Next(dissolveStatusList.Count)];

            #endregion

            #region Creation of Cloud Event 

            //Household and Supp Silver dissolve event Messages are common as the func is similar 
            HouseholdDissolveMemberOnStatusUpdate dissolveMemberDetails = HouseHoldDissolveEventMapper.HouseholdDissolveMemberDetails(headAccountcustomerId, oldStatus, newStatus, memberType, "", "", initiatedBy);

            CloudEvent cloudEvent = HouseHoldDissolveEventMapper.MapHouseholdDissolveEvent(dissolveMemberDetails, serviceBusTopicName);

            ServiceBusClient serviceBusClient = new(serviceBusConnectionString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusTopicName);

            ServiceBusMessage serviceBusMessage = new
                (System.Text.Json.JsonSerializer.Serialize(cloudEvent))
            { Subject = cloudEvent.Type, CorrelationId = cloudEvent.Id, MessageId = cloudEvent.Id };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            cloudEventDate = Convert.ToDateTime(cloudEvent.Time).ToString("yyyy-MM-dd");

            #endregion

            #region COSMOS validations to verify if the SSA member got delinked

            queryForFlyingClubDocument = $"SELECT Top 1 * FROM  c where c.DocType='FlyingClubMember' and c.PartitionKey='{childAccountCustomerId}'";

            memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

            int retryAgain = 5;

            while (retryAgain > 0 && (memberProfileDocument == null || childDocEtag == memberProfileDocument._etag))
            {
                await Task.Delay(500);

                memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

                retryAgain--;
            }

            if (memberProfileDocument is null || childDocEtag == memberProfileDocument._etag)
            {
                throw new Xunit.Sdk.XunitException($"Document Not available in Cosmos with searched Query  {queryForFlyingClubDocument} or" +
                    $"The SSAMemberNumber is not updated after dissolving.");
            }

            Assert.True(string.IsNullOrEmpty(memberProfileDocument.SSAMemberNumber),
                $"Expected the SSA Member Number value to be empty string but found {memberProfileDocument.SSAMemberNumber}.");


            #endregion



        }

        [Trait("Category", "IntegrationTests-Regression")]

        [Fact]
        public async Task SupplementarySilverDissolveOnTierDowngrade_SuccessAsync()
        {

            #region Searching for the Flying Club Member Document for the SSA Member Number

            string queryForFCMDocument = " SELECT Top 1 * FROM CustomerDetailsCollection c where c.DocType='FlyingClubMember'  and c.SSAMemberNumber <> '' order by c._ts desc";

            FlyingClubMember fcmDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            int retry = 5;

            while (retry > 0 && fcmDoc == null)
            {
                await Task.Delay(500);

                fcmDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(fcmDoc is not null, $"No Document is available for the searched Query {queryForFCMDocument}.");

            string ssaMemberNumber = fcmDoc.SSAMemberNumber;
            string initiatedBy = "AutomationScript";
            string childAccountCustomerId = fcmDoc.CustomerId;
            string childDocEtag = fcmDoc._etag;

            #endregion

            #region  Fetching member details from FCM Document of SSA Member

            string queryForFlyingClubDocument = $"SELECT Top 1 * FROM  c where c.DocType='FlyingClubMember' and c.MemberNumber='{ssaMemberNumber}'";

            FlyingClubMember memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

            retry = 5;

            while (retry > 0 && memberProfileDocument == null)
            {
                await Task.Delay(500);

                memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

                retry--;
            }

            string headAccountcustomerId = memberProfileDocument.CustomerId;

            string oldTier = memberProfileDocument.TierCode;

            Random random = new();

            //dissolve Supp Silver if new tier is Red or Silver
            List<string> dissolveTierList = new()
            { "Red", "Silver"};

            string newTier = dissolveTierList[random.Next(dissolveTierList.Count)];

            #endregion

            #region Creation of Cloud Event 


            SupplementarySilverDissolve dissolveSuppSilverMemberDetails = SupplementarySilverDissolveEventOnTierDowngrade.SupplementarySilverDissolveDetails(headAccountcustomerId, initiatedBy, oldTier, newTier);

            CloudEvent cloudEvent = SupplementarySilverDissolveEventOnTierDowngrade.MapSupplementarySilverDissolveEvent(dissolveSuppSilverMemberDetails, serviceBusTopicName);

            ServiceBusClient serviceBusClient = new(serviceBusConnectionString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusTopicName);

            ServiceBusMessage serviceBusMessage = new
                (System.Text.Json.JsonSerializer.Serialize(cloudEvent))
            { Subject = cloudEvent.Type, CorrelationId = cloudEvent.Id, MessageId = cloudEvent.Id };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            cloudEventDate = Convert.ToDateTime(cloudEvent.Time).ToString("yyyy-MM-dd");

            #endregion

            #region COSMOS validations to verify if the SSA member got delinked

            queryForFlyingClubDocument = $"SELECT Top 1 * FROM  c where c.DocType='FlyingClubMember' and c.PartitionKey='{childAccountCustomerId}'";

            memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

            int retryAgain = 5;

            while (retryAgain > 0 && (memberProfileDocument == null || childDocEtag == memberProfileDocument._etag))
            {
                await Task.Delay(500);

                memberProfileDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFlyingClubDocument);

                retryAgain--;
            }

            if (memberProfileDocument is null || childDocEtag == memberProfileDocument._etag)
            {
                throw new Xunit.Sdk.XunitException($"Document Not available in Cosmos with searched Query  {queryForFlyingClubDocument} or " +
                    $"The SSAMemberNumber is not updated after dissolving.");
            }

            Assert.True(string.IsNullOrEmpty(memberProfileDocument.SSAMemberNumber),
                $"Expected the SSA Member Number value to be empty string but found {memberProfileDocument.SSAMemberNumber}.");


            #endregion



        }

    }
}
