using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Basket
{
    public class BasketGraphQLEmailValidation
    {
        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, "fly.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "flycom")]
        [InlineData(LocationType.LHR, LocationType.JFK, "fly&com.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "fly_com.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "#@%^%#$@#$@#.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "abc.def@mail#archive.com")]
        public async Task TC01_BasketGraphQL_Invalid_EmailAddressNoAtSymbolAndGeneral(LocationType origin, LocationType destination, string inValidEmailID)
        {
            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, FareFamilyType.ECONOMYDELIGHT, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //update contact details GraphQL request
            string emailId = inValidEmailID;
            string phoneNumber = "12345678912";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Basket ID dont match");

            string email = contactBasketGQLResult.data.updateContactDetails.contactDetails.emailAddress;

            Assert.True(emailId == email, $"Email IDr valdation failed: \n  Response EmailID = {email} \n   Request EmailID = {emailId} ");

            Assert.True(contactBasketGQLResult.data.updateContactDetails.contactDetails.validation == "INVALID", $"Validation value in basketResponse is not INVALID \n----------\n  {contactBasketGQLResult.data.updateContactDetails.contactDetails} \n----------\n");
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, "")]
        [InlineData(LocationType.LHR, LocationType.JFK, null)]
        public async Task TC02_BasketGraphQL_Invalid_EmailAddressEmpty(LocationType origin, LocationType destination, string inValidEmailID)
        {
            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, FareFamilyType.ECONOMYDELIGHT, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //update contact details GraphQL request
            string emailId = inValidEmailID;
            string phoneNumber = "12345678912";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Basket ID dont match");

            string email = contactBasketGQLResult.data.updateContactDetails.contactDetails.emailAddress;

            Assert.True(emailId == email, $"Email IDr valdation failed: \n Response EmailID = {email} \n Request EmailID = {emailId} ");

            Assert.True(contactBasketGQLResult.data.updateContactDetails.contactDetails.validation == "INVALID", $"Validation value in basketResponse is not INVALID \n----------\n   {contactBasketGQLResult.data.updateContactDetails.contactDetails}\n----------\n  ");
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [Trait("Category", Traits.NbfGraphQLCICD)]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@@santy@@santy.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@@santy@@santy.")]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@@santy@@")]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@@santy")]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@")]
        public async Task TC03_BasketGraphQL_Invalid_EmailAddressDoubleAtSymbol(LocationType origin, LocationType destination, string inValidEmailID)
        {
            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, FareFamilyType.ECONOMYDELIGHT, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //update contact details GraphQL request
            string emailId = inValidEmailID;
            string phoneNumber = "12345678912";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Basket ID dont match");

            string email = contactBasketGQLResult.data.updateContactDetails.contactDetails.emailAddress;

            Assert.True(emailId == email, $"Email IDr valdation failed: \n Response EmailID = {email} \n Request EmailID = {emailId} ");

            Assert.True(contactBasketGQLResult.data.updateContactDetails.contactDetails.validation == "INVALID", $"Validation value in basketResponse is not INVALID \n----------\n   {contactBasketGQLResult.data.updateContactDetails.contactDetails}\n----------\n  ");
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test.")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test@test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test_")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@_Test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@-Test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester @ Test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@123")]
        [InlineData(LocationType.LHR, LocationType.JFK, "123@123")]
        [InlineData(LocationType.LHR, LocationType.JFK, "$$$$@123")]
        [InlineData(LocationType.LHR, LocationType.JFK, "$$$$@$$$")]
        public async Task TC04_BasketGraphQL_Invalid_EmailAddressNoDotCom(LocationType origin, LocationType destination, string inValidEmailID)
        {
            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, FareFamilyType.ECONOMYDELIGHT, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //update contact details GraphQL request
            string emailId = inValidEmailID;
            string phoneNumber = "12345678912";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Basket ID dont match");

            string email = contactBasketGQLResult.data.updateContactDetails.contactDetails.emailAddress;

            Assert.True(emailId == email, $"Email IDr valdation failed: \n Response EmailID = {email} \n Request EmailID = {emailId} ");

            Assert.True(contactBasketGQLResult.data.updateContactDetails.contactDetails.validation == "INVALID", $"Validation value in basketResponse is not INVALID \n----------\n   {contactBasketGQLResult.data.updateContactDetails.contactDetails}\n----------\n  ");
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [Trait("Category", Traits.NbfGraphQLCICD)]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test.test.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester123@Test.test.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester123@123.123.123.123")]
        [InlineData(LocationType.LHR, LocationType.JFK, "1234567890@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "firstname.lastname@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "firstname+lastname@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "_______@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "firstname-lastname@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "email@domain.co.jp")]
        [InlineData(LocationType.LHR, LocationType.JFK, "123test@test.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "!fly@com.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "email@domain.museum")]
        [InlineData(LocationType.LHR, LocationType.JFK, "user.name+tag+sorting@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "abc#def@mail.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "1234567890123456789012345678901234567890123456789012345678901234+x@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "domain-indeed@strange-domain.inininini")]
        [InlineData(LocationType.LHR, LocationType.JFK, "TesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTes@Test.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@TesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTeste.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "TesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTes@TesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTeste.com")]
        public async Task TC05_BasketGraphQL_Valid_EmailAddress(LocationType origin, LocationType destination, string validEmailID)
        {
            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, FareFamilyType.ECONOMYDELIGHT, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //update contact details GraphQL request
            string emailId = validEmailID;
            string phoneNumber = "12345678912";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Basket ID dont match");

            string email = contactBasketGQLResult.data.updateContactDetails.contactDetails.emailAddress;

            Assert.True(emailId == email, $"Email IDr valdation failed: \n Response EmailID = {email} \n Request EmailID = {emailId} ");

            Assert.True(contactBasketGQLResult.data.updateContactDetails.contactDetails.validation == "VALID", $"Validation value in basketResponse is not VALID \n----------\n   {contactBasketGQLResult.data.updateContactDetails.contactDetails}\n----------\n  ");
        }
    }
}