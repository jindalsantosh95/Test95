using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;
using PreferenceAudit = VAA.EAPI.Automation.Core.DB_Model.Entities.PreferenceAudit;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.AuditManagement
{
    [Collection("AuditManagement")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class PreferenceAuditTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture _tokenFixture;
        private readonly CosmosFixture _cosmosFixture;

        private PreRequisiteTestData preRequisiteTestData;

        public string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public PreferenceAuditTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            _tokenFixture = tokenFixture;
            _cosmosFixture = cosmosFixture;

            preRequisiteTestData = new();

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            if (!backendAPIEnv.Is("Project-DEV"))
                _tokenFixture.AddTokenForCustomerApiFromKeyVault("customer");

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<PreferenceAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            _cosmosFixture.RegisterType<CustomerProfile>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(15)]
        public async Task TC01_ValidatePreferenceAuditHistory_SuccessScenario()
        {
            #region Test Data

            string queryForPreferenceAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForPreferenceAuditDoc).WithParameter("@docType", "PreferenceAudit");

            PreferenceAudit prefAuditDoc = _cosmosFixture.CosmosQueryItem<PreferenceAudit>(query);

            Assert.True(prefAuditDoc is not null, "No Preference Audit Document Found.");

            string customerId = prefAuditDoc.CustomerId;

            string queryForCustPreferenceAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustPreferenceAuditDocs).WithParameter("@docType", "PreferenceAudit").WithParameter("@customerId", customerId);

            IEnumerable<PreferenceAudit> custPrefAuditDocs = _cosmosFixture.CosmosQueryItems<PreferenceAudit>(paramQuery);

            custPrefAuditDocs = custPrefAuditDocs.OrderByDescending(x => x.PreferenceUpdatedOn);

            #endregion Test Data

            #region API Call

            HttpResponseMessage response = GraphQL.RetrievePreferenceAudit(_tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve preference audit graphql operation but found {result.ToString()}");
            PreferenceAuditResponse[] preferenceAudits = JsonConvert.DeserializeObject<PreferenceAuditResponse[]>(result?.data.retrievePreferenceAuditHistory.preferenceHistoryAudit.ToString());

            preferenceAudits = preferenceAudits.OrderByDescending(x => x.preferenceUpdatedOn).ToArray();

            #endregion API Call

            #region Validation

            int i = 0;

            foreach (PreferenceAudit preferenceAudit in custPrefAuditDocs)
            {
                Assert.True(preferenceAudit.PreferenceUpdatedBy == preferenceAudits[i].preferenceUpdatedBy,
                   $"Expected the LastUpdatedBy to be {preferenceAudit.PreferenceUpdatedBy} but found {preferenceAudits[i].preferenceUpdatedBy} ");

                Assert.True(preferenceAudit.CreatedBy == preferenceAudits[i].createdBy,
                   $"Expected the CreatedBy to be {preferenceAudit.CreatedBy} but found {preferenceAudits[i].createdBy} ");

                Assert.True(preferenceAudit.PreferenceUpdatedOn == preferenceAudits[i].preferenceUpdatedOn,
                   $"Expected the UpdatedOn to be {preferenceAudit.PreferenceUpdatedOn} but found {preferenceAudits[i].preferenceUpdatedOn} ");

                Assert.True(preferenceAudit.CreatedOn == preferenceAudits[i].createdOn,
                   $"Expected the CreatedOn to be {preferenceAudit.CreatedOn} but found {preferenceAudits[i].createdOn} ");

                Assert.True(preferenceAudit.VAMarketingPreference == preferenceAudits[i].vAMarketingPreference,
                    $"Expected the VAMarketingPreference Value to be {preferenceAudit.VAMarketingPreference} but found {preferenceAudits[i].vAMarketingPreference} ");

                Assert.True(preferenceAudit.DoNotCall == preferenceAudits[i].doNotCall,
                    $"Expected the Do Not Call Value to be {preferenceAudit.DoNotCall} but found {preferenceAudits[i].doNotCall} ");

                Assert.True(preferenceAudit.DoNotSMS == preferenceAudits[i].doNotSMS,
                    $"Expected the Do Not SMS Value to be {preferenceAudit.DoNotSMS} but found {preferenceAudits[i].doNotSMS} ");

                Assert.True(preferenceAudit.DoNotEmail == preferenceAudits[i].doNotEmail,
                    $"Expected the Do Not Email Value to be {preferenceAudit.DoNotEmail} but found {preferenceAudits[i].doNotEmail} ");

                Assert.True(preferenceAudit.PreferredPartnerFFP == preferenceAudits[i].preferredPartnerFFP,
                   $"Expected the PreferredpartnerFFP to be {preferenceAudit.PreferredPartnerFFP} but found {preferenceAudits[i].preferredPartnerFFP} ");

                i++;
            }

            #endregion Validation
        }

        [Trait("Category", "AuditManagementRegression")]
        [Fact, RunOrder(16)]
        public async Task TC02_ValidateAuditDocumentAfterPreferenceUpdate_SuccessScenario()
        {
            #region Test Data

            string queryForCustomerProfileDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.LastUpdatedOn desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerProfileDoc).WithParameter("@docType", "CustomerProfile");

            CustomerProfile customerProfileDoc = _cosmosFixture.CosmosQueryItem<CustomerProfile>(parameterizedQuery);

            Assert.True(customerProfileDoc is not null, "Failed to Retrieve Customer Profile Document for test execution");

            string customerId = customerProfileDoc.CustomerId;

            IEnumerable<Preference> preferences = customerProfileDoc.Preferences;

            string vaMarketingPrefValue = preferences.FirstOrDefault(x => x.PreferenceType == "VAAMarketingPermissions")?.PreferenceValue?.ToUpper() == "N" ? "Y" : "N";

            string doNotCallPrefValue = preferences.FirstOrDefault(x => x.PreferenceType == "DoNotCall")?.PreferenceValue?.ToUpper() == "N" ? "Y" : "N";

            string doNotEmailPrefValue = preferences.FirstOrDefault(x => x.PreferenceType == "DoNotEmail")?.PreferenceValue?.ToUpper() == "N" ? "Y" : "N";

            string doNotSMSPrefValue = preferences.FirstOrDefault(x => x.PreferenceType == "DoNotSMS")?.PreferenceValue?.ToUpper() == "N" ? "Y" : "N";

            string preferredPartnerFFP = preferences.FirstOrDefault(x => x.PreferenceType == "PreferredPartnerFFP")?.PreferenceValue ?? string.Empty;

            string updatedBy = $"TestScript{DataGenerator.RandomString(6).ToLower()}.co.inco";

            #endregion Test Data

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberPreference(_tokenFixture.GetToken("customer"), customerId, vaMarketingPrefValue, doNotCallPrefValue, doNotEmailPrefValue, doNotSMSPrefValue, preferredPartnerFFP, updatedBy).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
               $"Expected a valid response for Update Member Preference API operation but found {result.ToString()}");

            string expectedMessage = "Details have been saved successfully";

            string actualMessage = result?.status?.Value.ToString();

            Assert.True(expectedMessage.Equals(actualMessage), $"Expected the response Message to be {expectedMessage} but found {actualMessage}.");

            #endregion API Call

            #region Customer Profile Doc Validation

            string queryForCustomerProfDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition query = new QueryDefinition(query: queryForCustomerProfDoc).WithParameter("@docType", "CustomerProfile").WithParameter("@partitionKey", customerId);

            CustomerProfile customerProfileDocAfterUpdate = _cosmosFixture.CosmosQueryItem<CustomerProfile>(query);

            IEnumerable<Preference> currentPreferences = customerProfileDocAfterUpdate.Preferences;

            string currentVAMarketingPrefValue = currentPreferences.FirstOrDefault(x => x.PreferenceType == "VAAMarketingPermissions").PreferenceValue;

            string currentDoNotSMSPrefValue = currentPreferences.FirstOrDefault(x => x.PreferenceType == "DoNotSMS").PreferenceValue;

            string currentDoNotCallPrefValue = currentPreferences.FirstOrDefault(x => x.PreferenceType == "DoNotCall").PreferenceValue;

            string currentDoNotEmailPrefValue = currentPreferences.FirstOrDefault(x => x.PreferenceType == "DoNotEmail").PreferenceValue;

            string currentDocPreferredPartnerFFP = currentPreferences.FirstOrDefault(x => x.PreferenceType == "PreferredPartnerFFP")?.PreferenceValue ?? string.Empty;

            string currentDocLastUpdatedBy = customerProfileDocAfterUpdate.LastUpdatedBy;

            string currentDocLastUpdatedOn = customerProfileDocAfterUpdate.LastUpdatedOn.Value.Date.ToString("dd-MMM-yyyy");

            DateTime currentDate = DateTime.UtcNow.Date;

            Assert.True(currentVAMarketingPrefValue == vaMarketingPrefValue,
             $"After the Update in the Customer Profile Document , Expected the VAA Marketing Preference Value to be {vaMarketingPrefValue} but found {currentVAMarketingPrefValue}");

            Assert.True(currentDoNotSMSPrefValue == doNotSMSPrefValue,
             $"After the Update in the Customer Profile Document , Expected the Do Not SMS Preference Value to be {doNotSMSPrefValue} but found {currentDoNotSMSPrefValue}");

            Assert.True(currentDoNotCallPrefValue == doNotCallPrefValue,
             $"After the Update in the Customer Profile Document , Expected the Do Not Call Preference Value to be {doNotCallPrefValue} but found {currentDoNotCallPrefValue}");

            Assert.True(currentDoNotEmailPrefValue == doNotEmailPrefValue,
             $"After the Update in the Customer Profile Document , Expected the Do Not Email Preference Value to be {doNotEmailPrefValue} but found {currentDoNotEmailPrefValue}");

            Assert.True(currentDocPreferredPartnerFFP == preferredPartnerFFP,
             $"After the Update in the Customer Profile Document , Expected the partner Preference Value to be {preferredPartnerFFP} but found {currentDocPreferredPartnerFFP}");

            Assert.True(currentDocLastUpdatedBy == updatedBy,
             $"After the Update in the Customer Profile Document , Expected the Doc LastUpdatedBy to be {updatedBy} but found {currentDocLastUpdatedBy}");

            Assert.True(currentDocLastUpdatedOn == currentDate.ToString("dd-MMM-yyyy"),
             $"After the Update in the Customer Profile Document , Expected the Doc LastUpdatedOn to be {currentDate.ToString("dd-MMM-yyyy")} but found {currentDocLastUpdatedOn}");

            #endregion Customer Profile Doc Validation

            #region Audit DB Validation

            string queryForCustPreferenceAuditDocs = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.PreferenceUpdatedBy = @updatedBy order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustPreferenceAuditDocs).WithParameter("@docType", "PreferenceAudit").WithParameter("@customerId", customerId).WithParameter("@updatedBy", updatedBy);

            PreferenceAudit custPrefAuditDoc = _cosmosFixture.CosmosQueryItem<PreferenceAudit>(paramQuery);

            int retry = 10;

            while (retry > 0 && custPrefAuditDoc is null)
            {
                await Task.Delay(1000);

                custPrefAuditDoc = _cosmosFixture.CosmosQueryItem<PreferenceAudit>(paramQuery);

                retry--;
            }

            Assert.True(custPrefAuditDoc is not null,
               $"For Customer Id {customerId} , Pref Audit Doc not found after retry of 10 seconds");

            Assert.True(custPrefAuditDoc.VAMarketingPreference == vaMarketingPrefValue,
                $" In the Pref Audit Doc ,Expected the VAA Marketing Preference Value to be {vaMarketingPrefValue} but found {custPrefAuditDoc.VAMarketingPreference}");

            Assert.True(custPrefAuditDoc.DoNotCall == doNotCallPrefValue,
                $" In the Pref Audit Doc ,Expected the Do Not Call Preference Value to be {doNotCallPrefValue} but found {custPrefAuditDoc.DoNotCall}");

            Assert.True(custPrefAuditDoc.DoNotEmail == doNotEmailPrefValue,
                $" In the Pref Audit Doc ,Expected the Do Not Email Preference Value to be {doNotEmailPrefValue} but found {custPrefAuditDoc.DoNotEmail}");

            Assert.True(custPrefAuditDoc.DoNotSMS == doNotSMSPrefValue,
                $" In the Pref Audit Doc ,Expected the Do Not SMS Preference Value to be {doNotSMSPrefValue} but found {custPrefAuditDoc.DoNotSMS}");

            Assert.True(custPrefAuditDoc.PreferredPartnerFFP == preferredPartnerFFP,
               $" In the Pref Audit Doc ,Expected the Partner Preference Value to be {preferredPartnerFFP} but found {custPrefAuditDoc.PreferredPartnerFFP}");

            Assert.True(custPrefAuditDoc.CreatedBy == updatedBy,
                $" In the Pref Audit Doc ,Expected the CreatedBy Value to be {updatedBy} but found {custPrefAuditDoc.CreatedBy}");

            Assert.True(custPrefAuditDoc.PreferenceUpdatedBy == updatedBy,
                $" In the Pref Audit Doc ,Expected the Preference Updated By Value to be {updatedBy} but found {custPrefAuditDoc.PreferenceUpdatedBy}");

            Assert.True(custPrefAuditDoc.CreatedOn.Value.ToString("dd-MMM-yyyy") == currentDate.ToString("dd-MMM-yyyy"),
                $" In the Pref Audit Doc ,Expected the CreatedOn Value to be {currentDate.ToString("dd-MMM-yyyy")} but found {custPrefAuditDoc.CreatedOn.Value.ToString("dd-MMM-yyyy")}");

            Assert.True(custPrefAuditDoc.PreferenceUpdatedOn.Value.ToString("dd-MMM-yyyy") == currentDate.ToString("dd-MMM-yyyy"),
                $" In the Pref Audit Doc ,Expected the PreferenceUpdatedOn Value to be {currentDate.ToString("dd-MMM-yyyy")} but found {custPrefAuditDoc.PreferenceUpdatedOn.Value.ToString("dd-MMM-yyyy")}");

            #endregion Audit DB Validation
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(14)]
        public async Task TC03_ValidateCustomerPreferenceAuditHistoryForNewEnrolment_SuccessScenario()
        {
            #region PreRequisite to create an enrolment before validations for the enrolment

            string[] titles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
            Random random = new();
            string title = titles[random.Next(titles.Length)];
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string[] genders = new string[] { "M", "F", "U", "X" };
            string gender = genders[random.Next(genders.Length)];
            string emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";
            string postCode = "RH11 9LG";
            string country = "GB";
            string channelID = "POWERAPP";
            string enroledBy = DataGenerator.RandomFirstName();
            string[] preferredPartnerFFPList = new string[] { "VS", "NZ", "NH", "DL", "SK", "SA", "VA", "GA", "KQ", "AR", "6E", "ME", "LA", "AF", "KL", "WS", "UX", "SV", "AM", "RO", "KE", "MF", "MU", "CI", "VN", "LY", "FM" };
            string preferredPartnerFFP = preferredPartnerFFPList[random.Next(preferredPartnerFFPList.Length)];

            string dateOfBirth = DateTime.UtcNow.AddDays(-6600).ToString("yyyy-MM-dd");
            (string memberNumber, string customerId) = await preRequisiteTestData.MemberEnrolment(channelID, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy, gender, preferredPartnerFFP);
            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion PreRequisite to create an enrolment before validations for the enrolment

            #region Retrieving data from cosmos collection

            string queryForPreferenceAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForPreferenceAuditDocs).WithParameter("@docType", "PreferenceAudit").WithParameter("@customerId", customerId);

            IEnumerable<PreferenceAudit> prefAuditCosmosDocs = _cosmosFixture.CosmosQueryItems<PreferenceAudit>(paramQuery).OrderByDescending(x => x.CreatedOn);

            int retry = 5;

            while (!prefAuditCosmosDocs.Any() && retry > 0)
            {
                prefAuditCosmosDocs = _cosmosFixture.CosmosQueryItems<PreferenceAudit>(paramQuery).OrderByDescending(x => x.CreatedOn);

                await Task.Delay(2000);

                retry--;
            }

            Assert.True(prefAuditCosmosDocs.Any(),
                $"After the Enrolment ,Failed to Retrieve Preference Audit Document after retry of 10 seconds for the customer {customerId}");

            Assert.True(prefAuditCosmosDocs.Count() == 1,
                $"Expected only one Preference Audit Document after the enrolment but found {prefAuditCosmosDocs.Count()}.");

            #endregion Retrieving data from cosmos collection

            #region API Call

            HttpResponseMessage response = GraphQL.RetrievePreferenceAudit(_tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve preference audit graphql operation but found {result.ToString()}");

            PreferenceAuditResponse[] preferenceAuditDocs = JsonConvert.DeserializeObject<PreferenceAuditResponse[]>(result?.data.retrievePreferenceAuditHistory.preferenceHistoryAudit.ToString());

            preferenceAuditDocs = preferenceAuditDocs.OrderByDescending(x => x.createdOn).ToArray();

            Assert.True(preferenceAuditDocs.Count() == 1,
                $"Expected only one Preference Audit in the GraphQL Response after the enrolment but found {preferenceAuditDocs.Count()} ");

            #endregion API Call

            #region Validation

            PreferenceAudit preferenceAudit = prefAuditCosmosDocs.First();

            DateTime currentDate = DateTime.UtcNow.Date;

            Assert.True(preferenceAudit.CreatedBy == enroledBy,
                $"In the Preference Audit Doc, Expected the  CreatedBy to be {enroledBy} but found {preferenceAudit.CreatedBy} ");

            Assert.True(preferenceAudit.CreatedOn?.ToString("dd-MMM-yyyy") == currentDate.ToString("dd-MMM-yyyy"),
                $"In thePreference Audit Doc, Expected the CreatedOn to be {currentDate.ToString("dd-MMM-yyyy")} but found {preferenceAudit.CreatedOn?.ToString("dd-MMM-yyyy")} ");

            Assert.True(preferenceAudit.VAMarketingPreference == "Y",
               $"In the Preference Audit Doc, Expected the VA Marketing Preference value to be Y but found {preferenceAudit.VAMarketingPreference} ");

            Assert.True(preferenceAudit.DoNotCall == "N",
               $"In the Preference Audit Doc, Expected the DO NOT CALL Value to be N but found {preferenceAudit.DoNotCall} ");

            Assert.True(preferenceAudit.DoNotEmail == "N",
               $"In the Preference Audit Doc, Expected the DO NOT Email Value to be N but found {preferenceAudit.DoNotEmail} ");

            Assert.True(preferenceAudit.DoNotSMS == "N",
               $"In the Preference Audit Doc, Expected the DO NOT SMS Value to be N but found {preferenceAudit.DoNotSMS} ");

            Assert.True(preferenceAudit.PreferredPartnerFFP == preferredPartnerFFP,
                 $"In the Preference Audit Doc, Expected the preferredPartnerFFP Value to be {preferredPartnerFFP} but found {preferenceAudit.PreferredPartnerFFP} ");

            int i = 0;

            string docId = preferenceAudit.Id;

            Assert.True(preferenceAudit.CreatedBy == preferenceAuditDocs[i].createdBy,
                $"For the Doc Id {docId} ,Expected the Preference Audit Doc CreatedBy to be {preferenceAudit.CreatedBy} but found {preferenceAuditDocs[i].createdBy} ");

            Assert.True(preferenceAudit.CreatedOn?.ToString("dd-MMM-yyyy") == preferenceAuditDocs[i].createdOn?.ToString("dd-MMM-yyyy"),
                $"For the Doc Id {docId} ,Expected the Preference Audit Doc CreatedOn to be {preferenceAudit.CreatedOn?.ToString("dd-MMM-yyyy")} but found {preferenceAuditDocs[i].createdOn?.ToString("dd-MMM-yyyy")} ");

            Assert.True(preferenceAudit.PreferenceUpdatedOn?.ToString("dd-MMM-yyyy") == preferenceAuditDocs[i].preferenceUpdatedOn?.ToString("dd-MMM-yyyy"),
                $"For the Doc Id {docId} ,Expected the Preference Audit Doc CreatedOn to be {preferenceAudit.PreferenceUpdatedOn?.ToString("dd-MMM-yyyy")} but found {preferenceAuditDocs[i].preferenceUpdatedOn?.ToString("dd-MMM-yyyy")} ");

            Assert.True(preferenceAudit.PreferenceUpdatedBy == preferenceAuditDocs[i].preferenceUpdatedBy,
                $"For the Doc Id {docId} ,Expected the Preference Audit Doc CreatedBy to be {preferenceAudit.CreatedBy} but found {preferenceAuditDocs[i].preferenceUpdatedBy} ");

            Assert.True(preferenceAudit.VAMarketingPreference == preferenceAuditDocs[i].vAMarketingPreference,
                $"For the Doc Id {docId} ,Expected the Preference Audit Doc VAMarketingPreference Value to be {preferenceAudit.VAMarketingPreference} but found {preferenceAuditDocs[i].vAMarketingPreference}");

            Assert.True(preferenceAudit.DoNotSMS == preferenceAuditDocs[i].doNotSMS,
               $"For the Doc Id {docId} ,Expected the Preference Audit Doc DO NOT SMS Value to be {preferenceAudit.DoNotSMS} but found {preferenceAuditDocs[i].doNotSMS}");

            Assert.True(preferenceAudit.DoNotEmail == preferenceAuditDocs[i].doNotEmail,
               $"For the Doc Id {docId} ,Expected the Preference Audit Doc DO NOT EMail Value to be {preferenceAudit.DoNotEmail} but found {preferenceAuditDocs[i].doNotEmail}");

            Assert.True(preferenceAudit.DoNotCall == preferenceAuditDocs[i].doNotCall,
               $"For the Doc Id {docId} ,Expected the Preference Audit Doc VAMarketingPreference Value to be {preferenceAudit.DoNotCall} but found {preferenceAuditDocs[i].doNotCall}");

            Assert.True(preferenceAudit.PreferredPartnerFFP == preferenceAuditDocs[i].preferredPartnerFFP,
             $"For the Doc Id {docId} ,Expected the Preference Audit Doc PreferredPartnerFFP Value to be {preferenceAudit.PreferredPartnerFFP} but found {preferenceAuditDocs[i].preferredPartnerFFP}");

            #endregion Validation
        }
    }
}