using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using Address = VAA.EAPI.Automation.Core.DB_Model.Entities.Address;
using ContactDetails = VAA.EAPI.Automation.Core.DB_Model.Entities.ContactDetails;
using CustomerSearch = VAA.EAPI.Automation.Core.DB_Model.Entities.CustomerSearch;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoopsBackendAPITests
{
    [Collection("CustomerApiSanityTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CustomerAPITests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        private static readonly IReadOnlyDictionary<string, int> loyaltyMemberStatusCodes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Active"] = 1,
            ["Expired"] = 2,
            ["Frozen"] = 3,
            ["Inactive"] = 4,
            ["Merged"] = 5,
            ["Deceased"] = 6,
            ["Suspended"] = 7,
            ["Cancelled"] = 8,
            ["Active Pending"] = 9,
            ["ActivePending"] = 9,
            ["Fraud"] = 10,
            ["Pending Cancellation"] = 11,
            ["Route Deal"] = 12
        };

        private static readonly IReadOnlyDictionary<string, int> loyaltyMemberTypeCodes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Individual"] = 1,
            ["Household"] = 2,
            ["Account"] = 3
        };

        public CustomerAPITests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            if (!backendAPIEnv.Is("Project-DEV"))
                this.tokenFixture.AddTokenForCustomerApiFromKeyVault("customer");

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<CustomerProfile>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

        }

        [Trait("Category", Traits.LOOPSCustomerAPIRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_CustomerSearchUsingFCMemberNumber()
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string tierCode = fcmDoc.TierCode;

            string status = fcmDoc.Status;

            string queryForCustomerSearchDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerSearchDoc).WithParameter("@docType", "CustomerSearch").WithParameter("@partitionKey", fcmDoc.MemberNumber);

            CustomerSearch customerSearchDoc = cosmosFixture.CosmosQueryItem<CustomerSearch>(paramQuery);

            string memberNumber = customerSearchDoc.LoyaltyMemberNumber;

            string customerId = customerSearchDoc.CustomerId;

            string firstName = customerSearchDoc.FirstName;

            string lastName = customerSearchDoc.LastName;

            string dateOfBirth = customerSearchDoc.DateOfBirth.Value.ToString("dd-MMM-yyyy");

            string emailAddress = customerSearchDoc.EmailAddress;

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.CustomerSearch(tokenFixture.GetToken("customer"), memberNumber, "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(result.customers[0].customerId.ToString().Equals(customerId),
               $"Expected the CustomerId in the Response to be {customerId} but found {result.customers[0].customerId.ToString()}");

            Assert.True(result.customers[0].memberNumber.ToString().Equals(memberNumber),
               $"Expected the Member Number in the Response to be {memberNumber} but found {result.customers[0].memberNumber.ToString()}");

            Assert.True(result.customers[0].status.ToString().Equals(status),
               $"Expected the Status in the Response to be {status} but found {result.customers[0].status.ToString()}");

            Assert.True(result.customers[0].tier.ToString().Equals(tierCode),
               $"Expected the TierCode in the Response to be {tierCode} but found {result.customers[0].tier.ToString()}");

            Assert.True(result.customers[0].firstName.ToString().Equals(firstName),
               $"Expected the Member FirstName in the Response to be {firstName} but found {result.customers[0].firstName.ToString()}");

            Assert.True(result.customers[0].lastName.ToString().Equals(lastName),
               $"Expected the Member LastName in the Response to be {lastName} but found {result.customers[0].lastName.ToString()}");

            Assert.True(result.customers[0].email.ToString().Equals(emailAddress),
               $"Expected the Member  EmailAddress in the Response to be {emailAddress} but found {result.customers[0].email.ToString()}");

            string actualDOB = DateTime.Parse(result.customers[0].dateOfBirth.ToString()).ToString("dd-MMM-yyyy");

            Assert.True(actualDOB.Equals(dateOfBirth),
               $"Expected the Member DOB in the Response to be {dateOfBirth} but found {actualDOB}");

            #endregion
        }

        [Trait("Category", Traits.LOOPSCustomerAPIRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_CustomerSearchUsingEmailAddress()
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string tierCode = fcmDoc.TierCode;

            string status = fcmDoc.Status;

            string queryForCustomerSearchDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerSearchDoc).WithParameter("@docType", "CustomerSearch").WithParameter("@partitionKey", fcmDoc.MemberNumber);

            CustomerSearch customerSearchDoc = cosmosFixture.CosmosQueryItem<CustomerSearch>(paramQuery);

            string memberNumber = customerSearchDoc.LoyaltyMemberNumber;

            string customerId = customerSearchDoc.CustomerId;

            string firstName = customerSearchDoc.FirstName;

            string lastName = customerSearchDoc.LastName;

            string dateOfBirth = customerSearchDoc.DateOfBirth.Value.ToString("dd-MMM-yyyy");

            string emailAddress = customerSearchDoc.EmailAddress;

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.CustomerSearch(tokenFixture.GetToken("customer"), "", emailAddress).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(result.customers[0].customerId.ToString().Equals(customerId),
               $"Expected the CustomerId in the Response to be {customerId} but found {result.customers[0].customerId.ToString()}");

            Assert.True(result.customers[0].memberNumber.ToString().Equals(memberNumber),
               $"Expected the Member Number in the Response to be {memberNumber} but found {result.customers[0].memberNumber.ToString()}");

            Assert.True(result.customers[0].status.ToString().Equals(status),
               $"Expected the Status in the Response to be {status} but found {result.customers[0].status.ToString()}");

            Assert.True(result.customers[0].tier.ToString().Equals(tierCode),
               $"Expected the TierCode in the Response to be {tierCode} but found {result.customers[0].tier.ToString()}");

            Assert.True(result.customers[0].firstName.ToString().Equals(firstName),
               $"Expected the Member FirstName in the Response to be {firstName} but found {result.customers[0].firstName.ToString()}");

            Assert.True(result.customers[0].lastName.ToString().Equals(lastName),
               $"Expected the Member LastName in the Response to be {lastName} but found {result.customers[0].lastName.ToString()}");

            Assert.True(result.customers[0].email.ToString().Equals(emailAddress),
               $"Expected the Member  EmailAddress in the Response to be {emailAddress} but found {result.customers[0].email.ToString()}");

            string actualDOB = DateTime.Parse(result.customers[0].dateOfBirth.ToString()).ToString("dd-MMM-yyyy");

            Assert.True(actualDOB.Equals(dateOfBirth),
               $"Expected the Member DOB in the Response to be {dateOfBirth} but found {actualDOB}");

            #endregion
        }

        [Trait("Category", Traits.LOOPSCustomerAPIRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_CustomerRetrieveUsingCustomerId()
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", fcmDoc.CustomerId);

            Core.DB_Model.Entities.Customer customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            string customerId = customerDoc.CustomerId;

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.CustomerRetrieve(tokenFixture.GetToken("customer"), customerId).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            CustomerDetails? custDetails = ((JObject)result.customerDetails).ToObject<CustomerDetails>();

            #region Basic Details Validation

            Assert.True(custDetails.LoyaltyMember.MemberNumber.Equals(customerDoc.LoyaltyMemberNumber),
               $"Expected the Member Number in the Response to be {customerDoc.LoyaltyMemberNumber} but found {custDetails.LoyaltyMember.MemberNumber}");

            Assert.True(loyaltyMemberStatusCodes.TryGetValue(fcmDoc.Status, out int expectedStatusCode),
              $"Unknown Member Status in the Flying Club document: {fcmDoc.Status}");

            Assert.True(int.TryParse(custDetails.LoyaltyMember.Status, out int actualStatusCode),
              $"Expected a numeric Member Status in the Response but found {custDetails.LoyaltyMember.Status}");

            Assert.True(actualStatusCode == expectedStatusCode,
              $"Expected the Member Status in the Response to be {fcmDoc.Status} ({expectedStatusCode}) but found {actualStatusCode}");

            Assert.True(loyaltyMemberTypeCodes.TryGetValue(fcmDoc.MemberType, out int expectedMemberTypeCode),
              $"Unknown Member Type in the Flying Club document: {fcmDoc.MemberType}");

            Assert.True(int.TryParse(custDetails.LoyaltyMember.MemberType, out int actualMemberTypeCode),
              $"Expected a numeric Member Type in the Response but found {custDetails.LoyaltyMember.MemberType}");

            Assert.True(actualMemberTypeCode == expectedMemberTypeCode,
              $"Expected the Member Type in the Response to be {fcmDoc.MemberType} ({expectedMemberTypeCode}) but found {actualMemberTypeCode}");

            Assert.True(custDetails.LoyaltyMember.EnrollmentChannel.Equals(customerDoc.EnrollmentChannel),
             $"Expected the Member Enrolment Channel in the Response to be {customerDoc.EnrollmentChannel} but found {custDetails.LoyaltyMember.EnrollmentChannel}");

            Assert.True(custDetails.LoyaltyMember.MediaCode.Equals(fcmDoc.MediaCode),
             $"Expected the Member MediaCode in the Response to be {fcmDoc.MediaCode} but found {custDetails.LoyaltyMember.MediaCode}");

            Assert.True(custDetails.FirstName.Equals(customerDoc.FirstName),
               $"Expected the Member FirstName in the Response to be {customerDoc.FirstName} but found {custDetails.FirstName}");

            Assert.True(custDetails.LastName.Equals(customerDoc.LastName),
               $"Expected the Member LastName in the Response to be {customerDoc.LastName} but found {custDetails.LastName}");

            Assert.True(custDetails.DateOfBirth.Date.Equals(customerDoc.DateOfBirth.Value.Date),
              $"Expected the Member DOB in the Response to be {customerDoc.DateOfBirth.Value.Date} but found {custDetails.DateOfBirth.Date}");

            Assert.True(custDetails.Title.Equals(customerDoc.Title),
              $"Expected the Member Title in the Response to be {customerDoc.Title} but found {custDetails.Title}");

            Assert.True(custDetails.Gender.Equals(customerDoc.Gender),
              $"Expected the Member Gender in the Response to be {customerDoc.Gender} but found {custDetails.Gender}");

            #endregion

            #region Email Details Validation

            ContactDetails emailDetails = customerDoc.ContactDetails.Where(c => c.ContactType == "Email" && c.Primary == "Y").FirstOrDefault();

            EmailDetails email = custDetails.ContactDetails.EmailDetails.Where(c => c.Primary == "Y").FirstOrDefault();

            Assert.True(emailDetails is not null,
                $"No Primary Email Found for the Customer {customerId}");

            Assert.True(email.EmailAddress.Equals(emailDetails.ContactValue),
              $"Expected the Member EmailAddress in the Response to be {emailDetails.ContactValue} but found {email.EmailAddress}");

            Assert.True(email.Valid.Equals(emailDetails.Valid),
              $"Expected the Member EmailAddress Valid Flag in the Response to be {emailDetails.Valid} but found {email.Valid}");

            #endregion

            #region Phone Details Validation

            ContactDetails phoneDetails = customerDoc.ContactDetails.Where(c => c.ContactType == "Phone" && c.Primary == "Y").FirstOrDefault();

            PhoneDetails phone = custDetails.ContactDetails.PhoneDetails.Where(c => c.Primary == "Y").FirstOrDefault();

            Assert.True(phoneDetails is not null,
                $"No Primary Phone Found for the Customer {customerId}");

            Assert.True(phone.PhoneNumber.Equals(phoneDetails.ContactValue),
              $"Expected the Member Phone/Contact number in the Response to be {phoneDetails.ContactValue} but found {phone.PhoneNumber}");

            Assert.True(phone.Valid.Equals(phoneDetails.Valid),
              $"Expected the Member Phone/Contact Number Valid Flag in the Response to be {phoneDetails.Valid} but found {phone.Valid}");

            #endregion

            #region Address Details Validation

            Address addressDetails = customerDoc.AddressDetails.Where(c => c.Primary == "Y").FirstOrDefault();

            Assert.True(addressDetails is not null,
               $"No Primary Address Found for the Customer {customerId}");

            Addressdetail address = custDetails.AddressDetails.Where(c => c.Primary == "Y").FirstOrDefault();

            Assert.True(address.AddressLine1.Equals(addressDetails.AddressLine1),
              $"Expected the Member Address Line 1 in the Response to be {addressDetails.AddressLine1} but found {address.AddressLine1}");

            Assert.True(address.City.Equals(addressDetails.City),
             $"Expected the Member City in the Response to be {addressDetails.City} but found {address.City}");

            Assert.True(address.Country.Equals(addressDetails.Country),
             $"Expected the Member Country  in the Response to be {addressDetails.Country} but found {address.Country}");

            Assert.True(address.PostalCode.Equals(addressDetails.PostCode),
             $"Expected the Member Address PostCode in the Response to be {addressDetails.PostCode} but found {address.PostalCode}");

            #endregion

            #endregion
        }

        [Trait("Category", Traits.LOOPSCustomerAPIRegression)]
        [Fact, RunOrder(4)]
        public async Task TC04_CustomerPreferenceUpdate()
        {
            #region Test Data

            string queryForCustomerProfileDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerProfileDoc).WithParameter("@docType", "CustomerProfile");

            CustomerProfile customerProfileDoc = cosmosFixture.CosmosQueryItem<CustomerProfile>(parameterizedQuery);

            string customerId = customerProfileDoc.CustomerId;

            #endregion

            #region API Call

            Preference? preference = customerProfileDoc.Preferences.FirstOrDefault(c => c.PreferenceType == "VAAMarketingPermissions");

            string prefValue = preference != null ? preference.PreferenceValue.Equals("Y") ? "N" : "Y" : "N";

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberPreference(tokenFixture.GetToken("customer"), customerId, prefValue).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Details have been saved successfully";

            Assert.True(result.ToString().Contains(expectedResponse),
                $"Expected the response to be {expectedResponse} but was {result.ToString()}");

            #endregion

            #region Preference Validation After Update

            string queryForCustomerProfDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerProfDoc).WithParameter("@docType", "CustomerProfile").WithParameter("@partitionKey", customerId);

            Preference pref = cosmosFixture.CosmosQueryItem<CustomerProfile>(paramQuery).Preferences.FirstOrDefault(c => c.PreferenceType == "VAAMarketingPermissions");

            Assert.True(pref.PreferenceValue.Equals(prefValue),
                $"Expected the preferenceValue to be {pref} but was {pref.PreferenceValue}");

            Assert.True(pref.LastUpdatedOn.Value.Date.Equals(DateTime.UtcNow.Date),
                $"Expected the preference Update Date to be {DateTime.UtcNow.Date} but was {pref.LastUpdatedOn.Value.Date}");

            #endregion
        }

        [Trait("Category", Traits.LOOPSCustomerAPIRegression)]
        [Fact, RunOrder(5)]
        public async Task TC05_CustomerEmailUpdate()
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", fcmDoc.CustomerId);

            Core.DB_Model.Entities.Customer customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            string customerId = customerDoc.CustomerId;

            ContactDetails contactDetail = customerDoc.ContactDetails.FirstOrDefault(c => c.ContactType == "Email" && c.Primary == "Y");

            Assert.True(contactDetail is not null,
               $"No Primary Email Id Found for the Member {customerDoc.LoyaltyMemberNumber}.");

            string emailContactId = contactDetail.ContactId;

            string emailAddress = DataGenerator.RandomString(20).ToLower() + "@testemailnoreply.com";

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberEmail(tokenFixture.GetToken("customer"), customerId, emailAddress, emailContactId).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Details have been saved successfully";

            Assert.True(result.ToString().Contains(expectedResponse),
                $"Expected the response to be {expectedResponse} but was {result.ToString()}");

            #endregion

            #region Email Validation After the Update

            customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            ContactDetails contactDetails = customerDoc.ContactDetails.FirstOrDefault(c => c.ContactType == "Email" && c.Primary == "Y");

            Assert.True(contactDetails.ContactValue.Equals(emailAddress),
                $"Expected the EmailAddress to be {emailAddress} but was {contactDetails.ContactValue}");

            Assert.True(contactDetails.LastUpdatedOn.Value.Date.Equals(DateTime.UtcNow.Date),
                $"Expected the EmailAddress Update Date to be {DateTime.UtcNow.Date} but was {contactDetails.LastUpdatedOn.Value.Date}");

            #endregion

        }

        [Trait("Category", Traits.LOOPSCustomerAPIRegression)]
        [Fact, RunOrder(6)]
        public async Task TC06_CustomerPhoneUpdate()
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", fcmDoc.CustomerId);

            Core.DB_Model.Entities.Customer customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            string customerId = customerDoc.CustomerId;

            string phoneContactId;

            try
            {

                phoneContactId = customerDoc.ContactDetails.FirstOrDefault(c => c.ContactType == "Phone" && c.Primary == "Y").ContactId;
            }
            catch (NullReferenceException)
            {
                throw new Xunit.Sdk.XunitException($"No Primary Phone Details Found for the Customer {customerId}");
            }

            Assert.True(!string.IsNullOrEmpty(phoneContactId),
               $"No Primary Phone Found for the Member {customerDoc.LoyaltyMemberNumber}.");

            string phoneNumber = DataGenerator.RandomNumber(100000000, 999999999).ToString();

            string startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberPhone(tokenFixture.GetToken("customer"), customerId, phoneNumber, startDate, phoneContactId).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Details have been saved successfully";

            Assert.True(result.ToString().Contains(expectedResponse),
                $"Expected the response to be {expectedResponse} but was {result.ToString()}");

            #endregion

            #region Phone Validation After the Update

            customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            ContactDetails contactDetails = customerDoc.ContactDetails.FirstOrDefault(c => c.ContactType == "Phone" && c.Primary == "Y");

            Assert.True(contactDetails.ContactValue.Equals(phoneNumber),
                $"Expected the Phone Number to be {phoneNumber} but was {contactDetails.ContactValue}");

            Assert.True(contactDetails.LastUpdatedOn.Value.Date.Equals(DateTime.UtcNow.Date),
                $"Expected the Phone Number Update Date to be {DateTime.UtcNow.Date} but was {contactDetails.LastUpdatedOn.Value.Date}");

            #endregion

        }

        [Trait("Category", Traits.LOOPSCustomerAPIRegression)]
        [Fact, RunOrder(7)]
        public async Task TC07_CustomerAddressUpdate()
        {

            #region Test Data

            PreRequisiteTestData.TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out _, out _);

            HttpResponseMessage enrolmentResponse = (await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy)).EnsureGQLResponseSuccess();

            string enrolmentContent = await enrolmentResponse.Content.ReadAsStringAsync();
            JObject enrolmentResult = JObject.Parse(enrolmentContent);
            string customerId = enrolmentResult.SelectToken("data.accountEnrolMember.customerId")?.ToString();

            Assert.False(customerId.IsNullOrWhiteSpace(), "Enrolment response did not contain a customer ID.");

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Core.DB_Model.Entities.Customer customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);
            Address enrolledPrimaryAddress = customerDoc?.AddressDetails?.FirstOrDefault(c => c.Primary == "Y");
            int enrolmentRetry = 5;

            while (enrolledPrimaryAddress is null && enrolmentRetry > 0)
            {
                await Task.Delay(1000);
                customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);
                enrolledPrimaryAddress = customerDoc?.AddressDetails?.FirstOrDefault(c => c.Primary == "Y");
                enrolmentRetry--;
            }

            Assert.True(enrolledPrimaryAddress is not null, $"No primary address found for the newly enrolled customer {customerId}.");
            string addressId = enrolledPrimaryAddress.AddressId;
            Assert.False(addressId.IsNullOrWhiteSpace(), $"Primary address ID was not found for customer {customerId}.");

            #endregion

            #region Calling Update Address API

            Random random = new();

            string addressLine1 = "Test AddressLine1 " + DataGenerator.RandomString(5);

            string addressLine2 = "Test AddressLine2 " + DataGenerator.RandomString(5);

            string addressLine3 = "Test AddressLine3 " + DataGenerator.RandomString(5);

            string company = "TestCompany" + DataGenerator.RandomString(5);

            string startDate = DateTime.Now.ToString("dd-MMM-yyyy");

            string[] postalCodes = { "PH2 3BF", "PH33 3WE", "CO10 4HA", "AB45 7BJ", "DN6 0TA", "TR11 9PY" };

            string[] cities = { "London", "Church Stretton", "Whitton", "New York", "Sydney", "Old Dailly" };

            string[] countries = { "United Kingdom", "United States of America", "Australia" };

            string city = cities[random.Next(cities.Length)];

            string postalCode = postalCodes[random.Next(postalCodes.Length)];

            string countryName = countries[random.Next(countries.Length)];

            string updatedBy = DataGenerator.RandomFirstName();

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberAddress(customerId, addressId, tokenFixture.GetToken("customer"),
                                  addressLine1, addressLine2, addressLine3, company, city, startDate, postalCode, "", countryName, updatedBy).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Details have been saved successfully";

            Assert.True(result.ToString().Contains(expectedResponse),
                $"Expected the message to be {expectedResponse} but found {result.ToString()}");

            #endregion

            #region Address Validation After the Update

            queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.LastUpdatedBy = @lastUpdatedBy ";

            paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId).WithParameter("@lastUpdatedBy", updatedBy);

            Address primaryAddress = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery)?.AddressDetails?.FirstOrDefault(c => c.Primary == "Y");

            int retry = 10;

            while (retry > 0 && primaryAddress is null)
            {
                await Task.Delay(1000);

                primaryAddress = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery)?.AddressDetails?.FirstOrDefault(c => c.Primary == "Y");

                retry--;

            }

            Assert.True(primaryAddress is not null, $" Failed to retrieve Primary Adddress for the Member {customerId} after the address update");

            Assert.True(primaryAddress.AddressLine1.Equals(addressLine1),
                $"Expected the AddressLine1 of the Parent {customerId} to be {addressLine1} but found {primaryAddress.AddressLine1}.");

            Assert.True(primaryAddress.AddressLine2.Equals(addressLine2),
                $"Expected the AddressLine2 of the Parent {customerId} to be {addressLine2} but found {primaryAddress.AddressLine2}.");

            Assert.True(primaryAddress.AddressLine3.Equals(addressLine3),
                $"Expected the AddressLine3 of the Parent {customerId} to be {addressLine3} but found {primaryAddress.AddressLine3}.");

            Assert.True(primaryAddress.LandlordOrCompanyName.Equals(company),
                $"Expected the CompanyName of the Parent {customerId} to be {company} but found {primaryAddress.LandlordOrCompanyName}.");

            Assert.True(primaryAddress.City.Equals(city),
                $"Expected the City of the Parent {customerId} to be {city} but found {primaryAddress.City}.");

            Assert.True(primaryAddress.PostCode.Equals(postalCode),
                $"Expected the PostCode of the Parent {customerId} to be {postalCode} but found {primaryAddress.PostCode}.");

            Assert.True(primaryAddress.Country.Equals(countryName),
                $"Expected the Country of the Parent {customerId} to be {countryName} but found {primaryAddress.Country}.");


            #endregion

        }

    }
}
