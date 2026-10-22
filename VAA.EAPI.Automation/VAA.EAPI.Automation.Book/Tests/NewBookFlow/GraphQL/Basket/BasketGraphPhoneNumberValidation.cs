using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Basket
{
    public class BasketGraphPhoneNumberValidation : IClassFixture<TokenFixture>, IClassFixture<ReportFixture>
    {
        private ReportFixture reportFixture;
        private readonly TokenFixture tokenFixture;
        private static Random rnd = new Random();
        private readonly TestConfig _testConfig;

        public BasketGraphPhoneNumberValidation(TokenFixture tokenFixture, ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;
            this.tokenFixture = tokenFixture;
            _testConfig = new TestConfig();
        }
    
        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        //Invalid scenarios
        [InlineData("12345678", "INVALID")]           // 8 digits (below minimum)
        [InlineData("123456789012", "INVALID")]       // 12 digits (above maximum)
        [InlineData("", "INVALID")]                   // Empty string (invalid)
        [InlineData("abcdefghij", "INVALID")]         // Non-numeric input (invalid)
        [InlineData("123-456-7890", "INVALID")]       // Contains dashes (invalid)
        [InlineData("1234567a89", "INVALID")]         // Mixed alphanumeric (invalid)
        [InlineData("1234+1234", "INVALID")]          // Mixed alphanumeric (invalid)

        //Valid phone senarios
        [InlineData("01293 123 123", "VALID")] //Space in between numbers
        [InlineData("01293 123123", "VALID")]  //Space in between numbers
        [InlineData("703 123 1234", "VALID")]  //Space in between numbers
        [InlineData("01293123123", "VALID")]   //Valid number leading with zero
        [InlineData("7031231234", "VALID")]    //Valid number 10 digit
        [InlineData("703 1231234", "VALID")]   //Space in between numbers
        public async Task BasketGraphQL_PhoneNumberScenarios(string phoneNumber, string expected)
        {
            // Set up initial data Origin & Destination
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.JFK;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 2, youngAdults: 1, children: 1, infants: 1);
            _testConfig.Book.Contact.PhoneNumber = phoneNumber;

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();
            var contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == _testConfig.Internal.BasketId, "Basket ID dont match");

            var responsePhoneNumber = contactBasketGQLResult.data.updateContactDetails.contactDetails.phoneNumber;

            Assert.True(responsePhoneNumber == phoneNumber.Replace(" ", ""), $"Phone number valdation failed: \n Response Phone number = {responsePhoneNumber} \n Request Phone number = {phoneNumber} ");
            Assert.True(contactBasketGQLResult.data.updateContactDetails.contactDetails.validation.Value == expected, $"Validation value in basketResponse is not {expected}");
        }

        [Fact]
        [Trait("Category", Traits.BookGraphQLRegression)]
        public async Task BasketGraphQL_PhoneNumber_CountryCodeEmpty()
        {
            // Set up initial data Origin & Destination
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.JFK;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 2, youngAdults: 1, children: 1, infants: 1);
            _testConfig.Book.Contact.CountryCode = "";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();
            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == _testConfig.Internal.BasketId, "Basket ID dont match");

            var responsePhoneNumber = contactBasketGQLResult.data.updateContactDetails.contactDetails.phoneNumber;

            Assert.True(responsePhoneNumber == _testConfig.Book.Contact.PhoneNumber, $"Phone number valdation failed: \n Response Phone number = {responsePhoneNumber} \n Request Phone number = {_testConfig.Book.Contact.PhoneNumber} ");
            Assert.True(contactBasketGQLResult.data.updateContactDetails.contactDetails.validation == "INVALID", $"Validation value in basketResponse is not INVALID");
        }
    }
}