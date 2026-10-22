using System.Globalization;
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
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoopsFrontEndAPITests
{

    [Collection("CustomerFEAPITests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CustomerFEAPITests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly CosmosFixture _cosmosFixture;

        private readonly TokenFixture _tokenFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public CustomerFEAPITests(CosmosFixture _cosmosFixture, TokenFixture _tokenFixture)
        {
            this._cosmosFixture = _cosmosFixture;

            this._tokenFixture = _tokenFixture;

            this._tokenFixture.AddTokenForVRedFromKeyVault("vred");

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this._cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this._cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this._cosmosFixture.RegisterType<Core.DB_Model.Entities.Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this._cosmosFixture.RegisterType<CustomerProfile>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this._cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this._cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this._cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this._cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            this._cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

        }

        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("memberNumber")]
        [InlineData("emailId")]
        public async Task TC01_SearchMembers_SuccessScenario(string searchField)
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.CreatedBy <> @createdBy order by c.UpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@createdBy", "WEB");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string tierCode = fcmDoc.TierCode;

            string status = fcmDoc.Status;

            string queryForCustomerSearchDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerSearchDoc).WithParameter("@docType", "CustomerSearch").WithParameter("@partitionKey", fcmDoc.MemberNumber);

            CustomerSearch customerSearchDoc = _cosmosFixture.CosmosQueryItem<CustomerSearch>(paramQuery);

            string memberNumber = customerSearchDoc.LoyaltyMemberNumber;

            string customerId = customerSearchDoc.CustomerId;

            string firstName = customerSearchDoc.FirstName;

            string lastName = customerSearchDoc.LastName;

            string dateOfBirth = customerSearchDoc.DateOfBirth.Value.ToString("dd-MMM-yyyy");

            string emailAddress = customerSearchDoc.EmailAddress;

            #endregion

            memberNumber = searchField.Equals("emailId") ? "" : memberNumber; //To Hit endpoint based on searchField

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.SearchMembers(memberNumber, emailAddress).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            SearchMembers searchMembers;

            try
            {
                searchMembers = JsonConvert.DeserializeObject<SearchMembers>(result[0].ToString());
            }

            catch (Exception) { throw new Xunit.Sdk.XunitException($"Member Not found .The Response was {result.ToString()} "); }

            #endregion

            #region Validations

            Assert.True(searchMembers.customerId.Equals(customerId),
               $"Expected the CustomerId in the Response to be {customerId} but found {searchMembers.customerId}");

            Assert.True(searchMembers.accountNumber.Equals(customerSearchDoc.LoyaltyMemberNumber),
               $"Expected the Member Number in the Response to be {customerSearchDoc.LoyaltyMemberNumber} but found {searchMembers.accountNumber}");

            Assert.True(searchMembers.accountStatus.Equals(status),
               $"Expected the Status in the Response to be {status} but found {searchMembers.accountStatus}");

            Assert.True(searchMembers.tier.Equals(tierCode),
               $"Expected the TierCode in the Response to be {tierCode} but found {searchMembers.tier}");

            Assert.True(searchMembers.name.firstName.Equals(firstName),
               $"Expected the Member FirstName in the Response to be {firstName} but found {searchMembers.name.firstName}");

            Assert.True(searchMembers.name.lastName.Equals(lastName),
               $"Expected the Member LastName in the Response to be {lastName} but found {searchMembers.name.lastName}");

            Assert.True(searchMembers.emailAddress.Equals(emailAddress),
               $"Expected the Member  EmailAddress in the Response to be {emailAddress} but found {searchMembers.emailAddress}");

            string actualDOB = searchMembers.dateOfBirth.Value.ToString("dd-MMM-yyyy");

            Assert.True(actualDOB.Equals(dateOfBirth),
               $"Expected the Member DOB in the Response to be {dateOfBirth} but found {actualDOB}");

            #endregion
        }

        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_GetMember_SuccessScenario()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.CreatedBy <> @createdBy order by c.UpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@createdBy", "WEB");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve FCM Doc for test data");

            string tierCode = fcmDoc.TierCode;

            string status = fcmDoc.Status;

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerSearchDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerSearchDoc).WithParameter("@docType", "CustomerSearch").WithParameter("@partitionKey", fcmDoc.MemberNumber);

            CustomerSearch customerSearchDoc = _cosmosFixture.CosmosQueryItem<CustomerSearch>(paramQuery);

            Assert.True(customerSearchDoc is not null, $"Failed to retrieve CustomerSearch Doc for Customer {customerId}");

            string memberNumber = customerSearchDoc.LoyaltyMemberNumber;

            string firstName = customerSearchDoc.FirstName;

            string lastName = customerSearchDoc.LastName;

            string dateOfBirth = customerSearchDoc.DateOfBirth.Value.ToString("dd-MMM-yyyy");

            string emailAddress = customerSearchDoc.EmailAddress;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

            Assert.True(customerDoc is not null, $"Failed to retrieve Customer Doc for Customer {customerId}");

            string queryForCustomerProfileDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition queryForCPDoc = new QueryDefinition(query: queryForCustomerProfileDoc).WithParameter("@docType", "CustomerProfile").WithParameter("@partitionKey", customerId);

            CustomerProfile customerProfileDoc = _cosmosFixture.CosmosQueryItem<CustomerProfile>(queryForCPDoc);

            Assert.True(customerProfileDoc is not null, $"Failed to retrieve Customer Profile Doc for Customer {customerId}");

            string queryForAwardsDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition queryForAwards = new QueryDefinition(query: queryForAwardsDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId);

            IEnumerable<Core.DB_Model.Entities.Award> awardDocs = _cosmosFixture.CosmosQueryItems<Core.DB_Model.Entities.Award>(queryForAwards);

            #endregion

            #region API Call 

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.GetMember(memberNumber).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            GetMember getMember = JsonConvert.DeserializeObject<GetMember>(result.ToString());

            #endregion

            #region Validations

            Assert.True(getMember.accountNumber.Equals(memberNumber),
               $"Expected the Member Number in the Response to be {memberNumber} but found {getMember.accountNumber}");

            Assert.True(getMember.name.firstName.Equals(firstName),
              $"Expected the Member FirstName in the Response to be {firstName} but found {getMember.name.firstName}");

            Assert.True(getMember.name.lastName.Equals(lastName),
               $"Expected the Member LastName in the Response to be {lastName} but found {getMember.name.lastName}");

            Assert.True(getMember.tier.Equals(tierCode),
               $"Expected the TierCode in the Response to be {tierCode} but found {getMember.tier}");

            ContactDetails emailDetails = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Email" && x.Primary == "Y");

            Assert.True(emailDetails is not null, $"No Primary Email Details found for the customer {customerId}");

            Assert.True(getMember.emailAddress.usage.Equals(emailDetails.Usage),
               $"Expected the Email Usage in the Response to be {emailDetails.Usage} but found  {getMember.emailAddress.usage}");

            Assert.True(getMember.emailAddress.emailId.Equals(emailDetails.ContactValue),
               $"Expected the EmailAddress Value in the Response to be {emailDetails.ContactValue} but found {getMember.emailAddress.emailId} ");

            Assert.True(getMember.emailAddress.preferredFlag.Equals(emailDetails.Primary),
               $"Expected the Email PreferredFlag in the Response to be {emailDetails.Primary}  but found {getMember.emailAddress.preferredFlag}");

            Assert.True(getMember.accountStatus.Equals(status),
              $"Expected the Status in the Response to be {status} but found {getMember.accountStatus}");

            string actualDOB = getMember.dateOfBirth.Value.ToString("dd-MMM-yyyy");

            Assert.True(actualDOB.Equals(dateOfBirth),
               $"Expected the Member DOB in the Response to be {dateOfBirth} but found {actualDOB}");

            Core.DB_Model.Entities.Address address = customerDoc.AddressDetails.FirstOrDefault(x => x.Primary == "Y");

            Assert.True(address is not null, $"No Primary Address Details found for the customer {customerId}");

            Assert.True(getMember.address.addressType == address.AddressType,
              $"Expected the Address Type in the Response to be {address.AddressType}  but found {getMember.address.addressType}");

            Assert.True(getMember.address.addressLine1 == address.AddressLine1,
             $"Expected the Address Line1 in the Response to be {address.AddressLine1}  but found {getMember.address.addressLine1}");

            Assert.True(getMember.address.city == address.City,
             $"Expected the Address City in the Response to be {address.City}  but found {getMember.address.city}");

            Assert.True(getMember.address.country == address.Country,
             $"Expected the Address Country in the Response to be {address.Country}  but found {getMember.address.country}");

            ContactDetails phoneDetails = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone" && x.Primary == "Y");

            Assert.True(phoneDetails is not null, $"No Primary Phone Details found for the customer {customerId}");

            PhoneNumber actualPhone = getMember.phoneNumbers.FirstOrDefault(x => x.preferredFlag == "Y");

            Assert.True(actualPhone.usage == phoneDetails.Usage,
               $"Expected the Phone Usage in the Response to be {phoneDetails.Usage} but found  {actualPhone.usage}");

            Assert.True(actualPhone.phoneNumber == phoneDetails.ContactValue,
               $"Expected the Phone Value in the Response to be {phoneDetails.ContactValue} but found {actualPhone.phoneNumber} ");

            Assert.True(actualPhone.preferredFlag == phoneDetails.Primary,
               $"Expected the Phone PreferredFlag in the Response to be {phoneDetails.Primary}  but found {actualPhone.preferredFlag}");

            Requests.Models.FrontendAPI.Award[] actualAwards = getMember.awards.OrderByDescending(x => x.awardId).ToArray();

            awardDocs = awardDocs.OrderByDescending(x => x.AwardId);

            Assert.True(awardDocs.Count() == actualAwards.Count(),
                $"The No of Awards in response does not match with No Of Award Docs. Expected the count of awards to be {awardDocs.Count()} but found {actualAwards.Count()}");

            int i = 0;

            foreach (Core.DB_Model.Entities.Award award in awardDocs)
            {

                Assert.True(award.AwardId == actualAwards[i].awardId,
                   $"Expected the AwardId in the Response to be {award.AwardId} but found  {actualAwards[i].awardId}");

                Assert.True(award.ProductId == actualAwards[i].productId,
                   $"Expected the ProductId Value in the Response to be {award.ProductId} but found {actualAwards[i].productId} ");

                Assert.True(award.StartDate.ToString("dd-MMM-yyyy") == Convert.ToDateTime(actualAwards[i].startDate).ToString("dd-MMM-yyyy"),
                    $"Expected the Award StartDate Value in the Response to be {award.StartDate.ToString("dd-MMM-yyyy")} but found {Convert.ToDateTime(actualAwards[i].startDate).ToString("dd-MMM-yyyy")} ");

                i++;
            }


            Assert.NotNull(getMember.preferences);

            Requests.Models.FrontendAPI.Preference[] actualPreferences = getMember.preferences.ToArray();

            foreach (Core.DB_Model.Entities.Preference preference in customerProfileDoc.Preferences)
            {
                string prefType = preference.PreferenceType;

                Requests.Models.FrontendAPI.Preference actualPreference = actualPreferences.FirstOrDefault(x => x.preferenceType == prefType);

                // The LOOPS front-end member response can omit this stored preference.
                // If it is returned, its value must still match Cosmos.
                if (prefType == "VaGroupMarketingPermissions" && actualPreference is null)
                    continue;

                Assert.True(actualPreference is not null,
                    $"PrefType {prefType} not found in the response but is present in cosmos. Returned types: {string.Join(", ", actualPreferences.Select(x => x.preferenceType))}");

                Assert.True(actualPreference.preferenceValue == preference.PreferenceValue,
                    $"For the PrefType {prefType} expected the pref value to be {preference.PreferenceValue} but found {actualPreference.preferenceValue}");

            }


            #endregion
        }

        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_GetMemberVouchers_SuccessScenario()
        {
            #region Test Data

            string queryForAwardsDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.AwardBasis <> @awardBasis and c.ProductId not in ('1634561592','1634561747','1634561772',null,'') order by c.UpdatedOn desc ";

            QueryDefinition queryForAwards = new QueryDefinition(query: queryForAwardsDoc).WithParameter("@docType", "Award").WithParameter("@awardBasis", "Redemption");

            Core.DB_Model.Entities.Award awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(queryForAwards);

            Assert.True(awardDoc is not null, "Failed to retrieve Award Doc for test data");

            string customerId = awardDoc.CustomerId;

            string queryForAwardDocs = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @customerId and  c.ProductId not in ('1634561592','1634561747','1634561772',null,'') order by c.UpdatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForAwardDocs).WithParameter("@docType", "Award").WithParameter("@customerId", customerId);

            IEnumerable<Core.DB_Model.Entities.Award> custAwardDocs = _cosmosFixture.CosmosQueryItems<Core.DB_Model.Entities.Award>(query).OrderByDescending(x => x.AwardNumber);

            Assert.True(custAwardDocs.Any(), $"Failed to retrieve Award Docs for the Customer {customerId}");

            #endregion

            #region API Call 

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.GetMemberVouchers(customerId).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Voucher[] vouchers = JsonConvert.DeserializeObject<Voucher[]>(result.ToString());

            vouchers = vouchers.OrderByDescending(x => x.voucherNumber).ToArray();

            Assert.True(custAwardDocs.Count() == vouchers.Length,
                $"The no of vouchers in the response does not match. Expected the count to be {custAwardDocs.Count()} but found {vouchers.Length}");

            #endregion

            #region Validations

            int i = 0;

            foreach (Core.DB_Model.Entities.Award award in custAwardDocs)
            {
                Assert.True(award.AwardId == vouchers[i].voucherID,
                  $"Expected the VoucherId in the Response to be {award.AwardId} but found  {vouchers[i].voucherID}");

                Assert.True(award.AwardNumber == vouchers[i].voucherNumber,
                   $"For the Voucher Id {award.AwardId} ,Expected the Voucher Number in the Response to be {award.AwardNumber} but found {vouchers[i].voucherNumber} ");

                Assert.True(award.Status == vouchers[i].status,
                  $"For the Voucher Id {award.AwardId} ,Expected the Voucher Status in the Response to be {award.Status} but found {vouchers[i].status} ");

                string actualStartDate = Convert.ToDateTime(vouchers[i].startDate).ToString("dd-MMM-yyyy") ?? "";

                string actualEndDate = Convert.ToDateTime(vouchers[i].expiryDate).ToString("dd-MMM-yyyy") ?? "";

                string actualUsedDate = Convert.ToDateTime(vouchers[i].usedDate).ToString("dd-MMM-yyyy") ?? "";

                string actualVoucherGiftedDate = Convert.ToDateTime(vouchers[i].dateGifted).ToString("dd-MMM-yyyy") ?? "";

                Assert.True(award.StartDate.ToString("dd-MMM-yyyy") == actualStartDate,
                  $"For the Voucher Id {award.AwardId} ,Expected the Voucher StartDate in the Response to be {award.StartDate.ToString("dd-MMM-yyyy")} but found {actualStartDate} ");

                Assert.True(award.ExpiryDate.ToString("dd-MMM-yyyy") == actualEndDate,
                  $"For the Voucher Id {award.AwardId} ,Expected the Voucher EndDate in the Response to be {award.ExpiryDate.ToString("dd-MMM-yyyy")} but found {actualEndDate} ");

                Assert.True(award.UsedDate.ToString("dd-MMM-yyyy") == actualUsedDate,
                 $"For the Voucher Id {award.AwardId} ,Expected the Voucher UsedDate in the Response to be {award.UsedDate.ToString("dd-MMM-yyyy")} but found {actualUsedDate} ");

                Assert.True(award.GiftedAwardDate.ToString("dd-MMM-yyyy") == actualVoucherGiftedDate,
                 $"For the Voucher Id {award.AwardId} ,Expected the Voucher GiftedDate in the Response to be {award.GiftedAwardDate.ToString("dd-MMM-yyyy")} but found {actualVoucherGiftedDate} ");


                i++;
            }

            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(4)]
        public async Task TC04_AwardVoucherToMember_SuccessScenario()
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.Status = @status order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@status", "Active");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve FCM Doc for test data");

            string customerId = fcmDoc.CustomerId;

            string queryForVoucherDetails = "Select TOP 1 ProductName,RowId,ExpirationPeriod from dbo.Product where ProductType='Electronic Voucher' and active='1' and RowID not in ('1-2XNQE0H','1-2XNQE13') order by NEWID()";

            object[][] voucherDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForVoucherDetails);

            Assert.True(voucherDetails.Length > 0 && voucherDetails[0].Length == 3, "No eligible voucher product was returned from the reference database.");

            string voucherName = voucherDetails[0][0]?.ToString();

            string voucherProductId = voucherDetails[0][1]?.ToString();

            Assert.False(voucherName.IsNullOrWhiteSpace(), "Voucher product name was not returned from the reference database.");

            Assert.False(voucherProductId.IsNullOrWhiteSpace(), "Voucher product ID was not returned from the reference database.");

            Assert.True(int.TryParse(voucherDetails[0][2]?.ToString(), out int expiryMonths) && expiryMonths > 0,
                $"Voucher product {voucherProductId} did not have a positive expiration period in months.");


            DateTime date = DateTime.UtcNow;

            string startDate = date.ToString("yyyy-MM-dd");

            string expiryDate = date.AddMonths(expiryMonths).ToString("yyyy-MM-dd");

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = createdByUserName + "@testemail.co.ic";


            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.AwardVoucher(customerId, voucherName, startDate, expiryDate, "AutomationScript", createdByEmailId, createdByUserName).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher rewarded successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");

            #endregion

            #region Validation

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.CreatedBy = @createdBy order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@createdBy", createdByEmailId);

            Core.DB_Model.Entities.Award awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            int retry = 5;

            while (retry > 0 && awardDoc is null)
            {
                await Task.Delay(1000);

                awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

                retry--;
            }

            Assert.True(awardDoc is not null, $"Award doc is not created for the customerId {customerId} after awarding voucher");

            Assert.True(awardDoc.ProductId.Equals(voucherProductId),
                $"Expected the Voucher ProductId to be {voucherProductId} but was {awardDoc.ProductId} .");

            Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd").Equals(startDate),
                $"Expected the Voucher StartDate to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.ExpiryDate.ToString("yyyy-MM-dd").Equals(expiryDate),
                $"For voucher product {voucherProductId} ({expiryMonths} months), expected the Voucher ExpiryDate to be {expiryDate} but was {awardDoc.ExpiryDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.Status.Equals("Available"),
                $"Expected the Voucher Status to be Available but was {awardDoc.Status} .");

            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(5)]
        public async Task TC05_RedeemVoucher_SuccessScenario()
        {
            #region Pre Requisite

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.Status = @status and c.ProductId not in ('1634561592','1634561747','1634561772',null,'') order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@status", "Available");

            Core.DB_Model.Entities.Award awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            string customerId = awardDoc.CustomerId;

            string voucherId = awardDoc.AwardId;

            string voucherNumber = awardDoc.AwardNumber;

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = createdByUserName + "@testemail.co.ic";

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.RedeemVoucher(customerId, voucherId, voucherNumber, "AutomationScript", createdByEmailId, createdByUserName).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher redeemed successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");

            #endregion

            #region Validation

            queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @voucherId and c.LastUpdatedBy = @updatedBy ";

            paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@voucherId", voucherId).WithParameter("@updatedBy", createdByEmailId);

            awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            int retry = 5;

            while (retry > 0 && awardDoc is null)
            {
                await Task.Delay(1000);

                awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

                retry--;
            }

            Assert.True(awardDoc.Status.Equals("Used"),
                $"After the Redeem API Call, Expected the Voucher Status to be Used but was {awardDoc.Status}.");

            Assert.True(awardDoc.UsedDate.ToString("yyyy-MM-dd").Equals(DateTime.UtcNow.ToString("yyyy-MM-dd")),
                $"After the Redeem API Call, Expected the Voucher Used Date to be {DateTime.UtcNow.ToString("yyyy-MM-dd")}" +
                $" but was {awardDoc.UsedDate.ToString("yyyy-MM-dd")}.");

            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(6)]
        public async Task TC06_UpdateVoucher_SuccessScenario()
        {
            #region Pre Requisite

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.Status = @status and c.ProductId not in ('1634561592','1634561747','1634561772',null,'') order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@status", "Available");

            Core.DB_Model.Entities.Award awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            Assert.True(awardDoc is not null, "Failed to retrieve Award doc for the Test Execution");

            string customerId = awardDoc.CustomerId;

            string voucherId = awardDoc.AwardId;

            string voucherStatus = awardDoc.Status;

            string voucherNumber = awardDoc.AwardNumber;

            string[] validStatuses = { "Available", "Expired", "Used", "Cancelled", "Gifted", "Partially Used" };

            string newStatus = validStatuses[0];

            Random random = new();

            while (newStatus == voucherStatus)
            {
                newStatus = validStatuses[random.Next(validStatuses.Length)];
            }

            string startDate = awardDoc.StartDate.AddDays(-1).ToString("yyyy-MM-dd");

            string endDate = awardDoc.ExpiryDate.AddDays(+1).ToString("yyyy-MM-dd");

            string usedDate = newStatus.Equals("Used") ? DateTime.UtcNow.ToString("yyyy-MM-dd") : "";

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = createdByUserName + "@testemail.co.ic";


            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.UpdateVoucher(customerId, voucherId, voucherNumber, startDate, endDate, newStatus, "AutomationScript", createdByEmailId, createdByUserName).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher updated successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");

            #endregion

            #region Validation

            queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @voucherId and c.LastUpdatedBy = @updatedBy ";

            paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@voucherId", voucherId).WithParameter("@updatedBy", createdByEmailId);

            awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            int retry = 5;

            while (retry > 0 && awardDoc is null)
            {
                await Task.Delay(1000);

                awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

                retry--;
            }

            string actualUsedDate = awardDoc.UsedDate.Equals(DateTime.MinValue) ? "" : awardDoc.UsedDate.ToString("yyyy-MM-dd");

            Assert.True(actualUsedDate.Equals(usedDate),
                $"Expected the Voucher UsedDate to be {usedDate} but was {actualUsedDate} .");

            Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd").Equals(startDate),
                $"Expected the Voucher StartDate to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.ExpiryDate.ToString("yyyy-MM-dd").Equals(endDate),
                $"Expected the Voucher EndDate to be {endDate} but was {awardDoc.ExpiryDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.Status.Equals(newStatus),
                $"Expected the Voucher Status to be {newStatus} but was {awardDoc.Status} .");

            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(7)]
        public async Task TC07_TransferVoucher_SuccessScenario()
        {

            #region Pre Requisite

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.Status = @status and c.ProductId not in ('1634561592','1634561747','1634561772',null,'') and c.AwardNumber not like '%-2' order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@status", "Available");

            Core.DB_Model.Entities.Award awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            string customerId = awardDoc.CustomerId;

            string voucherId = awardDoc.AwardId;

            string productId = awardDoc.ProductId;

            string queryForVoucherName = $"Select ProductName from dbo.Product where RowId = '{productId}' ";

            string voucherName = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForVoucherName);

            string voucherNumber = awardDoc.AwardNumber;

            string startDate = awardDoc.StartDate.ToString("yyyy-MM-dd");

            string expiryDate = awardDoc.ExpiryDate.ToString("yyyy-MM-dd");

            string queryForCustomerDoc = $"SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey <> @partitionKey and c.Status=@status order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId).WithParameter("@status", "Active");

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

            string recipientCustomerId = customerDoc.CustomerId;

            string recipientMemberNumber = customerDoc.LoyaltyMemberNumber;

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = createdByUserName + "@testemail.co.ic";

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.TransferVoucher(customerId, "", recipientMemberNumber, recipientCustomerId, voucherNumber, voucherId, voucherName, "AutomationScript", createdByEmailId, createdByUserName).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher gifted successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");

            #endregion

            #region Validation

            queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @voucherId and c.LastUpdatedBy = @updatedBy ";

            paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@voucherId", voucherId).WithParameter("@updatedBy", createdByEmailId);

            awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            int retry = 5;

            while (retry > 0 && awardDoc is null)
            {
                await Task.Delay(1000);

                awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

                retry--;
            }

            Assert.True(awardDoc.Status.Equals("Gifted"),
                $"After the Gift API Call, Expected the Voucher Status in the Sender Doc to be Gifted but was {awardDoc.Status}.");

            queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.CreatedBy = @createdBy ";

            paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", recipientCustomerId).WithParameter("@createdBy", createdByEmailId);

            awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            retry = 5;

            while (retry > 0 && awardDoc is null)
            {
                await Task.Delay(1000);

                awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

                retry--;
            }

            Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd").Equals(startDate),
                $"After the Gift API Call ,Expected the Voucher StartDate in the recipient Doc to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.ExpiryDate.ToString("yyyy-MM-dd").Equals(expiryDate),
                $"After the Gift API Call ,Expected the Voucher EndDate in the recipient Doc to be {expiryDate} but was {awardDoc.ExpiryDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.Status.Equals("Available"),
                $"After the Gift API Call, Expected the Voucher Status in the recipient Doc to be Available but was {awardDoc.Status} .");

            Assert.True(awardDoc.ProductId.Equals(productId),
               $"After the Gift API Call, Expected the Voucher ProductId in the recipient Doc to be {productId} but was {awardDoc.ProductId} .");

            Assert.True(awardDoc.AwardNumber.Equals($"{voucherNumber}-2"),
              $"After the Gift API Call, Expected the Voucher Number in the recipient Doc to be {voucherNumber}-2 but was {awardDoc.AwardNumber} .");

            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(8)]
        public async Task TC08_ManualCreditAdjustment_SuccessScenario()
        {

            #region Test Data

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType and c.StatusCode = @statusCode order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual").WithParameter("@statusCode", "Processed");

            Transaction txnDoc = _cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            Assert.True(txnDoc is not null,
              "Failed to Retrieve Test Data for Credit Transaction.");

            string memberNumber = txnDoc.MemberNumber;

            string customerId = txnDoc.CustomerId;

            string partnerCode = "VS";

            string productName = "Authorised Points Credit";

            string activityDate = DataGenerator.RandomPastDate(+365).ToString("dd-MMM-yyyy");

            int points = Convert.ToInt32(DataGenerator.RandomNumber(100, 200, true));

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = createdByUserName + "@testemail.co.ic";

            #endregion

            #region VRed Balance API Call for Member Before Manual Credit Txn

            HttpResponseMessage vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

            dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

            int usableBalance = Convert.ToInt32(vRedResult.usable.ToString());

            #endregion

            #region API Call


            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.ManualCreditAdjustment(memberNumber, partnerCode, productName, activityDate, "Test txn", points, 0, "AutomationScript", createdByEmailId, createdByUserName, customerId).EnsureIsSuccessStatusCode();


            #endregion

            #region VRed Balance API Call for Member After Manual Credit Txn

            vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

            vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

            int memberUsableBalanceAfterAdjustment = Convert.ToInt32(vRedResult.usable.ToString());

            int retry = 5;

            while (retry > 0 && memberUsableBalanceAfterAdjustment == usableBalance)
            {
                await Task.Delay(1000);

                vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

                vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

                memberUsableBalanceAfterAdjustment = Convert.ToInt32(vRedResult.usable.ToString());

                retry--;
            }

            Assert.True(memberUsableBalanceAfterAdjustment == usableBalance + points,
               $"The Sender Member {memberNumber} Usable balance after the successful Manual Credit Adjustment is not as expected. Expected the Usable balance to be  {usableBalance + points} but  was {memberUsableBalanceAfterAdjustment}.");

            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(9)]
        public async Task TC09_PointsTransfer_SuccessScenario()
        {

            #region Test Data

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType and c.StatusCode = @statusCode and c.ProductId= @productId order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual").WithParameter("@statusCode", "Processed").WithParameter("@productId", "1-1ZWY5ZT");

            Transaction txnDoc = _cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            Assert.True(txnDoc is not null,
              "Failed to Retrieve Test Data for Sender Account.");

            string senderAccountFcNumber = txnDoc.MemberNumber;

            string senderAccountCustomerId = txnDoc.CustomerId;

            string queryForReceipientFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.Status = @status and c.PartitionKey <> @customerId and c.SSAMemberNumber = '' order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForReceipientFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@status", "Active").WithParameter("@customerId", senderAccountCustomerId);

            FlyingClubMember recipientFCMDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(recipientFCMDoc is not null,
             "Failed to Retrieve Test Data for Recipient Account.");

            string recipientAccountFcNumber = recipientFCMDoc.MemberNumber;

            string recipientAccountCustomerId = recipientFCMDoc.CustomerId;

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = createdByUserName + "@testemail.co.ic";

            #endregion

            #region VRed Balance API Call for Sender Account Before Points Transfer

            HttpResponseMessage vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), senderAccountFcNumber).EnsureIsSuccessStatusCode();

            dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

            int usableBalance = Convert.ToInt32(vRedResult.usable.ToString());

            Assert.True(usableBalance is > 0 and > 10,
                $"The Member Usable balance is not sufficient for the Points transfer. The usable balance is {usableBalance}");

            int points = Convert.ToInt32(DataGenerator.RandomNumber(10, usableBalance, true));

            #endregion

            #region VRed Balance API Call for Recipient Account Before Points Transfer

            HttpResponseMessage vRedResponseForRecipient = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), recipientAccountFcNumber).EnsureIsSuccessStatusCode();

            dynamic recipientResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponseForRecipient.Content.ReadAsStringAsync());

            int recipientUsableBalance = Convert.ToInt32(recipientResult.usable.ToString());

            #endregion

            #region Points Transfer API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.PointsTransfer(senderAccountCustomerId, senderAccountFcNumber, recipientAccountFcNumber, recipientAccountCustomerId, points, "AutomationScript", createdByEmailId, createdByUserName).EnsureIsSuccessStatusCode();

            #endregion

            #region VRed Balance API Call for Sender Account After Points Transfer

            vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), senderAccountFcNumber).EnsureIsSuccessStatusCode();

            vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

            int senderUsableBalanceAfterTransfer = Convert.ToInt32(vRedResult.usable.ToString());

            int retry = 10;

            while (retry > 0 && senderUsableBalanceAfterTransfer == usableBalance)
            {
                await Task.Delay(1000);

                vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), senderAccountFcNumber).EnsureIsSuccessStatusCode();

                vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

                senderUsableBalanceAfterTransfer = Convert.ToInt32(vRedResult.usable.ToString());

                retry--;
            }

            Assert.True(senderUsableBalanceAfterTransfer == usableBalance - points,
               $"The Sender Member {senderAccountFcNumber} Usable balance after the successful points tranfer is not as expected. Expected the Usable balance to be  {usableBalance - points} but  was {senderUsableBalanceAfterTransfer}.");


            #endregion

            #region VRed Balance API Call for Recipient Account After Points Transfer

            vRedResponseForRecipient = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), recipientAccountFcNumber).EnsureIsSuccessStatusCode();

            recipientResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponseForRecipient.Content.ReadAsStringAsync());

            int recipientUsableBalanceAfterTransfer = Convert.ToInt32(recipientResult.usable.ToString());

            retry = 5;

            while (retry > 0 && recipientUsableBalanceAfterTransfer == recipientUsableBalance)
            {

                await Task.Delay(1000);

                vRedResponseForRecipient = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), recipientAccountFcNumber).EnsureIsSuccessStatusCode();

                recipientResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponseForRecipient.Content.ReadAsStringAsync());

                recipientUsableBalanceAfterTransfer = Convert.ToInt32(recipientResult.usable.ToString());

                retry--;
            }

            Assert.True(recipientUsableBalanceAfterTransfer == recipientUsableBalance + points,
              $"The Recipient Member {recipientAccountFcNumber} Usable balance after the successful points tranfer is not as expected. Expected the Usable balance to be  {recipientUsableBalance + points} but  was {recipientUsableBalanceAfterTransfer}.");



            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(10)]
        public async Task TC10_GetMemberTierHistory_SuccessScenario()
        {

            #region Test Data

            string queryForMebershipChangeDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.UpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForMebershipChangeDoc).WithParameter("@docType", "MembershipChange").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            MembershipChange membershipChangeDoc = _cosmosFixture.CosmosQueryItem<MembershipChange>(paramQuery);

            Assert.True(membershipChangeDoc is not null,
              "Failed to Retrieve Membership Change doc for Test execution.");

            string memberNumber = membershipChangeDoc.MemberNumber;

            string customerId = membershipChangeDoc.CustomerId;

            string queryForMebershipChangeDocs = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition query = new QueryDefinition(query: queryForMebershipChangeDocs).WithParameter("@docType", "MembershipChange").WithParameter("@customerId", customerId);

            IEnumerable<MembershipChange> membershipChangeDocs = _cosmosFixture.CosmosQueryItems<MembershipChange>(query).OrderByDescending(x => x.EndDate);

            #endregion


            #region API Call 

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.GetTierHistory(customerId).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            MemberTierHistory tierHistory = JsonConvert.DeserializeObject<MemberTierHistory>(result);

            TierHistoryItem[] tierHistories = tierHistory.tierHistoryItems.OrderByDescending(x => x.endDate).ToArray();

            Assert.True(membershipChangeDocs.Count() == tierHistories.Length,
                $"The no of Tier history sections in the response does not match. Expected the count to be {membershipChangeDocs.Count()} but found {tierHistories.Length}");

            #endregion


            #region Validations

            int i = 0;

            foreach (MembershipChange membershipChange in membershipChangeDocs)
            {

                Assert.True(tierHistories[i].tier == membershipChange.TierCode,
                 $"For the doc id {membershipChange.Id} , Expected the TierCode to be {membershipChange.TierCode} but found {tierHistories[i].tier} ");

                Assert.True(tierHistories[i].startDate?.Date.ToString("dd-MMM-yyyy") == membershipChange.StartDate.Value.ToString("dd-MMM-yyyy"),
                   $"For the doc id {membershipChange.Id} , Expected the Tier StartDate to be {membershipChange.StartDate} but found {tierHistories[i].startDate?.Date} ");

                Assert.True(tierHistories[i].endDate?.Date.ToString("dd-MMM-yyyy") == membershipChange.EndDate.Value.ToString("dd-MMM-yyyy"),
                   $"For the doc id {membershipChange.Id} , Expected the Tier EndDate to be {membershipChange.EndDate} but found {tierHistories[i].endDate?.Date} ");

                Assert.True(tierHistories[i].active == membershipChange.Active,
                   $"For the doc id {membershipChange.Id} , Expected the Tier Active Flag to be {membershipChange.Active} but found {tierHistories[i].active} ");

                Assert.True(tierHistories[i].qualifyingTierPoints == (int)double.Parse(membershipChange.QualifyingTierPoints),
                  $"For the doc id {membershipChange.Id} , Expected the QualifyingTierPoints to be {membershipChange.QualifyingTierPoints} but found {tierHistories[i].qualifyingTierPoints} ");

                Assert.True(tierHistories[i].revision == membershipChange.Revision.ToString(),
                   $"For the doc id {membershipChange.Id} , Expected the Tier Revision number to be {membershipChange.Revision.ToString()} but found {tierHistories[i].revision} ");

                Assert.True(tierHistories[i].updatedOn?.Date.ToString("dd-MMM-yyyy") == membershipChange.LastUpdatedOn.Value.ToString("dd-MMM-yyyy"),
                  $"For the doc id {membershipChange.Id} , Expected the Doc UpdatedOnDate to be {membershipChange.LastUpdatedOn} but found {tierHistories[i].updatedOn?.Date} ");

                Assert.True(tierHistories[i].userCreated == membershipChange.CreatedBy,
                  $"For the doc id {membershipChange.Id} , Expected the Doc CreatedBy to be {membershipChange.CreatedBy} but found {tierHistories[i].userCreated} ");

                Assert.True(tierHistories[i].userUpdated == membershipChange.LastUpdatedBy,
                  $"For the doc id {membershipChange.Id} , Expected the Doc CreatedBy to be {membershipChange.LastUpdatedBy} but found {tierHistories[i].userUpdated} ");

                i++;

            }


            #endregion


        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(11)]
        public async Task TC11_ChangeMemberTierToSilver_SuccessScenario()
        {

            #region Fetching Data From Reporting DB - Red Tier Member 

            string queryToFetchRedTierMembers = "Select Top 1 CustomerId,MemberNumber from SCV.FlyingClubMember where TierCode='Red' and TierCode not in ('Glass','VAC') and Status='Active' and CreatedBy !='WEB-MYA' order by CreatedOn desc";

            Dictionary<string, string> dbResponse = AzureReportingDBHelper.MultiRowKeyValuePairQueryResponse(queryToFetchRedTierMembers);

            Assert.True(dbResponse.Any(), "Failed to test data for tier change");

            #endregion

            #region Updating the Member to Silver 

            string custId = dbResponse.First().Key;

            string tierStartDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

            string tierEndDate = DateTime.UtcNow.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date.ToString("yyyy-MM-dd");

            string tierName = "Silver";

            string queryForMembershipChangeDocument = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'MembershipChange' and  c.PartitionKey='{custId}' and c.Active='Y'";

            MembershipChange membershipChangeDoc = _cosmosFixture.QueryItem<MembershipChange>(queryForMembershipChangeDocument);

            string docId = membershipChangeDoc.Id;

            int revision = !string.IsNullOrEmpty(membershipChangeDoc.Revision.ToString()) ? Convert.ToInt32(Convert.ToDouble(membershipChangeDoc.Revision)) + 1 : 1;

            string createdBy = DataGenerator.RandomFirstName();

            #region Creating a New Tier API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.CreateNewTier(custId, tierName, tierStartDate, tierEndDate, revision.ToString(), createdBy, "Manual Upgrades");

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Tier Created Successfully";

            Assert.True(result.ToString().Contains(expectedResponse),
                $"Expected the message to be {expectedResponse} but found {result.ToString()}");

            #endregion

            #region Fetching Document Id for the Newly Created Tier

            queryForMembershipChangeDocument = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'MembershipChange' and  c.PartitionKey='{custId}' and c.Active='N' and c.CreatedBy = '{createdBy}' order by c.CreatedOn desc";

            membershipChangeDoc = _cosmosFixture.QueryItem<MembershipChange>(queryForMembershipChangeDocument);

            int retry = 10;

            while ((retry > 0 && membershipChangeDoc == null) || membershipChangeDoc.Id == docId)
            {
                await Task.Delay(500);

                membershipChangeDoc = _cosmosFixture.QueryItem<MembershipChange>(queryForMembershipChangeDocument);

                retry--;
            }

            Assert.True(membershipChangeDoc is not null,
                $"No Document Available for the searched query {queryForMembershipChangeDocument}.");

            docId = membershipChangeDoc.Id;

            #endregion

            #region Updating the Newly Created Tier API CALL

            string updatedBy = DataGenerator.RandomFirstName();

            response = LoopsFrontEndAPIEndpoints.UpdateNewTier(custId, docId, tierName, membershipChangeDoc.TierApprovalStatus, revision.ToString(), updatedBy);

            result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            expectedResponse = "Request Initiated successfully";

            Assert.True(result.message.ToString().Equals(expectedResponse),
                $"Expected the message to be {expectedResponse} but found {result.ToString()}");

            #endregion

            #endregion

            #region Validation of Newly Created Tier Active Flag in MembershipChange Doc

            string queryForMembershipChangeDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.Active = @activeFlag and c.TierCode = @tierCode order by c.LastUpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForMembershipChangeDoc).WithParameter("@docType", "MembershipChange").WithParameter("@customerId", custId).WithParameter("@activeFlag", "Y").WithParameter("@tierCode", tierName);

            MembershipChange membershipChangeDocAfterUpdate = _cosmosFixture.CosmosQueryItem<MembershipChange>(paramQuery);

            retry = 10;

            while (retry > 0 && membershipChangeDocAfterUpdate == null)
            {
                await Task.Delay(1000);

                membershipChangeDocAfterUpdate = _cosmosFixture.CosmosQueryItem<MembershipChange>(paramQuery);

                retry--;
            }

            Assert.True(membershipChangeDocAfterUpdate is not null,
             $"Failed to retrive the new membershipchange doc after tierchange of New tier: {tierName} for the customerId {custId}");

            #endregion

            #region Validation of FCM Doc after tier change

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", custId);

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, $"Failed to retrive the new FCM doc for the customerId {custId}");

            Assert.True(fcmDoc.TierCode == tierName,
                $"In FlyingClubMember doc ,The tiercode after tierchange for the customerId {custId} does not match . Expected {tierName} but found {fcmDoc.TierCode}");

            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(12)]
        public async Task TC12_UpdateMemberPhone_SuccessScenario()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.Status = @status and c.CreatedBy <> @createdBy order by c.UpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@status", "Active").WithParameter("@createdBy", "WEB");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve FCM Doc for test data");

            string memberNumber = fcmDoc.MemberNumber;

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

            Assert.True(customerDoc is not null, $"Failed to retrieve Customer Doc for Customer {customerId}");

            ContactDetails phoneDetails = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone" && x.Primary == "Y");

            string phoneId = phoneDetails.ContactId;

            string updatedBy = DataGenerator.RandomFirstName();

            string operationIdentifier = "Phone";

            string phoneNumber = "93234" + DataGenerator.RandomNumber(10000, 99999, true);

            string status = customerDoc.Status;

            #endregion

            #region API Call 

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.UpdateMemberPhone(operationIdentifier, customerId, memberNumber, phoneId, phoneNumber, updatedBy, status).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(string.IsNullOrEmpty(result?.data?.errorCode.ToString()), $"The Phone Update was not successful. The response returned was {result}");

            #endregion

            #region Validations

            queryForCustomerDoc = $"SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.LastUpdatedBy = @updatedBy ";

            query = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId).WithParameter("@updatedBy", updatedBy);

            customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

            int retry = 5;

            while (retry > 0 && customerDoc is null)
            {
                await Task.Delay(1000);

                customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

                retry--;
            }

            Assert.True(customerDoc is not null, $"Customer Doc not found after the phone update for the customerId {customerId}");

            phoneDetails = customerDoc.ContactDetails.FirstOrDefault(c => c.ContactType == "Phone" && c.Primary == "Y");

            Assert.True(phoneDetails.ContactValue.Equals(phoneNumber),
                $"Expected the Phone Number to be {phoneNumber} but was {phoneDetails.ContactValue}");

            Assert.True(phoneDetails.LastUpdatedOn.Value.Date.Equals(DateTime.UtcNow.Date),
                $"Expected the Phone Number Update Date to be {DateTime.UtcNow.Date} but was {phoneDetails.LastUpdatedOn.Value.Date}");


            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(13)]
        public async Task TC13_UpdateMemberEmail_SuccessScenario()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.Status = @status and c.CreatedBy <> @createdBy order by c.UpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@status", "Active").WithParameter("@createdBy", "WEB");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve FCM Doc for test data");

            string memberNumber = fcmDoc.MemberNumber;

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

            Assert.True(customerDoc is not null, $"Failed to retrieve Customer Doc for Customer {customerId}");

            ContactDetails emailDetails = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Email" && x.Primary == "Y");

            string emailContactId = emailDetails.ContactId;

            string updatedBy = DataGenerator.RandomFirstName();

            string operationIdentifier = "EMAIL";

            string emailAddress = DataGenerator.RandomString(10).ToString().ToLower() + "@testmail.noreply.co.in";

            string updatedOn = DateTime.UtcNow.Date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            #endregion

            #region API Call 

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.UpdateMemberEmail(operationIdentifier, customerId, memberNumber, emailContactId, emailAddress, updatedBy, updatedOn).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(string.IsNullOrEmpty(result?.data?.errorCode.ToString()), $"The Email Update was not successful. The response returned was {result}");

            #endregion

            #region Validations

            queryForCustomerDoc = $"SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.LastUpdatedBy = @updatedBy ";

            query = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId).WithParameter("@updatedBy", updatedBy);

            customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

            int retry = 5;

            while (retry > 0 && customerDoc is null)
            {
                await Task.Delay(1000);

                customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

                retry--;
            }

            Assert.True(customerDoc is not null, $"Customer Doc not found after the phone update for the customerId {customerId}");

            emailDetails = customerDoc.ContactDetails.FirstOrDefault(c => c.ContactType == "Email" && c.Primary == "Y");

            Assert.True(emailDetails.ContactValue.Equals(emailAddress),
                $"Expected the Email Address to be {emailAddress} but was {emailDetails.ContactValue}");

            Assert.True(emailDetails.LastUpdatedOn.Value.Date.Equals(DateTime.UtcNow.Date),
                $"Expected the Email Address Update Date to be {DateTime.UtcNow.Date} but was {emailDetails.LastUpdatedOn.Value.Date}");


            #endregion
        }


        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(14)]
        public async Task TC14_UpdateMemberAddress_SuccessScenario()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.Status = @status and c.CreatedBy <> @createdBy order by c.UpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@status", "Active").WithParameter("@createdBy", "WEB");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve FCM Doc for test data");

            string memberNumber = fcmDoc.MemberNumber;

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

            Assert.True(customerDoc is not null, $"Failed to retrieve Customer Doc for Customer {customerId}");

            Core.DB_Model.Entities.Address addressDetails = customerDoc.AddressDetails.FirstOrDefault(x => x.Primary == "Y");

            string addressId = addressDetails.AddressId;

            string updatedBy = DataGenerator.RandomFirstName();

            string operationIdentifier = "ADDRESS";

            string addressLine1 = "TestAddress" + DataGenerator.RandomString(5).ToString();

            string addressLine2 = "TestAddress" + DataGenerator.RandomString(5).ToString();

            string city = "Crawley";

            string country = "United Kingdom";

            #endregion

            #region API Call 

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.UpdateMemberAddress(operationIdentifier, customerId, memberNumber, addressId, addressLine1, addressLine2, city, country, updatedBy).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(string.IsNullOrEmpty(result?.data?.errorCode.ToString()), $"The Email Update was not successful. The response returned was {result}");

            #endregion

            #region Validations

            queryForCustomerDoc = $"SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.LastUpdatedBy = @updatedBy ";

            query = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId).WithParameter("@updatedBy", updatedBy);

            customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

            int retry = 5;

            while (retry > 0 && customerDoc is null)
            {
                await Task.Delay(1000);

                customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(query);

                retry--;
            }

            Assert.True(customerDoc is not null, $"Customer Doc not found after the phone update for the customerId {customerId}");

            addressDetails = customerDoc.AddressDetails.FirstOrDefault(x => x.Primary == "Y");

            Assert.True(addressDetails.AddressLine1.Equals(addressLine1),
                $"Expected the Address Line 1  to be {addressLine1} but was {addressDetails.AddressLine1}");

            Assert.True(addressDetails.AddressLine2.Equals(addressLine2),
                $"Expected the Address Line 1  to be {addressLine2} but was {addressDetails.AddressLine2}");

            Assert.True(addressDetails.City.Equals(city),
                $"Expected the city to be {addressLine1} but was {addressDetails.City}");

            Assert.True(addressDetails.Country.Equals(country),
                $"Expected the Country to be {country} but was {addressDetails.Country}");

            #endregion
        }

        [Trait("Category", Traits.LOOPSCustomerFEAPIRegression)]
        [Fact, RunOrder(15)]
        public async Task TC15_VredMemberAttributeFlagValidationSuccessScenario()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.CreatedBy <> @createdBy order by c.UpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@createdBy", "WEB");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve FCM Doc for test data");

            string memberNumber = fcmDoc.MemberNumber;

            #endregion

            #region VRED API Validations - API Call 

            HttpResponseMessage vredResponse = LoopsFrontEndAPIEndpoints.VRedMemberStatusAPI(_tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await vredResponse.Content.ReadAsStringAsync());

            bool isRedUserIdExists = !string.IsNullOrEmpty(result.redUserId.ToString());

            Assert.True(isRedUserIdExists, $"The member {memberNumber} selected is not a linked member at VRED end");

            HttpResponseMessage responseFeAPI = LoopsFrontEndAPIEndpoints.GetMember(memberNumber).EnsureIsSuccessStatusCode();

            dynamic? resultFeAPI = JsonConvert.DeserializeObject<dynamic>(await responseFeAPI.Content.ReadAsStringAsync());

            GetMember getMember = JsonConvert.DeserializeObject<GetMember>(resultFeAPI.ToString());

            bool vredMemberFlag = getMember.vredMember;

            #endregion

            #region VRED Flag Valdidations

            if (isRedUserIdExists)
            {
                Assert.True(vredMemberFlag, $"Expected VRED Member Flag atttribute for member id: {memberNumber} is true but retrieved in Get member response as {vredMemberFlag}");
            }
            #endregion
        }

    }
}
