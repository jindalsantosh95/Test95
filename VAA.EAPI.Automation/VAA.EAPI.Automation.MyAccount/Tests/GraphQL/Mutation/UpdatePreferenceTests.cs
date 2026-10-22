using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.GraphQL;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation;
using Application = VAA.EAPI.Automation.MyAccount.Constants.Application;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation
{
[Trait("Category", Traits.MyAccountGQLRegressionTests)]
    public class UpdatePreferenceTests : IClassFixture<CosmosFixture>
    {
        public static readonly string VAAMarketingPreference = "VaaMarketingPermissions";
        public static readonly string OALMarketingPreference = "PreferredPartnerFFP";
        public static readonly string env = Config.Environment;
        public static readonly string LogInPassword = Application.LoginPassword;
        private static readonly string CosmosEndpointCustomerTrxDb = Application.CustomerTransactionCosmosDbEndpoint;
        private static readonly string CosmosEndpointAuditDb = Application.AuditCosmosDbEndpoint;
        private static readonly string customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
        private static readonly string auditCosmosDb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;
        private readonly CosmosFixture _cosmosFixture;

        public UpdatePreferenceTests(CosmosFixture cosmosFixture)
        {
            _cosmosFixture = cosmosFixture;

            _cosmosFixture.RegisterClient(CosmosEndpointAuditDb, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            _cosmosFixture.RegisterClient(CosmosEndpointCustomerTrxDb, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<PreferenceAudit>(CosmosEndpointAuditDb, auditCosmosDb, Application.AuditCosmosCollection);

            _cosmosFixture.RegisterType<CustomerProfile>(CosmosEndpointCustomerTrxDb, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }

        [Trait("Category", Traits.UpdatePreference)]
        [Fact]
        [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
        [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
        [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
        public async Task TC01_VAAMarketingPreferenceUpdate_SuccessScenario()
        {

            (string customerId, string memberNumber) = await EnrolmentHelper.GraphQLEnrolment();

            // Use a member-specific value so parallel tests read only their own Cosmos documents.
            string updatedBy = $"TestScript-{memberNumber}-{Guid.NewGuid():N}";
            string token = await EntraTokenHelper.GetIdToken(env,"",memberNumber, LogInPassword);

            HttpResponseMessage memberDetails = GraphQL.AccountMemberDetails(env, token).EnsureGQLResponseSuccess();

            AccountMemberDetailsResponse accountMemberDetailsResponse = JsonConvert.DeserializeObject<AccountMemberDetailsResponse>(await memberDetails.Content.ReadAsStringAsync());

            bool isVaaMarketingPreferred = accountMemberDetailsResponse.Data.AccountMemberDetails.FlyingClubMemberDetails.IsVaaMarketingPreferred.Value;

            //Preference Value based on Member current Preference Value
            string vaaMarketingPreference = isVaaMarketingPreferred ? "N" : "Y";

            UpdatePreference request = new UpdatePreference
            {
                UpdatePreferenceRequest = new UpdatePreferenceRequest
                {
                    UpdatedBy = updatedBy,
                    Preferences = new[]
                {
                    new Requests.Models.GraphQL.Mutation.Preference
                    {
                        PreferenceType = VAAMarketingPreference,
                        PreferenceValue = vaaMarketingPreference
                    }
                }
                }
            };

            HttpResponseMessage updatePreference = GraphQL.UpdatePreference(env, token, request).EnsureGQLResponseSuccess();

            string content = await updatePreference.Content.ReadAsStringAsync();

            JObject result = JObject.Parse(content);

            bool actualResponse = (bool)result.RequireToken("data.updatePreference");

            bool expectedResponse = true;

            Assert.True(actualResponse == expectedResponse, $"Expected true as response but found {result.ToString()}");

            //Validation of Preference Values in Customer Profile Doc of Customer Txn DB
            //
            CustomerProfile customerProfile = null;

            RetryHelper.RunWithRetry(() =>
            {
                string queryForCustomerProfileDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.LastUpdatedBy = @lastUpdatedBy ";

                QueryDefinition query = new QueryDefinition(query: queryForCustomerProfileDoc).WithParameter("@docType", "CustomerProfile").WithParameter("@customerId", customerId).WithParameter("@lastUpdatedBy", updatedBy);

                customerProfile = _cosmosFixture.CosmosQueryItem<CustomerProfile>(query);

                Assert.True(customerProfile is not null, $"After VAA Preference Update , Customer Profile Document Not Found for {customerId}.");

            }, maxRetries: 5, delay: 1000);

            Core.DB_Model.Entities.Preference preference = customerProfile?.Preferences.FirstOrDefault(x => x.PreferenceType == Application.PreferenceType);

            Assert.True(preference is not null, $"VAA Preference Details in Customer Profile Doc Not Found for {customerId}");

            Assert.True(preference?.PreferenceValue == vaaMarketingPreference, $"For the Customer {customerId} \n VAA Preference Value in Cosmos is not as Expected. \n Expected {vaaMarketingPreference} but found {preference.PreferenceValue}");

            Assert.True(preference?.LastUpdatedBy == updatedBy, $"For the Customer {customerId} \n VAA Preference LastUpdatedBy in Cosmos is not as Expected. \n Expected {updatedBy} but found {preference.LastUpdatedBy}");

            //Validation of Preference Values in Preference Audit Doc of AuditDB
            PreferenceAudit prefAuditDoc = null;

            RetryHelper.RunWithRetry(() =>
            {
                string queryForPreferenceAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.PreferenceUpdatedBy = @updatedBy ";

                QueryDefinition paramQuery = new QueryDefinition(query: queryForPreferenceAuditDoc).WithParameter("@docType", "PreferenceAudit").WithParameter("@customerId", customerId).WithParameter("@updatedBy", updatedBy);

                prefAuditDoc = _cosmosFixture.CosmosQueryItem<PreferenceAudit>(paramQuery);

                Assert.True(prefAuditDoc is not null, $"Preference Audit Document Not Found for {customerId} After VAA Preference Update.");

            }, maxRetries: 5, delay: 1000);

            Assert.True(prefAuditDoc?.VAMarketingPreference == vaaMarketingPreference, $"For the Customer {customerId} \n VAA Preference Value in PrefAuditDoc of Cosmos is not as Expected. \n Expected {vaaMarketingPreference} but found {prefAuditDoc.VAMarketingPreference}");

        }

        [Trait("Category", Traits.UpdatePreference)]
        [Fact]
        [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
        [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
        [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
        public async Task TC02_OALPreferredPartnerPreferenceUpdate_SuccessScenario()
        {

            (string customerId, string memberNumber) = await EnrolmentHelper.GraphQLEnrolment();

            // Use a member-specific value so parallel tests read only their own Cosmos documents.
            string updatedBy = $"TestScript-{memberNumber}-{Guid.NewGuid():N}";
            string token = await EntraTokenHelper.GetIdToken(env, "", memberNumber, LogInPassword);

            HttpResponseMessage memberDetails = GraphQL.AccountMemberDetails(env, token).EnsureGQLResponseSuccess();

            AccountMemberDetailsResponse accountMemberDetailsResponse = JsonConvert.DeserializeObject<AccountMemberDetailsResponse>(await memberDetails.Content.ReadAsStringAsync());

            string currentPreferredPartnerValue = accountMemberDetailsResponse.Data.AccountMemberDetails.FlyingClubMemberDetails.PreferredPartnerFfp;

            //Preference Value based on Member current Preference Value
            string newPreferredPartnerValue = currentPreferredPartnerValue == "LY" ? "SA" : "LY";

            UpdatePreference request = new UpdatePreference
            {
                UpdatePreferenceRequest = new UpdatePreferenceRequest
                {
                    UpdatedBy = updatedBy,
                    Preferences = new[]
                {
                    new Requests.Models.GraphQL.Mutation.Preference
                    {
                        PreferenceType = OALMarketingPreference,
                        PreferenceValue = newPreferredPartnerValue
                    }
                }
                }
            };

            HttpResponseMessage updatePreference = GraphQL.UpdatePreference(env, token, request).EnsureGQLResponseSuccess();

            string content = await updatePreference.Content.ReadAsStringAsync();

            JObject result = JObject.Parse(content);

            bool actualResponse = (bool)result.RequireToken("data.updatePreference");

            bool expectedResponse = true;

            Assert.True(actualResponse == expectedResponse, $"Expected true as response but found {result.ToString()}");

            //Validation of Preference Values in Customer Profile Doc of Customer Txn DB
            CustomerProfile customerProfile = null;

            RetryHelper.RunWithRetry(() =>
            {
                string queryForCustomerProfileDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.LastUpdatedBy = @lastUpdatedBy ";

                QueryDefinition query = new QueryDefinition(query: queryForCustomerProfileDoc).WithParameter("@docType", "CustomerProfile").WithParameter("@customerId", customerId).WithParameter("@lastUpdatedBy", updatedBy);

                customerProfile = _cosmosFixture.CosmosQueryItem<CustomerProfile>(query);

                Assert.True(customerProfile is not null, $"After OAL Preference Update , Customer Profile Document Not Found for {customerId}.");

            }, maxRetries: 5, delay: 1000);

            Core.DB_Model.Entities.Preference preference = customerProfile?.Preferences.FirstOrDefault(x => x.PreferenceType == OALMarketingPreference);

            Assert.True(preference is not null, $"OAL Details in Customer Profile Doc Not Found for {customerId}");

            Assert.True(preference?.PreferenceValue == newPreferredPartnerValue, $"For the Customer {customerId} \n OAL Preference Value in Cosmos is not as Expected. \n Expected {newPreferredPartnerValue} but found {preference.PreferenceValue}");

            Assert.True(preference?.LastUpdatedBy == updatedBy, $"For the Customer {customerId} \n OAL Preference LastUpdatedBy in Cosmos is not as Expected. \n Expected {updatedBy} but found {preference.LastUpdatedBy}");

            //Validation of Preference Values in Preference Audit Doc of AuditDB
            PreferenceAudit prefAuditDoc = null;

            RetryHelper.RunWithRetry(() =>
            {
                string queryForPreferenceAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.PreferenceUpdatedBy = @updatedBy ";

                QueryDefinition paramQuery = new QueryDefinition(query: queryForPreferenceAuditDoc).WithParameter("@docType", "PreferenceAudit").WithParameter("@customerId", customerId).WithParameter("@updatedBy", updatedBy);

                prefAuditDoc = _cosmosFixture.CosmosQueryItem<PreferenceAudit>(paramQuery);

                Assert.True(prefAuditDoc is not null, $"Preference Audit Document Not Found for {customerId} After OAL Preference Update.");

            }, maxRetries: 5, delay: 1000);

            Assert.True(prefAuditDoc?.PreferredPartnerFFP == newPreferredPartnerValue, $"For the Customer {customerId} \n OAL Preference Value in PrefAuditDoc of Cosmos is not as Expected. \n Expected {newPreferredPartnerValue} but found {prefAuditDoc.PreferredPartnerFFP}");

        }

        [Trait("Category", Traits.UpdatePreference)]
        [Fact]
        [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
        [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
        [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
        public async Task TC03_BothVAA_OALPreferredPartnerPreferenceUpdate_SuccessScenario()
        {

            (string customerId, string memberNumber) = await EnrolmentHelper.GraphQLEnrolment();

            // Use a member-specific value so parallel tests read only their own Cosmos documents.
            string updatedBy = $"TestScript-{memberNumber}-{Guid.NewGuid():N}";
            string token = await EntraTokenHelper.GetIdToken(env, "", memberNumber, LogInPassword);

            HttpResponseMessage memberDetails = GraphQL.AccountMemberDetails(env, token).EnsureGQLResponseSuccess();

            AccountMemberDetailsResponse accountMemberDetailsResponse = JsonConvert.DeserializeObject<AccountMemberDetailsResponse>(await memberDetails.Content.ReadAsStringAsync());

            string currentPreferredPartnerValue = accountMemberDetailsResponse.Data.AccountMemberDetails.FlyingClubMemberDetails.PreferredPartnerFfp;

            bool isVaaMarketingPreferred = accountMemberDetailsResponse.Data.AccountMemberDetails.FlyingClubMemberDetails.IsVaaMarketingPreferred.Value;

            //Preference Value based on Member current Preference Value
            string vaaMarketingPreference = isVaaMarketingPreferred ? "N" : "Y";

            //Preference Value based on Member current Preference Value
            string newPreferredPartnerValue = currentPreferredPartnerValue == "LY" ? "SA" : "LY";

            UpdatePreference request = new UpdatePreference
            {
                UpdatePreferenceRequest = new UpdatePreferenceRequest
                {
                    UpdatedBy = updatedBy,
                    Preferences = new[]
                {
                    new Requests.Models.GraphQL.Mutation.Preference
                    {
                        PreferenceType = OALMarketingPreference,
                        PreferenceValue = newPreferredPartnerValue

                    },
                     new Requests.Models.GraphQL.Mutation.Preference
                    {
                        PreferenceType = VAAMarketingPreference,
                        PreferenceValue = vaaMarketingPreference

                    }
                }
                }
            };

            HttpResponseMessage updatePreference = GraphQL.UpdatePreference(env, token, request).EnsureGQLResponseSuccess();

            string content = await updatePreference.Content.ReadAsStringAsync();

            JObject result = JObject.Parse(content);

            bool actualResponse = (bool)result.RequireToken("data.updatePreference");

            bool expectedResponse = true;

            Assert.True(actualResponse == expectedResponse, $"Expected true as response but found {result.ToString()}");

            //Validation of Preference Values in Customer Profile Doc of Customer Txn DB

            CustomerProfile customerProfile = null;

            RetryHelper.RunWithRetry(() =>
            {
                string queryForCustomerProfileDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.LastUpdatedBy = @lastUpdatedBy ";

                QueryDefinition query = new QueryDefinition(query: queryForCustomerProfileDoc).WithParameter("@docType", "CustomerProfile").WithParameter("@customerId", customerId).WithParameter("@lastUpdatedBy", updatedBy);

                customerProfile = _cosmosFixture.CosmosQueryItem<CustomerProfile>(query);

                Assert.True(customerProfile is not null, $"After VAA and OAL Preference Update , Customer Profile Document Not Found for {customerId}.");

            }, maxRetries: 5, delay: 1000);

            Core.DB_Model.Entities.Preference vaaPreference = customerProfile?.Preferences.FirstOrDefault(x => x.PreferenceType == Application.PreferenceType);

            Core.DB_Model.Entities.Preference oalPreference = customerProfile?.Preferences.FirstOrDefault(x => x.PreferenceType == OALMarketingPreference);

            Assert.True(vaaPreference is not null, $"VAA Preference Details in Customer Profile Doc Not Found for {customerId}");

            Assert.True(oalPreference is not null, $"OAL Preference Details in Customer Profile Doc Not Found for {customerId}");

            Assert.True(vaaPreference?.PreferenceValue == vaaMarketingPreference, $"For the Customer {customerId} \n VAA Preference Value in Cosmos is not as Expected. \n Expected {vaaMarketingPreference} but found {vaaPreference.PreferenceValue}");

            Assert.True(vaaPreference?.LastUpdatedBy == updatedBy, $"For the Customer {customerId} \n VAA Preference LastUpdatedBy in Cosmos is not as Expected. \n Expected {updatedBy} but found {vaaPreference.LastUpdatedBy}");

            Assert.True(oalPreference?.PreferenceValue == newPreferredPartnerValue, $"For the Customer {customerId} \n OAL Preference Value in Cosmos is not as Expected. \n Expected {newPreferredPartnerValue} but found {oalPreference.PreferenceValue}");

            Assert.True(oalPreference?.LastUpdatedBy == updatedBy, $"For the Customer {customerId} \n OAL Preference LastUpdatedBy in Cosmos is not as Expected. \n Expected {updatedBy} but found {oalPreference.LastUpdatedBy}");

            //Validation of Preference Values in Preference Audit Doc of AuditDB
            PreferenceAudit prefAuditDoc = null;

            RetryHelper.RunWithRetry(() =>
            {
                string queryForPreferenceAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.PreferenceUpdatedBy = @updatedBy ";

                QueryDefinition paramQuery = new QueryDefinition(query: queryForPreferenceAuditDoc).WithParameter("@docType", "PreferenceAudit").WithParameter("@customerId", customerId).WithParameter("@updatedBy", updatedBy);

                prefAuditDoc = _cosmosFixture.CosmosQueryItem<PreferenceAudit>(paramQuery);

                Assert.True(prefAuditDoc is not null, $"Preference Audit Document Not Found for {customerId} After OAL and VAA Preference Update.");

            }, maxRetries: 5, delay: 1000);

            Assert.True(prefAuditDoc?.PreferredPartnerFFP == newPreferredPartnerValue, $"For the Customer {customerId} \n OAL Preference Value in PrefAuditDoc of Cosmos is not as Expected. \n Expected {newPreferredPartnerValue} but found {prefAuditDoc.PreferredPartnerFFP}");

            Assert.True(prefAuditDoc?.VAMarketingPreference == vaaMarketingPreference, $"For the Customer {customerId} \n VAA Preference Value in PrefAuditDoc of Cosmos is not as Expected. \n Expected {vaaMarketingPreference} but found {prefAuditDoc.VAMarketingPreference}");

        }

        [Trait("Category", Traits.UpdatePreference)]
        [Fact]
        [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
        public async Task TC04_ValidateErrorResponseWhenOALPreferenceValueExceedsThreeCharacters()
        {
            (_, string memberNumber) = await EnrolmentHelper.GraphQLEnrolment();

            // Use a member-specific value so parallel tests read only their own Cosmos documents.
            string updatedBy = $"TestScript-{memberNumber}-{Guid.NewGuid():N}";
            string token = await EntraTokenHelper.GetIdToken(env, "", memberNumber, LogInPassword);

            string newPreferredPartnerValue = "ELAL";

            UpdatePreference request = new UpdatePreference
            {
                UpdatePreferenceRequest = new UpdatePreferenceRequest
                {
                    UpdatedBy = updatedBy,
                    Preferences = new[]
                {
                    new Requests.Models.GraphQL.Mutation.Preference
                    {
                        PreferenceType = OALMarketingPreference,
                        PreferenceValue = newPreferredPartnerValue

                    }
                }
                }
            };

            HttpResponseMessage updatePreference = GraphQL.UpdatePreference(env, token, request).EnsureIsSuccessStatusCode();

            string content = await updatePreference.Content.ReadAsStringAsync();

            JObject result = JObject.Parse(content);

            string expectedResponse = "Invalid Preferred Partner FFP";
            string expectedErrorMessage = "Update Preference failed";
            string expectedErrorCode = "10200";

            string actualErrorMessage = result.RequireToken("errors[0].message").ToString();
            string actualErrorCode = result.RequireToken("errors[0].extensions.code").ToString();
            string actualResponse = result.RequireToken("errors[0].extensions.errors[0]").ToString();

            Assert.True(expectedErrorMessage == actualErrorMessage, $"Expected {expectedErrorMessage} as response but found {actualErrorMessage}");
            Assert.True(expectedErrorCode == actualErrorCode, $"Expected {expectedErrorCode} as response but found {actualErrorCode}");
            Assert.True(actualResponse.Contains(expectedResponse), $"Expected {expectedResponse} as response but found {actualResponse}");

        }

        [Trait("Category", Traits.UpdatePreference)]
        [Fact]
        public async Task TC05_ReturnsValidationErrorWhenPreferencesAreEmpty()
        {
            (string _, string memberNumber) = await EnrolmentHelper.GraphQLEnrolment();

            string token = await EntraTokenHelper.GetIdToken(env, "", memberNumber, LogInPassword);
            UpdatePreference request = new UpdatePreference
            {
                UpdatePreferenceRequest = new UpdatePreferenceRequest
                {
                    UpdatedBy = $"TestScript-{memberNumber}-{Guid.NewGuid():N}",
                    Preferences = Array.Empty<Requests.Models.GraphQL.Mutation.Preference>()
                }
            };

            // An empty preference list is rejected by the mutation before it calls downstream services.
            HttpResponseMessage updatePreference = GraphQL.UpdatePreference(env, token, request).EnsureIsSuccessStatusCode();
            JObject result = JObject.Parse(await updatePreference.Content.ReadAsStringAsync());

            // Validation failures can omit the GraphQL data object entirely, so either no result or a null result proves the mutation did not succeed.
            JToken? updatePreferenceResult = result.SelectToken("data.updatePreference");
            Assert.True(updatePreferenceResult is null or { Type: JTokenType.Null }, $"Expected no updatePreference result for Flying Club member '{memberNumber}' but found {result}.");
            Assert.True(result.RequireToken("errors[0].message").ToString() == "Preferences are required", $"Expected the empty-preferences validation message for Flying Club member '{memberNumber}' but found {result}.");
            Assert.True(result.RequireToken("errors[0].extensions.code").ToString() == "G061", $"Expected G061 for Flying Club member '{memberNumber}' but found {result}.");
            Assert.True(result.RequireToken("errors[0].extensions.Validation").ToString() == "Preferences are required", $"Expected the empty-preferences validation detail for Flying Club member '{memberNumber}' but found {result}.");
        }
    }
}
