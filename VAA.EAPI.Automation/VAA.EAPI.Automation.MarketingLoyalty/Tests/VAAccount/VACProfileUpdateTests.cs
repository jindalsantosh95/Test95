using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.VAAccount
{
    [Collection("VACTests")]
    public class VACProfileUpdateTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public VACProfileUpdateTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerProfile>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory]
        [InlineData("firstName")]
        [InlineData("lastName")]
        [InlineData("emailId")]
        [InlineData("dateOfBirth")]
        [InlineData("phoneNumber")]
        [InlineData("preferenceValue")]
        public async Task VACProfileUpdate(string field)
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode = @tierCode order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string memberNumber = fcmDoc?.MemberNumber;

            string customerId = fcmDoc?.CustomerId;

            string queryForCustomerProfileDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition query = new QueryDefinition(query: queryForCustomerProfileDoc).WithParameter("@docType", "CustomerProfile").WithParameter("@customerId", customerId);

            CustomerProfile customerProfileDoc = cosmosFixture.CosmosQueryItem<CustomerProfile>(query);

            string vaaMarketingPreferenceValue = customerProfileDoc.Preferences.FirstOrDefault(x => x.PreferenceType == "VAAMarketingPermissions")?.PreferenceValue ?? "N";

            string customerProfileDocETagBeforeProfileUpdate = customerProfileDoc?._etag;

            string queryForCustomerDocument = "SELECT * FROM CustomerCollection c WHERE  c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDocument).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            string firstName = customerDoc.FirstName;

            string lastName = customerDoc.LastName;

            string dateOfBirth = customerDoc.DateOfBirth.Value.Date.ToString("dd-MMM-yyyy");

            string emailId = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Email")?.ContactValue;

            string phoneCountryCode = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone")?.CountryCode;

            string phoneNumber = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone")?.ContactValue;

            string customerDocETagBeforeProfieUpdate = customerDoc._etag;

            #endregion

            switch (field)
            {
                case "firstName":
                    firstName = DataGenerator.RandomFirstName();
                    break;
                case "lastName":
                    lastName = DataGenerator.RandomLastName();
                    break;
                case "dateOfBirth":
                    dateOfBirth = DataGenerator.RandomDateOfBirth().ToString("dd-MMM-yyyy");
                    break;
                case "emailId":
                    emailId = firstName + lastName + DataGenerator.RandomNumber(0, 100, true) + "@noreply.com";
                    break;
                case "phoneNumber":
                    phoneNumber = $"{DataGenerator.RandomNumber(77777, 99999, true)}{DataGenerator.RandomNumber(77777, 99999, true)}";
                    phoneCountryCode = Application.PhoneCountryCodes.GetAny();
                    break;
                case "preferenceValue":
                    vaaMarketingPreferenceValue = vaaMarketingPreferenceValue == "Y" ? "N" : "Y";
                    break;

                default:
                    break;
            }

            HttpResponseMessage response = GraphQL.VACProfileUpdate(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber, firstName, lastName, emailId, dateOfBirth, phoneCountryCode, phoneNumber, vaaMarketingPreferenceValue);

            dynamic profileUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedMessage = "Member Profile Details Updated Successfully";

            string actualMessage = profileUpdateResult?.data?.UpdateVACMemberProfile?.ToString();

            Assert.True(expectedMessage.Equals(actualMessage), $"For the Profile Update, \n Expected the Response Message to be \n {expectedMessage} \n but found \n {actualMessage}");

            string expectedLastUpdatedBy = "LOOPSScript@fly.virgin.com";

            customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            int retry = 5;

            if (!field.Equals("preferenceValue"))
            {

                while (customerDoc._etag == customerDocETagBeforeProfieUpdate && retry > 0)
                {
                    customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

                    await Task.Delay(1000);

                    retry--;
                }

                Assert.True(!customerDoc._etag.Equals(customerDocETagBeforeProfieUpdate), $"After the successful profile update of Member PI Details \n, Expected the change in Customer Doc ETag but found No Change");

                Assert.True(firstName.Equals(customerDoc?.FirstName), $"After the successful profile update \n, Expected the firstName value to be \n {firstName} but found \n {customerDoc?.FirstName}");

                Assert.True(lastName.Equals(customerDoc?.LastName), $"After the successful profile update \n, Expected the lastName value to be \n {lastName} but found \n {customerDoc?.LastName}");

                Assert.True(dateOfBirth.Equals(customerDoc?.DateOfBirth.Value.ToString("dd-MMM-yyyy")), $"After the successful profile update \n, Expected the DateOfBirth value to be \n {dateOfBirth} but found \n {customerDoc?.DateOfBirth.Value.ToString("dd-MM-yyyy")}");

                IEnumerable<ContactDetails> contactDetails = customerDoc.ContactDetails;

                Assert.True(emailId.Equals(contactDetails.FirstOrDefault(x => x.ContactType == "Email")?.ContactValue), $"After the successful profile update \n, Expected the EmailId value to be \n {emailId} but found \n {contactDetails.FirstOrDefault(x => x.ContactType == "Email")?.ContactValue}");

                Assert.True(phoneCountryCode.Equals(contactDetails.FirstOrDefault(x => x.ContactType == "Phone")?.CountryCode), $"After the successful profile update \n, Expected the PhoneNumber Country Code value to be \n {phoneCountryCode} but found \n {contactDetails.FirstOrDefault(x => x.ContactType == "Phone")?.CountryCode}");

                Assert.True(phoneNumber.Equals(contactDetails.FirstOrDefault(x => x.ContactType == "Phone")?.ContactValue), $"After the successful profile update \n, Expected the PhoneNumber value to be \n {emailId} but found \n {contactDetails.FirstOrDefault(x => x.ContactType == "Phone")?.ContactValue}");

                Assert.True(expectedLastUpdatedBy.Equals(customerDoc.LastUpdatedBy), $"After the successful profile update \n, Expected the LastUpdatedBy value to be \n {expectedLastUpdatedBy} but found \n {customerDoc.LastUpdatedBy}");

            }
            else
            {
                customerProfileDoc = cosmosFixture.CosmosQueryItem<CustomerProfile>(query);

                retry = 5;

                while (customerProfileDoc._etag == customerProfileDocETagBeforeProfileUpdate && retry > 0)
                {
                    customerProfileDoc = cosmosFixture.CosmosQueryItem<CustomerProfile>(query);

                    await Task.Delay(1000);

                    retry--;
                }

                Assert.True(!customerProfileDoc._etag.Equals(customerProfileDocETagBeforeProfileUpdate), $"After the successful profile update of Member Preference Details \n, Expected the change in Customer Profile Doc ETag but found No Change");

                Assert.True(vaaMarketingPreferenceValue.Equals(customerProfileDoc.Preferences.FirstOrDefault(x => x.PreferenceType == "VAAMarketingPermissions")?.PreferenceValue), $"After the successful profile update \n, Expected the VAA Marketing Pref Value to be \n {vaaMarketingPreferenceValue} but found \n {customerProfileDoc.Preferences.FirstOrDefault(x => x.PreferenceType == "VAAMarketingPermissions")?.PreferenceValue}");

                Assert.True(expectedLastUpdatedBy.Equals(customerProfileDoc.LastUpdatedBy), $"After the successful profile update \n, Expected the LastUpdatedBy value to be \n {expectedLastUpdatedBy} but found \n {customerProfileDoc.LastUpdatedBy}");

            }
        }

    }
}
