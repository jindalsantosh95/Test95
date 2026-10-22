using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using Application = VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.Customer
{
    [Collection("UpdateMemberProfileTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class UpdateMemberProfileTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;
        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public UpdateMemberProfileTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerMSFromKeyVault("customer");

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<ContactDetails>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<ContactHistory>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerMSSubscriptionKey");

        }

        [Trait("Category", Traits.CustomerMSRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_UpdateMemberAddress__SuccessScenario()
        {

            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.EnrollmentChannel <> @channel order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "Customer").WithParameter("@channel", "WEB");

            Core.DB_Model.Entities.Customer fcMember = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

            Assert.True(fcMember is not null, "Failed to retrieve the Test Data");

            string customerId = fcMember.CustomerId;

            string memberNumber = fcMember.LoyaltyMemberNumber;

            string currentAddressType = fcMember.AddressDetails[0].AddressType;

            string newAddressLine1 = "testing member address update sceanrio";

            string currentAddressLine2 = fcMember.AddressDetails[0].AddressLine2;

            string currentAddressLine3 = fcMember.AddressDetails[0].AddressLine3;

            string currentAddressVerified = fcMember.AddressDetails[0].AddressVerified;

            string currentLandlordOrCompanyName = fcMember.AddressDetails[0].LandlordOrCompanyName;

            string currentCity = fcMember.AddressDetails[0].City;

            string currentPostalCode = fcMember.AddressDetails[0].PostCode;

            string currentRegion = fcMember.AddressDetails[0].Province;

            string currentCountry = fcMember.AddressDetails[0].Country;

            string currentPreferredFlag = fcMember.AddressDetails[0].Primary;


            #endregion

            #region API Call

            HttpResponseMessage response = Microservice.UpdateMemberAddress(tokenFixture.GetToken("customer"), ocpSubKey, customerId, currentAddressType, newAddressLine1, currentAddressLine2, currentAddressLine3, currentAddressVerified, currentLandlordOrCompanyName, currentCity, currentPostalCode, currentRegion, currentCountry, currentPreferredFlag).EnsureIsAcceptedStatusCode();

            string responseUrl = response?.Headers?.Location?.ToString();

            HttpResponseMessage actualUpdateMemberResponse = Microservice.GetUpdateMemberAddress(tokenFixture.GetToken("customer"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualUpdateMemberResponse?.Content?.ReadAsStringAsync();

            dynamic updateMemberAddressResponse = JsonConvert.DeserializeObject<dynamic>(content);

            Assert.True(!updateMemberAddressResponse.ToString().Contains("Request failed"), $"Expected a valid response for update member address operation but found {updateMemberAddressResponse.ToString()}");

            Assert.True(updateMemberAddressResponse.ToString().Contains("Updated Member Address Successfully"), $"Expected a valid success response for update member address operation but found {updateMemberAddressResponse.ToString()}");

            #endregion

            #region Validations
            string cosmosQuery = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c._ts desc ";

            QueryDefinition parameterizedQueryAfterUpdate = new QueryDefinition(query: cosmosQuery).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

            Assert.True(fcMemberPostUpdate is not null, "Failed to retrieve the Test Data");

            Assert.True(fcMemberPostUpdate.AddressDetails[0].AddressLine1.Equals(newAddressLine1), $"Expected Address line 1 in cosmos is {newAddressLine1} but found {fcMemberPostUpdate.AddressDetails[0].AddressLine1} ");

            Assert.True((fcMemberPostUpdate.AddressDetails?[0]?.AddressLine2 ?? "") == (currentAddressLine2 ?? ""), $"Expected Address line 2 verified in cosmos is {currentAddressLine2 ?? "null"} but found {fcMemberPostUpdate.AddressDetails?[0]?.AddressLine2 ?? "null"}");

            Assert.True((fcMemberPostUpdate.AddressDetails?[0]?.AddressLine3 ?? "") == (currentAddressLine3 ?? ""), $"Expected Address verified in cosmos is {currentAddressLine3 ?? "null"} but found {fcMemberPostUpdate.AddressDetails?[0]?.AddressLine3 ?? "null"}");

            Assert.True((fcMemberPostUpdate.AddressDetails?[0]?.AddressVerified ?? "") == (currentAddressVerified ?? ""), $"Expected Address verified in cosmos is {currentAddressVerified ?? "null"} but found {fcMemberPostUpdate.AddressDetails?[0]?.AddressVerified ?? "null"}");

            Assert.True((fcMemberPostUpdate.AddressDetails?[0]?.PostCode ?? "") == (currentPostalCode ?? ""), $"Expected Address verified in cosmos is {currentPostalCode ?? "null"} but found {fcMemberPostUpdate.AddressDetails?[0]?.PostCode ?? "null"}");

            #endregion

        }

        [Trait("Category", Traits.CustomerMSRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_UpdateMemberEmail_SuccessScenario()
        {

            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.EnrollmentChannel <> @channel order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "Customer").WithParameter("@channel", "WEB");

            Core.DB_Model.Entities.Customer fcMember = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

            Assert.True(fcMember is not null, "Failed to retrieve the Test Data");

            string customerId = fcMember.CustomerId;

            string memberNumber = fcMember.LoyaltyMemberNumber;

            string currentEmailAddress = fcMember.ContactDetails.Where(contact => contact.ContactType.Equals("Email", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

            string newEmailAddress = DataGenerator.RandomString(15, false) + "@noreply.com";

            #endregion

            #region API Call

            HttpResponseMessage response = Microservice.UpdateMemberEmail(tokenFixture.GetToken("customer"), ocpSubKey, memberNumber, newEmailAddress).EnsureIsSuccessStatusCode();

            string content = await response?.Content?.ReadAsStringAsync();

            dynamic updateMemberEmailResponse = JsonConvert.DeserializeObject<dynamic>(content);

            Assert.True(!updateMemberEmailResponse.ToString().Contains("Request failed"), $"Expected a valid response for update member email operation but found {updateMemberEmailResponse.ToString()}");

            Assert.True(updateMemberEmailResponse.ToString().Contains("Email Update Successful"), $"Expected a valid success response for update member email operation but found {updateMemberEmailResponse.ToString()}");

            #endregion

            #region Validations
            string cosmosQueryAfterUpdate = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c._ts desc ";

            QueryDefinition parameterizedQueryAfterUpdate = new QueryDefinition(query: cosmosQueryAfterUpdate).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

            Assert.True(fcMemberPostUpdate is not null, "Failed to retrieve the Test Data");

            string emailAddressPostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Email", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

            int retryCount = 5;

            while (retryCount > 0 && (!emailAddressPostUpdate.Equals(currentEmailAddress, StringComparison.OrdinalIgnoreCase)))
            {

                await Task.Delay(2000);

                fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

                emailAddressPostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Email", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

                retryCount--;

            }

            Assert.True(emailAddressPostUpdate.Equals(newEmailAddress), $"Expected Email in cosmos is {newEmailAddress} but found {emailAddressPostUpdate.ToString()} ");

            #endregion

        }

        [Trait("Category", Traits.CustomerMSRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_UpdateMemberPhone_SuccessScenario()
        {

            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.EnrollmentChannel <> @channel order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "Customer").WithParameter("@channel", "WEB");

            Core.DB_Model.Entities.Customer fcMember = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

            Assert.True(fcMember is not null, "Failed to retrieve the Test Data");

            string customerId = fcMember.CustomerId;

            string memberNumber = fcMember.LoyaltyMemberNumber;

            string currentPhoneNumber = fcMember.ContactDetails.Where(contact => contact.ContactType.Equals("Phone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

            string newPhoneNumber = DataGenerator.RandomNumber(99900000, 99999999, true).ToString();

            string countryCode = "44";

            #endregion

            #region API Call

            HttpResponseMessage response = Microservice.UpdateMemberPhone(tokenFixture.GetToken("customer"), ocpSubKey, customerId, memberNumber, newPhoneNumber, countryCode).EnsureIsAcceptedStatusCode();

            string responseUrl = response?.Headers?.Location?.ToString();

            HttpResponseMessage actualUpdateMemberPhoneResponse = Microservice.GetUpdateMemberPhone(tokenFixture.GetToken("customer"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualUpdateMemberPhoneResponse?.Content?.ReadAsStringAsync();

            dynamic updateMemberPhoneResponse = JsonConvert.DeserializeObject<dynamic>(content);

            Assert.True(!updateMemberPhoneResponse.ToString().Contains("Request failed"), $"Expected a valid response for update member phone operation but found {updateMemberPhoneResponse.ToString()}");

            Assert.True(updateMemberPhoneResponse.ToString().Contains("Updated Phone Number Successfully"), $"Expected a valid success response for update member address operation but found {updateMemberPhoneResponse.ToString()}");

            #endregion

            #region Validations
            string cosmosQueryAfterUpdate = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c._ts desc ";

            QueryDefinition parameterizedQueryAfterUpdate = new QueryDefinition(query: cosmosQueryAfterUpdate).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

            Assert.True(fcMemberPostUpdate is not null, "Failed to retrieve the Test Data");

            string phoneNumberPostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Phone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

            string countryCodePostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Phone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.CountryCode;

            int retryCount = 5;

            while (retryCount > 0 && (!phoneNumberPostUpdate.Equals(newPhoneNumber, StringComparison.OrdinalIgnoreCase)))
            {

                await Task.Delay(2000);

                fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

                phoneNumberPostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Phone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

                countryCodePostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Phone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.CountryCode;

                retryCount--;

            }

            Assert.True(phoneNumberPostUpdate.Equals(newPhoneNumber), $"Expected Phone number in cosmos is {newPhoneNumber} but found {phoneNumberPostUpdate.ToString()} ");

            Assert.True(countryCodePostUpdate.Equals("44"), $"Expected country code in cosmos is {"44"} but found {countryCodePostUpdate.ToString()}");

            #endregion

        }


        [Trait("Category", Traits.CustomerMSRegression)]
        [Theory, RunOrder(4)]
        [InlineData("EmailUpdate")]
        [InlineData("PhoneUpdate")]
        [InlineData("DobUpdate")]
        [InlineData("FirstNameLastNameUpdate")]
        public async Task TC04_UpdateVAC_SuccessScenario(string UpdateScenarioType)
        {

            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.TierCode = @tierCode order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "VAC");

            FlyingClubMember fcMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcMember is not null, "Failed to retrieve the Test Data");

            string customerId = fcMember.CustomerId;

            string memberNumber = fcMember.MemberNumber;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.CustomerId= @customerId ";

            QueryDefinition parameterizedCustomerDocQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer fcCustomer = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedCustomerDocQuery);

            string currentPhoneNumber = fcCustomer.ContactDetails.Where(contact => contact.ContactType.Equals("Phone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

            string currentEmailAddress = fcCustomer.ContactDetails.Where(contact => contact.ContactType.Equals("Email", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

            string firstName = fcCustomer.FirstName;

            string lastName = fcCustomer.LastName;

            string doB = fcCustomer.DateOfBirth?.ToString("yyyy-MM-dd");

            string newDob = String.Empty;
            string newEmailAddress = String.Empty;
            string newPhoneNumber = String.Empty;
            string newFirstName = String.Empty;
            string newLastName = String.Empty;


            #endregion

            #region for different scenarios based on input parameter and API call

            HttpResponseMessage response = default(HttpResponseMessage);

            if (UpdateScenarioType.Equals("EmailUpdate"))
            {

                newEmailAddress = DataGenerator.RandomString(15, false) + "@noreply.com";

                response = Microservice.UpdateVACMemberProfile(tokenFixture.GetToken("customer"), ocpSubKey, customerId, memberNumber, firstName, lastName, doB, newEmailAddress, currentPhoneNumber).EnsureIsSuccessStatusCode();

            }
            else if (UpdateScenarioType.Equals("PhoneUpdate"))
            {
                newPhoneNumber = DataGenerator.RandomNumber(1999900000, 1999999999, true).ToString();

                response = Microservice.UpdateVACMemberProfile(tokenFixture.GetToken("customer"), ocpSubKey, customerId, memberNumber, firstName, lastName, doB, currentEmailAddress, newPhoneNumber).EnsureIsSuccessStatusCode();
            }
            else if (UpdateScenarioType.Equals("DobUpdate"))
            {
                newDob = DateTime.Now.AddYears(-19).ToString("yyyy-MM-dd");

                response = Microservice.UpdateVACMemberProfile(tokenFixture.GetToken("customer"), ocpSubKey, customerId, memberNumber, firstName, lastName, newDob, currentEmailAddress, currentPhoneNumber).EnsureIsSuccessStatusCode();

            }
            else if (UpdateScenarioType.Equals("FirstNameLastNameUpdate"))
            {
                newFirstName = DataGenerator.RandomString(10, false);
                newLastName = DataGenerator.RandomString(10, false);

                response = Microservice.UpdateVACMemberProfile(tokenFixture.GetToken("customer"), ocpSubKey, customerId, memberNumber, newFirstName, newLastName, doB, currentEmailAddress, currentPhoneNumber).EnsureIsSuccessStatusCode();

            }

            string updateResponseContent = await response?.Content?.ReadAsStringAsync();

            Assert.True(updateResponseContent.ToString().Contains("Successfully updated"), $"Expected a valid response for update member email operation but found {updateResponseContent.ToString()}");

            #endregion

            #region Validations
            string cosmosQueryAfterUpdate = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c._ts desc ";

            QueryDefinition parameterizedQueryAfterUpdate = new QueryDefinition(query: cosmosQueryAfterUpdate).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

            Assert.True(fcMemberPostUpdate is not null, "Failed to retrieve the Test Data");

            int retryCount = 5;

            if (UpdateScenarioType == "EmailUpdate")
            {

                string emailAddressPostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Email", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

                while (retryCount > 0 && emailAddressPostUpdate.Equals(currentEmailAddress, StringComparison.OrdinalIgnoreCase))
                {

                    await Task.Delay(2000);

                    fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

                    emailAddressPostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Email", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

                    retryCount--;

                }

                Assert.True(emailAddressPostUpdate.Equals(newEmailAddress), $"Expected Email in cosmos is {newEmailAddress} but found {emailAddressPostUpdate.ToString()} ");

            }
            else if (UpdateScenarioType == "PhoneUpdate")
            {

                string phoneNumberPostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Phone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

                while (retryCount > 0 && (!phoneNumberPostUpdate.Equals(currentPhoneNumber, StringComparison.OrdinalIgnoreCase)))
                {

                    await Task.Delay(2000);

                    fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

                    phoneNumberPostUpdate = fcMemberPostUpdate.ContactDetails.Where(contact => contact.ContactType.Equals("Phone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

                    retryCount--;

                }

                Assert.True(phoneNumberPostUpdate.Equals(newPhoneNumber), $"Expected Phone number in cosmos is {currentPhoneNumber} but found {phoneNumberPostUpdate.ToString()} ");

            }

            else if (UpdateScenarioType == "DobUpdate")
            {

                string dobPostUpdate = fcMemberPostUpdate.DateOfBirth?.ToString("yyyy-MM-dd");

                while (retryCount > 0 && dobPostUpdate.Equals(doB, StringComparison.OrdinalIgnoreCase))
                {

                    await Task.Delay(2000);

                    fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

                    dobPostUpdate = fcMemberPostUpdate.DateOfBirth?.ToString("yyyy-MM-dd");

                    retryCount--;

                }

                Assert.True(dobPostUpdate.Equals(newDob), $"Expected updated DOB in cosmos is {newDob} but found {dobPostUpdate.ToString()} ");

            }

            else if (UpdateScenarioType == "FirstNameLastNameUpdate")
            {

                string firstNamePostUpdate = fcMemberPostUpdate.FirstName;

                string lastNamePostUpdate = fcMemberPostUpdate.LastName;

                while (retryCount > 0 && (firstNamePostUpdate.Equals(firstName, StringComparison.OrdinalIgnoreCase) || lastNamePostUpdate.Equals(lastName, StringComparison.OrdinalIgnoreCase)))
                {

                    await Task.Delay(2000);

                    fcMemberPostUpdate = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpdate);

                    firstNamePostUpdate = fcMemberPostUpdate.FirstName;

                    lastNamePostUpdate = fcMemberPostUpdate.LastName;

                    retryCount--;

                }

                Assert.True(firstNamePostUpdate.Equals(newFirstName), $"Expected updated First Name in cosmos is {newFirstName} but found {firstNamePostUpdate.ToString()} ");

                Assert.True(lastNamePostUpdate.Equals(newLastName), $"Expected updated Last Name in cosmos is {newLastName} but found {lastNamePostUpdate.ToString()} ");

            }
            #endregion            
        }


        [Trait("Category", Traits.CustomerMSRegression)]
        [Fact, RunOrder(5)]
        public async Task TC05_UpgradeVAC_ToFlyingClub_SuccessScenario()
        {

            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.TierCode = @tierCode order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "VAC");

            FlyingClubMember fcMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcMember is not null, "Failed to retrieve the Test Data");

            string customerId = fcMember.CustomerId;

            string memberNumber = fcMember.MemberNumber;

            DateTimeOffset? lastUpdatedOnBeforeUpgrade = fcMember.LastUpdatedOn;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.CustomerId= @customerId ";

            QueryDefinition parameterizedCustomerDocQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer fcCustomer = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedCustomerDocQuery);

            string phoneNumber = fcCustomer.ContactDetails.Where(contact => contact.ContactType.Equals("Phone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

            string emailAddress = fcCustomer.ContactDetails.Where(contact => contact.ContactType.Equals("Email", StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ContactValue;

            string firstName = fcCustomer.FirstName;

            string lastName = fcCustomer.LastName;

            string doB = fcCustomer.DateOfBirth?.ToString("yyyy-MM-dd");

            string enrollementChannel = "POWERAPP";

            string channelUserId = "Automation@test.com";

            #endregion

            #region for upgrade to FC Account - API call

            HttpResponseMessage response = Microservice.UpgradeToFlyingClub(tokenFixture.GetToken("customer"), ocpSubKey, firstName, lastName, doB, phoneNumber, emailAddress, customerId, memberNumber, enrollementChannel, channelUserId).EnsureIsSuccessStatusCode();

            dynamic vacUpgradeResponse = await response?.Content?.ReadAsStringAsync();

            Assert.True(!vacUpgradeResponse.ToString().Contains("Request failed"), $"Expected a valid response for VAC upgrade to FlyingClub Account but found {vacUpgradeResponse.ToString()}");

            Assert.True(vacUpgradeResponse.ToString().Contains("Account Upgraded Successful"), $"Expected a valid success response for VAC upgradeto FlyingClub Account with member status as Active {vacUpgradeResponse.ToString()}");

            #endregion

            #region Validations for Customer Document in Cosmos DB

            string cosmosQueryAfterUpgrade = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c._ts desc ";

            QueryDefinition parameterizedQueryAfterUpgrade = new QueryDefinition(query: cosmosQueryAfterUpgrade).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer customerDocumentAfterUpgrade = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpgrade);

            DateTimeOffset? lastUpdatedOnAfterUpgrade = customerDocumentAfterUpgrade.LastUpdatedOn;

            int retry = 5;

            while (lastUpdatedOnBeforeUpgrade.Equals(lastUpdatedOnAfterUpgrade) && retry > 0)
            {
                await Task.Delay(1000);

                customerDocumentAfterUpgrade = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryAfterUpgrade);

                lastUpdatedOnAfterUpgrade = customerDocumentAfterUpgrade.LastUpdatedOn;

                retry--;
            }

            Assert.True(customerDocumentAfterUpgrade is not null, "Failed to retrieve the Test Data");

            Assert.True(customerDocumentAfterUpgrade.CustomerId.Equals(customerId), $"Expected a valid customerId but found {customerId.ToString()}");

            Assert.True(customerDocumentAfterUpgrade.CreatedBy.Equals("WEB"), $"Expected a valid CreatedBy as WEB but found {customerDocumentAfterUpgrade.CreatedBy.ToString()}");

            Assert.True(customerDocumentAfterUpgrade.EnrollmentChannel.Equals(enrollementChannel), $"Expected a valid EnrolmentChannel as  {enrollementChannel} but found {customerDocumentAfterUpgrade.EnrollmentChannel.ToString()}");

            Assert.True(customerDocumentAfterUpgrade.FirstName.Equals(firstName), $"Expected a valid FirstName but found {customerDocumentAfterUpgrade.FirstName.ToString()}");

            Assert.True(customerDocumentAfterUpgrade.LastName.Equals(lastName), $"Expected a valid LastName but found {customerDocumentAfterUpgrade.LastName.ToString()}");

            string actualPhoneNumber = customerDocumentAfterUpgrade.ContactDetails?.FirstOrDefault(c => c.ContactType == "Phone")?.ContactValue;

            Assert.True(actualPhoneNumber.Equals(phoneNumber), $"Expected a valid PhoneNumber but found {actualPhoneNumber.ToString()}");

            string actualEmailId = customerDocumentAfterUpgrade.ContactDetails?.FirstOrDefault(c => c.ContactType == "Email")?.ContactValue;

            Assert.True(actualEmailId.Equals(emailAddress), $"Expected a valid EmailId but found {actualEmailId.ToString()}");

            #endregion

            #region Validations for Flying Club Member  Document in Cosmos DB

            string cosmosQueryForFCDocument = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c._ts desc ";

            QueryDefinition parameterizedFCDocumentQuery = new QueryDefinition(query: cosmosQueryForFCDocument).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

            FlyingClubMember flyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedFCDocumentQuery);

            Assert.True(flyingClubMemberDocument is not null, "Failed to retrieve the Flying Club member document");

            Assert.True(flyingClubMemberDocument.CustomerId.Equals(customerId), $"Expected a valid customerId but found {customerId.ToString()}");

            Assert.True(flyingClubMemberDocument.MemberNumber.Equals(memberNumber), $"Expected a valid MemberNumber but found {memberNumber.ToString()}");

            Assert.True(flyingClubMemberDocument.TierCode.Equals("Red"), $"Expected a valid TierCode as Red but found {flyingClubMemberDocument.TierCode.ToString()}");

            Assert.True(flyingClubMemberDocument.LastUpdatedBy.Equals(channelUserId), $"Expected a LastUpdatedBy as {channelUserId} but found {flyingClubMemberDocument.LastUpdatedBy.ToString()}");

            #endregion

        }

    }
}
