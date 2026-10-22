using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    public class PhoneNumberValidation : IClassFixture<TokenFixture>, IClassFixture<ReportFixture>
    {
        private ReportFixture reportFixture;
        private readonly TokenFixture tokenFixture;
        private static Random rnd = new Random();
        private readonly TestConfig _testConfig;

        public PhoneNumberValidation(TokenFixture tokenFixture, ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;
            this.tokenFixture = tokenFixture;
            _testConfig = new TestConfig();
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
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
        public async Task PhoneNumberValidationScenarios(string phoneNumber, string expected)
        {
            // Set up initial data Origin & Destination
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.MCO;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 2, youngAdults: 1, children: 1, infants: 1);
            _testConfig.Book.Contact.PhoneNumber = phoneNumber;

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);
            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);
            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();
            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            // get link to update Contact details from getBasketBFF response
            string link = getBasket["contactDetails"]["link"].ToString();

            //getbasket BFF
            HttpResponseMessage getBasketResponse_BFF = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            //update contact details BFF request
            var updateContactRequest = WebBFFHelper.UpdateContactRequestBff(_testConfig);
            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(_testConfig.Internal.BasketId, link, updateContactRequest).EnsureIsSuccessStatusCode();

            dynamic updateContact = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse_BFF.Content.ReadAsStringAsync());
            var validationField = updateContact["contactDetails"]["validation"].ToString();

            Assert.True(validationField == expected, $"Validation value in basketResponse is not {expected}");
        }

        [Fact]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task ContactCountryCodeEmpty()
        {
            // Set up initial data for card, Origin & Destination
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.MCO;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 2, youngAdults: 1, children: 1, infants: 1);
            _testConfig.Book.Contact.CountryCode = "";

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);
            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);
            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();
            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            // get link to update Contact details from getBasketBFF response
            string link = getBasket["contactDetails"]["link"].ToString();

            //getbasket BFF
            HttpResponseMessage getBasketResponse_BFF = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            //update contact details BFF request
            var updateContactRequest = WebBFFHelper.UpdateContactRequestBff(_testConfig);
            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(_testConfig.Internal.BasketId, link, updateContactRequest).EnsureIsSuccessStatusCode();

            dynamic updateContact = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse_BFF.Content.ReadAsStringAsync());
            var validationField = updateContact["contactDetails"]["validation"].ToString();

            Assert.True(validationField == "INVALID", $"Validation value in basketResponse is not INVALID");
        }
    }
}