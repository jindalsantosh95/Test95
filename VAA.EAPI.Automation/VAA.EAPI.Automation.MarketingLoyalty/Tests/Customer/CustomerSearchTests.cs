using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;
using ContactDetails = VAA.EAPI.Automation.Core.DB_Model.Entities.ContactDetails;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Customer
{
    [Collection("CustomerSearchTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CustomerSearchTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public CustomerSearchTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<Address>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<ContactDetails>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(1)]
        public async Task A_CustomerSearch_SuccessScenario_UsingCustomerId()
        {
            #region

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            #endregion

            HttpResponseMessage response = GraphQL.CustomerSearch(tokenFixture.GetToken("graphqlv2"), customerId, "");

            dynamic customerSearchResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string queryForCustomerDocument = $"SELECT * FROM CustomerCollection c WHERE  c.DocType = 'Customer' and c.PartitionKey='{customerId}' order by c._ts desc";

            Core.DB_Model.Entities.Customer customer = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDocument);

            int retry = 5;

            while (retry > 0 && customer == null)
            {
                await Task.Delay(500);

                customer = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDocument);

                retry--;
            }

            Assert.True(customer.FirstName.Equals(customerSearchResult.data.loyaltyCustomer.firstName.ToString()),
                $"Expected the Customer FirstName as {customer.FirstName} but found {customerSearchResult.data.loyaltyCustomer.firstName.ToString()}.");

            Assert.True(customer.LastName.Equals(customerSearchResult.data.loyaltyCustomer.lastName.ToString()),
                $"Expected the Customer LastName as {customer.LastName} but found {customerSearchResult.data.loyaltyCustomer.lastName.ToString()}.");

            Assert.True(customer.LoyaltyMemberNumber.Equals(customerSearchResult.data.loyaltyCustomer.loyaltyNumber.ToString()),
               $"Expected the Customer FC Number as {customer.LoyaltyMemberNumber} but found {customerSearchResult.data.loyaltyCustomer.loyaltyNumber.ToString()}.");

            Assert.True(customer.DateOfBirth.Value.Date.ToString("dd-MMM-yyyy").Equals(DateTime.Parse(customerSearchResult.data.loyaltyCustomer.dateOfBirth.ToString()).ToString("dd-MMM-yyyy")),
                $"Expected the Customer DateOfBirth as {customer.DateOfBirth} but found {customerSearchResult.ToString()}.");

            Address[] address = customer.AddressDetails;

            if (address[0].Primary == "Y")
            {

                Assert.True(address[0].AddressType.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressType.ToString()),
                  $"Expected the Customer Primary AddressType as {address[0].AddressType} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressType.ToString()}.");

                Assert.True(address[0].AddressLine1.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine1.ToString()),
                 $"Expected the Customer AddressLine 1 as {address[0].AddressLine1} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine1.ToString()}.");

                string addressLine2 = string.IsNullOrEmpty(address[0].AddressLine2) ? "" : address[0].AddressLine2;

                Assert.True(addressLine2.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine2.ToString()),
                 $"Expected the Customer AddressLine 2 as {addressLine2} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine2.ToString()}.");

                string addressLine3 = string.IsNullOrEmpty(address[0].AddressLine3) ? "" : address[0].AddressLine3;

                Assert.True(addressLine3.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine3.ToString()),
                 $"Expected the Customer AddressLine 3 as {addressLine3} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine3.ToString()}.");

                Assert.True(address[0].City.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].city.ToString()),
                 $"Expected the Customer City as {address[0].City} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].city.ToString()}.");

                Assert.True(address[0].Country.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].country.ToString()),
                 $"Expected the Customer Country as {address[0].Country} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].country.ToString()}.");

            }

            ContactDetails[] contactDetails = customer.ContactDetails;

            for (int i = 0; i < contactDetails.Length; i++)
            {
                if (contactDetails[i].ContactType == "Phone" && contactDetails[i].Primary == "Y")
                {
                    Assert.True(contactDetails[i].ContactValue.Equals(customerSearchResult.data.loyaltyCustomer.contactDetails.phoneDetails[0].phoneNumber.ToString()),
                  $"Expected the Customer Phone Number  as {contactDetails[i].ContactValue} but found {customerSearchResult.data.loyaltyCustomer.contactDetails.phoneDetails[0].phoneNumber.ToString()}.");

                }
                else if (contactDetails[i].ContactType == "Email" && contactDetails[i].Primary == "Y")
                {
                    Assert.True(contactDetails[i].ContactValue.Equals(customerSearchResult.data.loyaltyCustomer.contactDetails.emailDetails[0].EmailAddress.ToString()),
                 $"Expected the Customer Email Address as {contactDetails[i].ContactValue} but found {customerSearchResult.data.loyaltyCustomer.contactDetails.emailDetails[0].EmailAddress.ToString()}.");

                    Assert.True(contactDetails[i].Valid.Equals(customerSearchResult.data.loyaltyCustomer.contactDetails.emailDetails[0].Flag_to_indicate_if_Email_is_valid.ToString()),
                 $"Expected the Customer Email Address Valid Flag as {contactDetails[i].Valid} but found {customerSearchResult.data.loyaltyCustomer.contactDetails.emailDetails[0].Flag_to_indicate_if_Email_is_valid.ToString()}.");


                }
            }

            Assert.True(fcmDoc.Status.Equals(customerSearchResult.data.loyaltyCustomer.status.ToString()),
                $"Expected the Customer Account Status as {fcmDoc.Status} but found {customerSearchResult.data.loyaltyCustomer.status.ToString()}.");

            Assert.True(fcmDoc.MemberType.Equals(customerSearchResult.data.loyaltyCustomer.memberAccountType.ToString()),
               $"Expected the Customer Account Type as {fcmDoc.MemberType} but found {customerSearchResult.data.loyaltyCustomer.memberAccountType.ToString()}.");

            Assert.True(fcmDoc.HeadOfGroupMemberNumber.Equals(customerSearchResult.data.loyaltyCustomer.headOfGroupMemberNumber.ToString()),
               $"Expected the Customer field valueHeadOfGroupMemberNumber  as {fcmDoc.HeadOfGroupMemberNumber} but found {customerSearchResult.data.loyaltyCustomer.headOfGroupMemberNumber.ToString()}.");

            string tAndCFlag = fcmDoc.TermsAndConditionsFlag.Equals("Y") ? "True" : "False";

            Assert.True(tAndCFlag.Equals(customerSearchResult.data.loyaltyCustomer.termsAndConditionsAccepted.ToString()),
               $"Expected the Customer T&C Conditions Flag as {tAndCFlag} but found {customerSearchResult.data.loyaltyCustomer.termsAndConditionsAccepted.ToString()}.");


        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(2)]
        public async Task B_CustomerSearch_SuccessScenario_UsingMemberNumber()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string memberNumber = fcmDoc.MemberNumber;

            string customerId = fcmDoc.CustomerId;

            #endregion

            HttpResponseMessage response = GraphQL.CustomerSearch(tokenFixture.GetToken("graphqlv2"), "", memberNumber);

            dynamic customerSearchResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string queryForCustomerDocument = $"SELECT * FROM CustomerCollection c WHERE  c.DocType = 'Customer' and c.LoyaltyMemberNumber='{memberNumber}' order by c._ts desc";

            Core.DB_Model.Entities.Customer customer = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDocument);

            Assert.True(customer.FirstName.Equals(customerSearchResult.data.loyaltyCustomer.firstName.ToString()),
                $"Expected the Customer FirstName as {customer.FirstName} but found {customerSearchResult.data.loyaltyCustomer.firstName.ToString()}.");

            Assert.True(customer.LastName.Equals(customerSearchResult.data.loyaltyCustomer.lastName.ToString()),
                $"Expected the Customer LastName as {customer.LastName} but found {customerSearchResult.data.loyaltyCustomer.lastName.ToString()}.");

            Assert.True(customer.LoyaltyMemberNumber.Equals(customerSearchResult.data.loyaltyCustomer.loyaltyNumber.ToString()),
               $"Expected the Customer FC Number as {customer.LoyaltyMemberNumber} but found {customerSearchResult.data.loyaltyCustomer.loyaltyNumber.ToString()}.");

            Assert.True(customer.DateOfBirth.Value.Date.ToString("dd-MMM-yyyy").Equals(DateTime.Parse(customerSearchResult.data.loyaltyCustomer.dateOfBirth.ToString()).ToString("dd-MMM-yyyy")),
               $"Expected the Customer DateOfBirth as {customer.DateOfBirth} but found {customerSearchResult.ToString()}.");

            Address[] address = customer.AddressDetails;

            if (address[0].Primary == "Y")
            {
                Assert.True(address[0].AddressType.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressType.ToString()),
                  $"Expected the Customer Primary AddressType as {address[0].AddressType} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressType.ToString()}.");

                Assert.True(address[0].AddressLine1.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine1.ToString()),
                 $"Expected the Customer AddressLine 1 as {address[0].AddressLine1} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine1.ToString()}.");

                string addressLine2 = string.IsNullOrEmpty(address[0].AddressLine2) ? "" : address[0].AddressLine2;

                Assert.True(addressLine2.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine2.ToString()),
                 $"Expected the Customer AddressLine 2 as {addressLine2} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine2.ToString()}.");

                string addressLine3 = string.IsNullOrEmpty(address[0].AddressLine3) ? "" : address[0].AddressLine3;

                Assert.True(addressLine3.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine3.ToString()),
                 $"Expected the Customer AddressLine 3 as {addressLine3} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].addressLine3.ToString()}.");

                Assert.True(address[0].City.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].city.ToString()),
                 $"Expected the Customer City as {address[0].City} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].city.ToString()}.");

                Assert.True(address[0].Country.Equals(customerSearchResult.data.loyaltyCustomer.addressDetails[0].country.ToString()),
                 $"Expected the Customer Country as {address[0].Country} but found {customerSearchResult.data.loyaltyCustomer.addressDetails[0].country.ToString()}.");

            }

            ContactDetails[] contactDetails = customer.ContactDetails;


            for (int i = 0; i < contactDetails.Length; i++)
            {
                if (contactDetails[i].ContactType == "Phone" && contactDetails[i].Primary == "Y")
                {
                    Assert.True(contactDetails[i].ContactValue.Equals(customerSearchResult.data.loyaltyCustomer.contactDetails.phoneDetails[0].phoneNumber.ToString()),
                  $"Expected the Customer Phone Number  as {contactDetails[i].ContactValue} but found {customerSearchResult.data.loyaltyCustomer.contactDetails.phoneDetails[0].phoneNumber.ToString()}.");

                }
                else if (contactDetails[i].ContactType == "Email" && contactDetails[i].Primary == "Y")
                {
                    Assert.True(contactDetails[i].ContactValue.Equals(customerSearchResult.data.loyaltyCustomer.contactDetails.emailDetails[0].EmailAddress.ToString()),
                 $"Expected the Customer Email Address as {contactDetails[i].ContactValue} but found {customerSearchResult.data.loyaltyCustomer.contactDetails.emailDetails[0].EmailAddress.ToString()}.");

                    Assert.True(contactDetails[i].Valid.Equals(customerSearchResult.data.loyaltyCustomer.contactDetails.emailDetails[0].Flag_to_indicate_if_Email_is_valid.ToString()),
                 $"Expected the Customer Email Address Valid Flag as {contactDetails[i].Valid} but found {customerSearchResult.data.loyaltyCustomer.contactDetails.emailDetails[0].Flag_to_indicate_if_Email_is_valid.ToString()}.");


                }
            }


            Assert.True(fcmDoc.Status.Equals(customerSearchResult.data.loyaltyCustomer.status.ToString()),
                $"Expected the Customer Account Status as {fcmDoc.Status} but found {customerSearchResult.data.loyaltyCustomer.status.ToString()}.");

            Assert.True(fcmDoc.MemberType.Equals(customerSearchResult.data.loyaltyCustomer.memberAccountType.ToString()),
               $"Expected the Customer Account Type as {fcmDoc.MemberType} but found {customerSearchResult.data.loyaltyCustomer.memberAccountType.ToString()}.");

            Assert.True(fcmDoc.HeadOfGroupMemberNumber.Equals(customerSearchResult.data.loyaltyCustomer.headOfGroupMemberNumber.ToString()),
               $"Expected the Customer field valueHeadOfGroupMemberNumber  as {fcmDoc.HeadOfGroupMemberNumber} but found {customerSearchResult.data.loyaltyCustomer.headOfGroupMemberNumber.ToString()}.");

            string tAndCFlag = fcmDoc.TermsAndConditionsFlag.Equals("Y") ? "True" : "False";

            Assert.True(tAndCFlag.Equals(customerSearchResult.data.loyaltyCustomer.termsAndConditionsAccepted.ToString()),
               $"Expected the Customer T&C Conditions Flag as {tAndCFlag} but found {customerSearchResult.data.loyaltyCustomer.termsAndConditionsAccepted.ToString()}.");


        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(3)]
        public async Task C_CustomerSearch_MandatoryFieldMissingError_WhenBothCustomerIdAndMemberNumberAreEmptyOrNull()
        {

            HttpResponseMessage response = GraphQL.CustomerSearch(tokenFixture.GetToken("graphqlv2"), "", "");

            dynamic customerSearchResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Either 'Customer ID' or 'Member Number' must be specified";

            Assert.True(expectedResponse.Equals(customerSearchResult.errors[0].extensions.Exception.ToString()),
                $"Expected the Error message as {expectedResponse} but found {customerSearchResult.errors[0].extensions.Exception.ToString()}.");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(4)]
        public async Task D_CustomerSearch_InvalidFieldValueError_WhenCustomerIdIsInvalid()
        {

            HttpResponseMessage response = GraphQL.CustomerSearch(tokenFixture.GetToken("graphqlv2"), "fbce3904-e27a-4508-aa3c-635f12b161", "");

            dynamic customerSearchResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unable to retrieve Customer details [Http connector failed.]";

            Assert.True(customerSearchResult.errors[0].message.ToString().Contains(expectedResponse),
                $"Expected the error message {expectedResponse} but found {customerSearchResult.errors[0].message.ToString()}.");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(5)]
        public async Task E_CustomerSearch_InvalidFieldValueError_WhenMemberNumberIsInvalid()
        {

            HttpResponseMessage response = GraphQL.CustomerSearch(tokenFixture.GetToken("graphqlv2"), "", "1009800123");

            dynamic customerSearchResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unable to retrieve Customer details [Http connector failed.]";

            Assert.True(customerSearchResult.errors[0].message.ToString().Contains(expectedResponse),
                $"Expected the Error message as {expectedResponse} but found {customerSearchResult.errors[0].message.ToString()}.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(6)]
        public async Task F_CustomerSearch_AwardDocumentValidation()
        {
            string queryForAwardDocument = "SELECT * FROM CustomerDetailsCollection c WHERE c.DocType = 'Award' order by c.CreatedOn desc";

            Award award = cosmosFixture.QueryItem<Award>(queryForAwardDocument);

            int retry = 5;

            while (retry > 0 && award == null)
            {
                await Task.Delay(500);

                award = cosmosFixture.QueryItem<Award>(queryForAwardDocument);

                retry--;
            }

            Assert.True(award is not null, "Didn't find any member with any of 3(LTG,MM,Wings) awards/status flags assigned.");

            HttpResponseMessage response = GraphQL.CustomerSearch(tokenFixture.GetToken("graphqlv2"), award.CustomerId, "");

            dynamic customerSearchResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            queryForAwardDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'Award' and c.PartitionKey='{award.CustomerId}'";

            List<Award> awards = cosmosFixture.QueryItems<Award>(queryForAwardDocument).ToList();

            retry = 5;

            while (retry > 0 && award == null)
            {
                await Task.Delay(500);

                awards = cosmosFixture.QueryItems<Award>(queryForAwardDocument).ToList();

                retry--;
            }

            List<AwardDetails> awardDetails = JsonConvert.DeserializeObject<List<AwardDetails>>(customerSearchResult.data.loyaltyCustomer.awardDetails.ToString());

            Assert.True(awards.Count().Equals(awardDetails.Count()),
                $"The Award Document Count Does not Match. Expected the Count as {awards.Count()} but found {awardDetails.Count()} in the response. ");

            for (int i = 0; i < awardDetails.Count(); i++)
            {
                Assert.True(awards[i].AwardId == awardDetails[i].awardId, $"Expected the Award Id as {awards[i].AwardId} but found {awardDetails[i].awardId}.");

                Assert.True(awards[i].AwardNumber == awardDetails[i].awardNumber, $"Expected the Award Number as {awards[i].AwardNumber} but found {awardDetails[i].awardNumber}.");

                string expectedStartDate = DateTime.MinValue == awards[i].StartDate ? "" : awards[i]?.StartDate.ToString("dd-MMM-yyyy");

                string actualStartDate = awardDetails[i].startDate.HasValue ? awardDetails[i].startDate.Value.ToString("dd-MMM-yyyy") : "";

                Assert.True(expectedStartDate == actualStartDate, $"Expected the Award StartDate as {expectedStartDate} but found {actualStartDate}.");

                string expectedExpiryDate = DateTime.MinValue == awards[i].ExpiryDate ? "" : awards[i]?.ExpiryDate.ToString("dd-MMM-yyyy");

                string actualExpiryDate = awardDetails[i].expiryDate.HasValue ? awardDetails[i].expiryDate.Value.ToString("dd-MMM-yyyy") : "";

                Assert.True(expectedExpiryDate == actualExpiryDate, $"Expected the Award EndDate as {expectedExpiryDate} but found {actualExpiryDate}.");

                if (!string.IsNullOrEmpty(awards[i].ProductId))
                {
                    string strInputQueryForProductDetails = $"Select ProductName from dbo.Product where RowId='{awards[i].ProductId}'";

                    string productName = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForProductDetails);

                    Assert.True(productName == awardDetails[i].productName, $"Expected the Award ProductName as {productName} but found {awardDetails[i].productName}.");
                }

            }
        }
    }
}
