using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Basket
{
    public class BasketFlyingClubGraphQL
    {
        [Theory]
        [InlineData(LocationType.LHR, LocationType.ATL, FareFamilyType.ECONOMYDELIGHT, 1, 0, 0, 0, "VS", "9999998025", "Five", "Axistest")]
        [InlineData(LocationType.LHR, LocationType.ATL, FareFamilyType.ECONOMYDELIGHT, 1, 0, 0, 0, "AR", "9106599849", "Aerolineas", "Argentinas")]

        [Trait("Category", Traits.BookGraphQLRegression)]
        public async Task BasketGraphQLFlyingClubValidation(LocationType origin, LocationType destination, FareFamilyType fareFamily, int adults, int teens, int child, int infants, string fcProgram, string fcNumber, string fcFirstName, string fcLastName)
        {
            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: adults, numberOfChildren: teens, numberOfYouths: child, numberOfInfants: infants);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, fareFamily, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            Assert.True(GraphQlAssertions.SummaryUpgradeOffersValidation(offerPriceResult, out string upgradeOffers), upgradeOffers);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");

            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);

            var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic rtncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(rtncBasketGQLResult.data.updateTnC.basketId.Value == basketId, "Basket ID dont match");

            Assert.True(rtncBasketGQLResult.data.updateTnC.termsAndConditions.validation.Value == "VALID", "Basket TermsAndConditions validation is not Valid");

            //Update contact details GraphQL request
            string emailId = "hematestJan10@test.com";
            string phoneNumer = "98989656512";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumer, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Basket ID dont match");

            Assert.True(contactBasketGQLResult.data.updateContactDetails.contactDetails.validation.Value == "VALID", "Basket ContactDetails validation is not Valid");

            GraphQLRequestBuilder.UpdateFCPassengerRequestGQL(basketId, fcProgram, fcNumber, "2003-02-19", fcFirstName, fcLastName);

            var updatePassengerRequest = GraphQLRequestBuilder.UpdatePassengerRequestGQL(basketId, "0", fcFirstName, fcLastName, "2003-02-19", "male", "ADULT", fcProgram, fcNumber);

            var updatePassengerResponseGQL = GraphQLEndpoint.Book(updatePassengerRequest).EnsureGQLResponseSuccess();

            var updatePassengerResultGQL = JsonConvert.DeserializeObject<JObject>(await updatePassengerResponseGQL.Content.ReadAsStringAsync());

            JObject bob = JsonConvert.DeserializeObject<JObject>(await updatePassengerResponseGQL.Content.ReadAsStringAsync());

            var fqtValidityStatus = bob.SelectToken("$.data.updatePeople.people[0].membershipDetails.fqtValidityStatus").ToString();

            if (fcProgram == "VS")
            {
                Assert.True(fqtValidityStatus.Is("VALID"), $"VALID was not returned as expected for Basket");
            }
            else
                Assert.True(fqtValidityStatus.Is("UNCHECKED"), $"UNCHECKED was not returned as expected for Basket");
        }
    }
}