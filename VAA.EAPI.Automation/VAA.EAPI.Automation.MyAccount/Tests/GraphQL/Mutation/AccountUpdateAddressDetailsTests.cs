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
using VAA.EAPI.Automation.MyAccount.Models.MemberAccountDetails;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation;

public sealed class AccountUpdateAddressDetailsTests : IClassFixture<CosmosFixture>
{
    private readonly CosmosFixture _cosmosFixture;
    private readonly string _environment = Config.Environment;

    public AccountUpdateAddressDetailsTests(CosmosFixture cosmosFixture)
    {
        _cosmosFixture = cosmosFixture;
        string customerCosmosDb = _environment.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;

        // Register each Customer projection updated by the address-update event before the tests query Cosmos.
        _cosmosFixture.RegisterClient(Application.CustomerCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
        _cosmosFixture.RegisterType<Customer>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        _cosmosFixture.RegisterType<AddressHistory>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        _cosmosFixture.RegisterType<CustomerSearch>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerSearchCosmosCollection);
    }

    [Theory]
    [InlineData(EnrolmentHelper.Country.GB, false, "UK")]
    [InlineData(EnrolmentHelper.Country.US, false, "US")]
    [InlineData(null, false, "NonUKUS")]
    [InlineData(null, true, "NonUKUS")]
    [Trait("Category", Traits.MemberAccountDetailsGraphQl)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public async Task TC01_UpdatesUKUSAndNonUKUSAddresses(EnrolmentHelper.Country? country, bool omitPostcode, string addressCategory)
    {
        (string customerId, string memberNumber) = await EnrolmentHelper.GraphQLEnrolment();
        QueryDefinition customerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType").WithParameter("@customerId", customerId).WithParameter("@docType", Application.DocTypeCustomer);
        QueryDefinition customerSearchQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerSearch c WHERE c.LoyaltyMemberNumber = @memberNumber AND c.DocType = @docType").WithParameter("@memberNumber", memberNumber).WithParameter("@docType", Application.DocTypeCustomerSearch);
        Customer? customerBeforeUpdate = null;

        // Wait for the enrolled member's primary address so the historical document can be checked after the update.
        RetryHelper.RunWithRetry(() =>
        {
            customerBeforeUpdate = _cosmosFixture.CosmosQueryItem<Customer>(customerQuery);
            Assert.True(customerBeforeUpdate is not null, $"Member '{memberNumber}': expected a Customer document after enrolment, but found no document.");
        }, maxRetries: 5, delay: 1000);

        Customer originalCustomer = customerBeforeUpdate!;
        Address? primaryAddressBeforeUpdate = originalCustomer.AddressDetails?.FirstOrDefault(addressDetail => (addressDetail.Primary ?? string.Empty).Is("Y"));
        Assert.True(primaryAddressBeforeUpdate is not null, $"Member '{memberNumber}': expected a primary address after enrolment, but found no address.");
        Address originalAddress = primaryAddressBeforeUpdate!;
        EnrolmentHelper.AddressWithPhoneCode selectedAddress = country.HasValue ? EnrolmentHelper.PickRandomAddress(country.Value) : EnrolmentHelper.PickRandomNonUkUsAddress();

        // The selected address must differ from the current one so the address history result proves an update occurred.
        while (selectedAddress.AddressLine1.Is(originalAddress.AddressLine1))
            selectedAddress = country.HasValue ? EnrolmentHelper.PickRandomAddress(country.Value) : EnrolmentHelper.PickRandomNonUkUsAddress();

        string postalCode = omitPostcode ? string.Empty : selectedAddress.PostalCode;

        using HttpResponseMessage countryResponse = Microservice.GetCountries(_environment).EnsureIsSuccessStatusCode();
        Countries[] countries = JsonConvert.DeserializeObject<Countries[]>(await countryResponse.Content.ReadAsStringAsync()) ?? Array.Empty<Countries>();
        Countries? requestedCountry = countries.SingleOrDefault(countryDetails => countryDetails.iataCountryCode.Is(selectedAddress.CountryCode));
        Assert.True(requestedCountry is not null, $"Member '{memberNumber}': expected country '{selectedAddress.CountryCode}' in reference data for the {addressCategory} scenario, but found no country.");
        string expectedCountryName = requestedCountry!.countryName;
        Assert.True(!expectedCountryName.IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a populated name for country '{selectedAddress.CountryCode}', but found '{(expectedCountryName.IsNullOrWhiteSpace() ? "<empty>" : expectedCountryName)}'.");

        string token = await EntraTokenHelper.GetIdToken(_environment, string.Empty, memberNumber, Application.LoginPassword);
        AccountAddressDetail addressDetail = new()
        {
            AddressLine1 = selectedAddress.AddressLine1,
            AddressLine2 = selectedAddress.AddressLine2,
            AddressLine3 = "Automation address line 3",
            AddressType = "Home",
            Region = string.Empty,
            PreferredFlag = "Y",
            PostalCode = postalCode,
            Country = selectedAddress.CountryCode,
            CompanyName = "Virgin Atlantic Automation",
            City = selectedAddress.City,
            AddressVerified = true
        };
        AccountUpdateAddressDetails request = new() { UpdateAddressDetail = new UpdateMemberAddressRequestInput { AccountAddressDetail = addressDetail, MemberType = "Individual" } };

        // GraphQL errors are returned in a successful HTTP envelope, so validate the GraphQL result before Cosmos checks.
        using HttpResponseMessage response = GraphQL.AccountUpdateAddressDetails(_environment, token, request).EnsureGQLResponseSuccess();
        JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(responseBody.RequireToken("data.accountUpdateAddressDetails").ToString().Is("Updated Member Address Successfully"), $"Member '{memberNumber}': expected address-update response 'Updated Member Address Successfully', but found '{responseBody.RequireToken("data.accountUpdateAddressDetails")}'.");

        Customer? customerAfterUpdate = null;
        CustomerSearch? customerSearchAfterUpdate = null;

        // Poll both projections because the Customer service writes them asynchronously from the address-update event.
        RetryHelper.RunWithRetry(() =>
        {
            customerAfterUpdate = _cosmosFixture.CosmosQueryItem<Customer>(customerQuery);
            customerSearchAfterUpdate = _cosmosFixture.CosmosQueryItem<CustomerSearch>(customerSearchQuery);
            Address? primaryAddress = customerAfterUpdate?.AddressDetails?.FirstOrDefault(address => (address.Primary ?? string.Empty).Is("Y"));
            Assert.True(primaryAddress?.AddressLine1.Is(addressDetail.AddressLine1) == true && customerSearchAfterUpdate?.Country.Is(expectedCountryName) == true && customerSearchAfterUpdate.PostCode.Is(postalCode.Replace("-", string.Empty, StringComparison.Ordinal)) == true, $"Member '{memberNumber}': expected Customer address line '{addressDetail.AddressLine1}', CustomerSearch country '{expectedCountryName}', and postcode '{postalCode.Replace("-", string.Empty, StringComparison.Ordinal)}', but found address line '{primaryAddress?.AddressLine1 ?? "<missing>"}', country '{customerSearchAfterUpdate?.Country ?? "<missing>"}', and postcode '{customerSearchAfterUpdate?.PostCode ?? "<missing>"}'.");
        }, maxRetries: 5, delay: 1000);

        Customer updatedCustomer = customerAfterUpdate!;
        CustomerSearch updatedCustomerSearch = customerSearchAfterUpdate!;
        Address? updatedPrimaryAddress = updatedCustomer.AddressDetails?.FirstOrDefault(address => (address.Primary ?? string.Empty).Is("Y"));
        Assert.True(updatedPrimaryAddress is not null, $"Member '{memberNumber}': expected a primary address after the update, but found no address.");
        Address updatedAddress = updatedPrimaryAddress!;
        string expectedPostCode = postalCode.Replace("-", string.Empty, StringComparison.Ordinal);
        SoftAssertHelper softAssert = new();

        softAssert.True(updatedAddress.AddressType.Is(addressDetail.AddressType), $"Member '{memberNumber}': expected Customer address type '{addressDetail.AddressType}', but found '{updatedAddress.AddressType}'.");
        softAssert.True(updatedAddress.AddressLine1.Is(addressDetail.AddressLine1), $"Member '{memberNumber}': expected Customer address line 1 '{addressDetail.AddressLine1}', but found '{updatedAddress.AddressLine1}'.");
        softAssert.True((updatedAddress.AddressLine2 ?? string.Empty).Is(addressDetail.AddressLine2), $"Member '{memberNumber}': expected Customer address line 2 '{addressDetail.AddressLine2}', but found '{updatedAddress.AddressLine2 ?? "<empty>"}'.");
        softAssert.True((updatedAddress.AddressLine3 ?? string.Empty).Is(addressDetail.AddressLine3), $"Member '{memberNumber}': expected Customer address line 3 '{addressDetail.AddressLine3}', but found '{updatedAddress.AddressLine3 ?? "<empty>"}'.");
        softAssert.True((updatedAddress.LandlordOrCompanyName ?? string.Empty).Is(addressDetail.CompanyName), $"Member '{memberNumber}': expected Customer company name '{addressDetail.CompanyName}', but found '{updatedAddress.LandlordOrCompanyName ?? "<empty>"}'.");
        softAssert.True(updatedAddress.City.Is(addressDetail.City), $"Member '{memberNumber}': expected Customer city '{addressDetail.City}', but found '{updatedAddress.City}'.");
        softAssert.True(updatedAddress.PostCode.Is(expectedPostCode), $"Member {memberNumber}: Customer postcode was not updated.");
        softAssert.True(updatedAddress.Province.Is(addressDetail.Region), $"Member {memberNumber}: Customer region was not updated.");
        softAssert.True(updatedAddress.Country.Is(expectedCountryName), $"Member {memberNumber}: Customer country was not updated.");
        softAssert.True(updatedAddress.Primary.Is(addressDetail.PreferredFlag), $"Member {memberNumber}: Customer primary-address flag was not updated.");
        softAssert.True(updatedAddress.AddressVerified.Is("Y"), $"Member {memberNumber}: Customer address verified flag was not updated.");
        softAssert.True(updatedCustomerSearch.PostCode.Is(expectedPostCode), $"Member {memberNumber}: CustomerSearch postcode was not updated.");
        softAssert.True(updatedCustomerSearch.Province.Is(addressDetail.Region), $"Member {memberNumber}: CustomerSearch region was not updated.");
        softAssert.True(updatedCustomerSearch.Country.Is(expectedCountryName), $"Member {memberNumber}: CustomerSearch country was not updated.");

        QueryDefinition addressHistoryQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType AND c.AddressLine1 = @addressLine1 ORDER BY c._ts DESC").WithParameter("@customerId", customerId).WithParameter("@docType", "AddressHistory").WithParameter("@addressLine1", originalAddress.AddressLine1);
        AddressHistory? addressHistory = null;

        // The Customer service creates history before replacing the active primary address.
        RetryHelper.RunWithRetry(() =>
        {
            addressHistory = _cosmosFixture.CosmosQueryItem<AddressHistory>(addressHistoryQuery);
            Assert.True(addressHistory is not null, $"Member '{memberNumber}': expected previous address history after the update, but found no document.");
        }, maxRetries: 5, delay: 1000);

        AddressHistory previousAddressHistory = addressHistory!;
        softAssert.True(previousAddressHistory.AddressLine1.Is(originalAddress.AddressLine1), $"Member {memberNumber}: address history did not preserve the previous address line 1.");
        softAssert.True(previousAddressHistory.City.Is(originalAddress.City), $"Member {memberNumber}: address history did not preserve the previous city.");
        softAssert.True(previousAddressHistory.Province.Is(originalAddress.Province), $"Member {memberNumber}: address history did not preserve the previous region.");
        softAssert.True(previousAddressHistory.PostCode.Is(originalAddress.PostCode), $"Member {memberNumber}: address history did not preserve the previous postcode.");
        softAssert.True(previousAddressHistory.Country.Is(originalAddress.Country), $"Member {memberNumber}: address history did not preserve the previous country.");
        softAssert.AssertAll();
    }

    [Theory]
    [InlineData(EnrolmentHelper.Country.GB, "UK")]
    [InlineData(EnrolmentHelper.Country.US, "US")]
    [Trait("Category", Traits.MemberAccountDetailsGraphQl)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    public async Task TC02_RejectsMissingPostcodeForUKAndUS(EnrolmentHelper.Country country, string addressCategory)
    {
        string memberNumber = (await EnrolmentHelper.GraphQLEnrolment()).Item2;
        EnrolmentHelper.AddressWithPhoneCode address = EnrolmentHelper.PickRandomAddress(country);
        string token = await EntraTokenHelper.GetIdToken(_environment, string.Empty, memberNumber, Application.LoginPassword);
        AccountUpdateAddressDetails request = new()
        {
            UpdateAddressDetail = new UpdateMemberAddressRequestInput
            {
                AccountAddressDetail = new AccountAddressDetail { AddressLine1 = address.AddressLine1, AddressLine2 = address.AddressLine2, AddressType = "Home", PreferredFlag = "Y", PostalCode = string.Empty, Country = address.CountryCode, CompanyName = "Virgin Atlantic Automation", City = address.City, AddressVerified = true },
                MemberType = "Individual"
            }
        };

        // UK and US are the only configured postcode-mandatory countries in the GraphQL resolver.
        using HttpResponseMessage response = GraphQL.AccountUpdateAddressDetails(_environment, token, request);
        JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();

        softAssert.True(response.IsSuccessStatusCode, $"Member {memberNumber}: expected HTTP 200 with a GraphQL validation error for the {addressCategory} scenario.");
        softAssert.True(responseBody.SelectToken("data.accountUpdateAddressDetails") is null or { Type: JTokenType.Null }, $"Member {memberNumber}: the {addressCategory} request without a postcode unexpectedly returned data.");
        softAssert.True(responseBody.RequireToken("errors[0].message").ToString().Is("PostalCode Missing"), $"Member {memberNumber}: the {addressCategory} missing-postcode error message was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.code").ToString().Is("15001"), $"Member {memberNumber}: the {addressCategory} missing-postcode error code was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.message").ToString().Is("One of the mandatory field is missing in the request"), $"Member {memberNumber}: the {addressCategory} missing-postcode extension message was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.source").ToString().Is("Consuming application"), $"Member {memberNumber}: the {addressCategory} missing-postcode error source was incorrect.");
        softAssert.AssertAll();
    }

}
