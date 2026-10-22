using AventStack.ExtentReports.Gherkin.Model;
using Microsoft.Graph.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorEngine.Compilation.ImpromptuInterface;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.Microservices.Book;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    public class PassengerValidation : IClassFixture<TokenFixture>, IClassFixture<ReportFixture>
    {
        private ReportFixture reportFixture;
        private readonly TokenFixture tokenFixture;
        private static Random rnd = new Random();
        private readonly TestConfig _testConfig;

        public PassengerValidation(TokenFixture tokenFixture, ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;
            this.tokenFixture = tokenFixture;
            _testConfig = new TestConfig();
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(30, 35, "VALID")] // Valid First Name with exactly 30 characters and Valid Last Name with exactly 34 characters
        [InlineData(1, 2, "VALID")] // Valid First Name with Minimum Length and Valid Last Name with Minimum Length
        [InlineData(31, 37, "INVALID")] // INValid - First Name exceeds 30 characters and Last Name exceeds 35 characters
        public async Task PassengerNameLengthValidator(int firstNameLength, int lastNameLength, string expected)
        {
            // Set up initial data Origin & Destination
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.JFK;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, youngAdults: 1, children: 1, infants: 1);

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

            // get link to update Contact details from getBasketBFF response
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            HttpResponseMessage updatePassengerResponse_BFF = null;

            foreach (var item in (JArray)getBasket["people"])
            {
                _testConfig.Book.Passenger.PeopleLink = item["link"].ToString();
                _testConfig.Book.Passenger.Type = item["type"].ToString();
                _testConfig.Book.Passenger.FirstName = DataGenerator.RandomString(firstNameLength);
                _testConfig.Book.Passenger.LastName = DataGenerator.RandomString(lastNameLength);

                //update basket Passenger details BFF request
                _testConfig.Book.Passenger.DateOfBirth = _testConfig.Book.Passenger.GenerateDateOfBirth(_testConfig.Book.Passenger.Type);

                var updatePassengerRequest = WebBFFHelper.UpdatePassengerRequestBff(_testConfig);
                updatePassengerResponse_BFF = WebBFFEndpoint.UpdatePassengerBFF(_testConfig.Internal.BasketId, _testConfig.Book.Passenger.PeopleLink, updatePassengerRequest).EnsureIsSuccessStatusCode();
            }
            dynamic passengerBasket = JsonConvert.DeserializeObject<dynamic>(updatePassengerResponse_BFF.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(((JArray)passengerBasket.people).All(x => x["validation"].ToString().Equals(expected, StringComparison.OrdinalIgnoreCase)), $"VALID was not returned as expected for People \n Basket Passenger respons: \n {passengerBasket}");

            //update contact details BFF request
            var updateContactRequest = WebBFFHelper.UpdateContactRequestBff(_testConfig);
            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(_testConfig.Internal.BasketId, link, updateContactRequest).EnsureIsSuccessStatusCode();
            dynamic updateContact = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse_BFF.Content.ReadAsStringAsync());
            
            //Verify Contact validation
            var validationField = updateContact["contactDetails"]["validation"].ToString();
            Assert.True(validationField == "VALID", $"Validation value in Contact details basket is not VALID");

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);
            HttpResponseMessage tncResponse = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();
            dynamic updateTnCResult = JsonConvert.DeserializeObject<dynamic>(await tncResponse.Content.ReadAsStringAsync());

            //Verify Terms and condition validation
            var validationTNC = updateTnCResult["termsAndConditions"]["validation"].ToString();
            Assert.True(validationTNC == "VALID", $"Validation value in Term and Condition basket is not VALID");

            //Get the basket from the BFF
            getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();
            getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            //Verify Terms and condition validation
            var validation = getBasket["validation"].ToString();
            Assert.True(validation == expected, $"Validation value in basket response is not {expected}");

        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("Anne-Marie", "Pinky-Marie", "VALID")] // Valid Name with Special Characters/Hyphen for First Name and Last Name
        [InlineData("Anne Marie", "Pinky Marie", "VALID")] // Valid Name with Spaces for First Name and Last Name
        [InlineData("John123", "Doe456", "INVALID")] // Valid - First Name and Last name with Numeric Values
        [InlineData("@John$", "#Doe%", "VALID")] // Valid - First Name and Last name with special characters should be cleared in API
        [InlineData("Check fifte   cha", "Check fifte chaa", "VALID")] // Valid - First Name and Last name with Space
        [InlineData("Check O'fif", "Check O'e", "VALID")] // Valid - First Name and Last name with Space and special character '
        [InlineData(" ", " ", "INVALID")] // InValid - Whitespace only input for First Name and Last name
        public async Task PassengerNameValidator(string firstName, string lastName, string expected)
        {
            // Set up initial data Origin & Destination
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.JFK;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, youngAdults: 1, children: 1, infants: 1);

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

            // get link to update Contact details from getBasketBFF response
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            HttpResponseMessage updatePassengerResponse_BFF = null;

            foreach (var item in (JArray)getBasket["people"])
            {
                _testConfig.Book.Passenger.PeopleLink = item["link"].ToString();
                _testConfig.Book.Passenger.Type = item["type"].ToString();
                _testConfig.Book.Passenger.FirstName = firstName;
                _testConfig.Book.Passenger.LastName = lastName;

                //update basket Passenger details BFF request
                _testConfig.Book.Passenger.DateOfBirth = _testConfig.Book.Passenger.GenerateDateOfBirth(_testConfig.Book.Passenger.Type);

                var updatePassengerRequest = WebBFFHelper.UpdatePassengerRequestBff(_testConfig);
                updatePassengerResponse_BFF = WebBFFEndpoint.UpdatePassengerBFF(_testConfig.Internal.BasketId, _testConfig.Book.Passenger.PeopleLink, updatePassengerRequest).EnsureIsSuccessStatusCode();
            }
            dynamic passengerBasket = JsonConvert.DeserializeObject<dynamic>(updatePassengerResponse_BFF.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(((JArray)passengerBasket.people).All(x => x["validation"].ToString().Equals(expected, StringComparison.OrdinalIgnoreCase)), $"VALID was not returned as expected for People \n Basket Response : \n {passengerBasket}");

            //update contact details BFF request
            var updateContactRequest = WebBFFHelper.UpdateContactRequestBff(_testConfig);
            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(_testConfig.Internal.BasketId, link, updateContactRequest).EnsureIsSuccessStatusCode();
            dynamic updateContact = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse_BFF.Content.ReadAsStringAsync());

            //Verify Contact validation
            var validationField = updateContact["contactDetails"]["validation"].ToString();
            Assert.True(validationField == "VALID", $"Validation value in Contact details basket is not VALID");

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);
            HttpResponseMessage tncResponse = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();
            dynamic updateTnCResult = JsonConvert.DeserializeObject<dynamic>(await tncResponse.Content.ReadAsStringAsync());

            //Verify Terms and condition validation
            var validationTNC = updateTnCResult["termsAndConditions"]["validation"].ToString();
            Assert.True(validationTNC == "VALID", $"Validation value in Term and Condition basket is not VALID");

            //Get the basket from the BFF
            getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();
            getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            //Verify Terms and condition validation
            var validation = getBasket["validation"].ToString();
            Assert.True(validation == expected, $"Validation value in basket response is not {expected}");
        }

        [Theory]
        [InlineData(2, 0, 0, 0)]
        [InlineData(2, 1, 1, 1)]
        [InlineData(2, 2, 1, 1)]
        [InlineData(1, 1, 1, 1)]
        [InlineData(0, 1, 1, 0)]
        [InlineData(1, 0, 0, 1)]
        [InlineData(2, 2, 0, 1)]
        [InlineData(2, 0, 2, 0)]
        [InlineData(0, 1, 0, 0)]
        [InlineData(0, 0, 1, 0)]
        [InlineData(2, 2, 2, 2)]
        [InlineData(4, 0, 0, 0)]
        [InlineData(4, 1, 4, 1)]
        [InlineData(4, 2, 2, 0)]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task ReturnBasketPtcValidation(int adults, int teens, int child, int infants)
        {
            // Set up initial data Origin & Destination
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.JFK;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: (adults-1), youngAdults: teens, children: child, infants: infants);

            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(_testConfig);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "", _testConfig.Internal.Bearer).EnsureIsSuccessStatusCode();
            dynamic offerSearchResults = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateSearchResult(offerSearchResults, offerSearchGQLRequest, out string result), result);

            //get basket id from search graphql response body
            _testConfig.Internal.BasketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);
            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForAirlineAndFareFamily(_testConfig, offerSearchResponse.ReadResponseAsString());

            Assert.True(summaryLinks.Any(), $"Summary link not returned for {_testConfig.Search.FareFamilyType} and flight type {_testConfig.Search.FlightType} with request being {offerSearchGQLRequest}");

            var summaryUrl = summaryLinks.FirstOrDefault();
            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);
            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();
            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            //Asserting the counts and the acutal passenger types in the basket response against the initial search request
            Assert.True(WebBFFAssertions.ValidatePtcsCount(offerSearchGQLRequest, getBasket, out string PtcsCount), PtcsCount);
            Assert.True(WebBFFAssertions.ValidatePtcsType(offerSearchGQLRequest, getBasket, out string PtcsType), PtcsType);
        }

        [Fact]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task DOBValidation_InVALID_onePTC_VirginOnly()
        {
            // Set up initial data Origin & Destination
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.JFK;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, youngAdults: 1, children: 1, infants: 1);

            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(_testConfig);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "", _testConfig.Internal.Bearer).EnsureIsSuccessStatusCode();
            dynamic offerSearchResults = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateSearchResult(offerSearchResults, offerSearchGQLRequest, out string result), result);

            //get basket id from search graphql response body
            _testConfig.Internal.BasketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);
            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForAirlineAndFareFamily(_testConfig, offerSearchResponse.ReadResponseAsString());

            Assert.True(summaryLinks.Any(), $"Summary link not returned for {_testConfig.Search.FareFamilyType} and flight type {_testConfig.Search.FlightType} with request being {offerSearchGQLRequest}");

            var summaryUrl = summaryLinks.FirstOrDefault();
            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);
            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();
            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            HttpResponseMessage updatePassengerResponse_BFF = null;

            foreach (var item in (JArray)getBasket["people"])
            {
                _testConfig.Book.Passenger.PeopleLink = item["link"].ToString();
                _testConfig.Book.Passenger.Type = item["type"].ToString();

                // Assign incorrect DOB based on Passenger Type
                switch (_testConfig.Book.Passenger.Type)
                {
                    case "ADULT":
                        _testConfig.Book.Passenger.DateOfBirth = DateTime.Now.AddYears(-15).ToString("yyyy-MM-dd");
                        break;
                    case "TEEN":
                        _testConfig.Book.Passenger.DateOfBirth = DateTime.Now.AddYears(-10).ToString("yyyy-MM-dd");
                        break;
                    case "CHILD":
                        _testConfig.Book.Passenger.DateOfBirth = DateTime.Now.AddYears(-18).ToString("yyyy-MM-dd");
                        break;
                    case "INFANT":
                        _testConfig.Book.Passenger.DateOfBirth = DateTime.Now.AddYears(-3).ToString("yyyy-MM-dd");
                        break;
                    default:
                        Assert.Fail($"Unknown PTC type found: {_testConfig.Book.Passenger.Type}");
                        break;
                }

                var updatePassengerRequest = WebBFFHelper.UpdatePassengerRequestBff(_testConfig);
                updatePassengerResponse_BFF = WebBFFEndpoint.UpdatePassengerBFF(_testConfig.Internal.BasketId, _testConfig.Book.Passenger.PeopleLink, updatePassengerRequest).EnsureIsSuccessStatusCode();
            }
            dynamic passengerBasket = JsonConvert.DeserializeObject<dynamic>(updatePassengerResponse_BFF.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            Assert.True(((JArray)passengerBasket.people).All(x => x["validation"].ToString().Equals("INVALID", StringComparison.OrdinalIgnoreCase)), $"VALID was not returned as expected for People \n Basket Passenger Response \n {passengerBasket}");
        }
    }
}