using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests
{
    public class MemberSearchByPhoneNumberTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private readonly string ocpSubKey;

        public MemberSearchByPhoneNumberTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
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

            tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");

            ocpSubKey = Vault.Default.GetSecretValue("TestingVRedMSSubscriptionKey");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact]
        public async Task MemberSearchByPhoneNumber()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "VAC").WithParameter("@tierCode2", "Glass");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string memberNumber = fcmDoc?.MemberNumber;

            string customerId = fcmDoc?.CustomerId;

            string queryForCustomerDocument = "SELECT * FROM CustomerCollection c WHERE  c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDocument).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            string phoneNumber = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone")?.ContactValue;

            #endregion

            SoftAssertHelper softAssert = new SoftAssertHelper();

            HttpResponseMessage response = GraphQL.MemberSearchByPhone(tokenFixture.GetToken("graphqlv2"), phoneNumber);

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            IEnumerable<MemberSearchByPhoneResponse> searchResponse = JsonConvert.DeserializeObject<MemberSearchByPhoneResponse[]>(result?.data?.memberSearchbyPhone?.ToString());

            HttpResponseMessage memberBalanceResponse = Microservice.GetMemberPointBalance(tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            string memberUsableBalance = result?.Usable?.ToString() ?? "0";

            MemberSearchByPhoneResponse memberData = searchResponse.First(x => x.memberNumber == memberNumber) ?? null;

            Assert.True(memberData is not null, $"MemberData not found in the API response for the memberNumber {memberNumber}\n. The Response Returned is \n {searchResponse}");

            softAssert.True(memberData.envelopeSalutation.Equals(customerDoc.EnvelopeSalutation), $"The Envelope Salution is not as expected. Expected {customerDoc.EnvelopeSalutation} but found \n  {memberData.envelopeSalutation}");

            softAssert.True(memberData.birthDate.Date.Equals(customerDoc.DateOfBirth.Value.Date), $"The Date Of Birth is not as expected. Expected {customerDoc.DateOfBirth.Value.Date} but found \n  {memberData.birthDate.Date}");

            softAssert.True(memberData.currentTier.Equals(fcmDoc.TierCode), $"The Member Current TierCode is not as expected. Expected {fcmDoc.TierCode} but found \n  {memberData.currentTier}");

            string expectedEmailAddress = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Email" && x.Primary == "Y")?.ContactValue;

            softAssert.True(memberData.emailAddress.Equals(expectedEmailAddress), $"The Member Email Address is not as expected. Expected {expectedEmailAddress} but found \n  {memberData.emailAddress}");

            softAssert.True(memberData.addressLine1 == customerDoc.AddressDetails.First().AddressLine1, $"The Member AddressLine 1 is not as expected. Expected {customerDoc.AddressDetails.First().AddressLine1} but found \n  {memberData.addressLine1}");

            softAssert.True(memberData.addressLine2 == customerDoc.AddressDetails.First().AddressLine2, $"The Member AddressLine 2 is not as expected. Expected {customerDoc.AddressDetails.First().AddressLine2} but found \n  {memberData.addressLine2}");

            softAssert.True(memberData.addressLine3 == customerDoc.AddressDetails.First().AddressLine3, $"The Member AddressLine 3 is not as expected. Expected {customerDoc.AddressDetails.First().AddressLine3} but found \n  {memberData.addressLine3}");

            softAssert.True(memberData.city == customerDoc.AddressDetails.First().City, $"The Member Address City is not as expected. Expected {customerDoc.AddressDetails.First().City} but found \n  {memberData.city}");

            softAssert.True(memberData.region == customerDoc.AddressDetails.First().Province, $"The Member Address Region is not as expected. Expected {customerDoc.AddressDetails.First().Province} but found \n  {memberData.region}");

            softAssert.True(memberData.country == customerDoc.AddressDetails.First().Country, $"The Member Address Country is not as expected. Expected {customerDoc.AddressDetails.First().Country} but found \n  {memberData.country}");

            softAssert.True(memberData.postCode == customerDoc.AddressDetails.First().PostCode, $"The Member Address PostCode is not as expected. Expected {customerDoc.AddressDetails.First().PostCode} but found \n  {memberData.postCode}");

            Core.DB_Model.Entities.ContactDetails expectedPhoneDetails = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone" && x.Primary == "Y");

            softAssert.True(memberData.phoneType == expectedPhoneDetails.ContactSubType, $"The Member PhoneType is not as expected. Expected {expectedPhoneDetails.ContactSubType} but found \n  {memberData.phoneType}");

            softAssert.True(memberData.phoneCountryCode == expectedPhoneDetails.CountryCode, $"The Member Phone CountryCode is not as expected. Expected {expectedPhoneDetails.CountryCode} but found \n  {memberData.phoneCountryCode}");

            softAssert.True(memberData.phoneNumber == expectedPhoneDetails.ContactValue, $"The Member Phone Number is not as expected. Expected {expectedPhoneDetails.ContactValue} but found \n  {memberData.phoneNumber}");

            softAssert.True(memberData.enrollmentDate.Date == fcmDoc.EnrollmentDate.Value.Date, $"The Member Enrolment Date is not as expected. Expected {fcmDoc.EnrollmentDate.Value.Date} but found \n  {memberData.enrollmentDate.Date}");

            softAssert.True(memberData.memberPointBalance == memberUsableBalance, $"The Member Point Balance is not as expected. Expected {memberUsableBalance} but found \n  {memberData.memberPointBalance}");

            softAssert.AssertAll();
        }

    }
}
