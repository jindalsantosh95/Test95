using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    public class BasketSummaryValidation
    {
        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 0, 0, 0, "GBP")]
        [InlineData(LocationType.MAN, LocationType.ATL, 1, 1, 1, 0, "GBP")]
        [InlineData(LocationType.LHR, LocationType.LOS, 1, 2, 1, 1, "GBP")]
        [InlineData(LocationType.LHR, LocationType.LAS, 2, 3, 1, 0, "GBP")]
        [InlineData(LocationType.MAN, LocationType.SEA, 1, 1, 1, 1, "GBP")]
        [InlineData(LocationType.JFK, LocationType.LHR, 1, 0, 1, 1, "USD")]
        [InlineData(LocationType.LAS, LocationType.LHR, 1, 1, 0, 2, "USD")]
        [InlineData(LocationType.SEA, LocationType.LHR, 4, 0, 0, 0, "USD")]
        [InlineData(LocationType.JNB, LocationType.LHR, 1, 0, 0, 0, "ZAR")]
        [InlineData(LocationType.CPT, LocationType.LHR, 1, 0, 0, 0, "ZAR")]
        [InlineData(LocationType.JNB, LocationType.JFK, 1, 0, 0, 0, "ZAR")]
        [InlineData(LocationType.CPT, LocationType.JFK, 1, 0, 0, 0, "ZAR")]
        [InlineData(LocationType.BGI, LocationType.LHR, 1, 0, 0, 0, "BBD")]
        [InlineData(LocationType.BGI, LocationType.MAN, 1, 0, 0, 0, "BBD")]
        [InlineData(LocationType.ANU, LocationType.LHR, 1, 0, 0, 0, "JMD")]
        [InlineData(LocationType.SVD, LocationType.LHR, 1, 0, 0, 0, "XCD")]
        [InlineData(LocationType.GND, LocationType.LHR, 1, 0, 0, 0, "XCD")]
        [InlineData(LocationType.UVF, LocationType.LHR, 1, 0, 0, 0, "XCD")]
        [InlineData(LocationType.PLS, LocationType.LHR, 1, 0, 0, 0, "XCD")]
        [InlineData(LocationType.DXB, LocationType.LHR, 1, 0, 0, 0, "AED")]
        [InlineData(LocationType.PVG, LocationType.LHR, 1, 0, 0, 0, "CNY")]
        public async Task BasketSummaryCurrencyValidation(LocationType origin, LocationType destination, int adults, int teens, int child, int infants, string expectedCurrency)
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: adults, teens: teens, child: child, infants: infants);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            var url = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForOneWay(result, FlightScenarioType.Virgin);

            //Getting basketId from the response url
            WebBFFHelper.SummaryURL(url, out string basketId, out string slice, out string offerId);

            //Need to build url for Summary link from returned url
            url = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            dynamic summaryBFFPayload = summaryBffResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            //Asserting the Basket Summary Currency
            Assert.True(BasketBFFAssertion.ValidateBasketSummaryCurrency(summaryBFFPayload, getBasket, expectedCurrency, out string currency), currency);
        }


        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 0, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, 6, 1, 2, 0)]
        [InlineData(LocationType.JFK, LocationType.LHR, 1, 0, 1, 1)]
        public async Task BasketSummaryPaxCountValidation(LocationType origin, LocationType destination, int adults, int teens, int child, int infants)
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: adults, teens: teens, child: child, infants: infants);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            var url = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForOneWay(result, FlightScenarioType.Virgin);

            //Getting basketId from the response url
            WebBFFHelper.SummaryURL(url, out string basketId, out string slice, out string offerId);

            //Need to build url for Summary link from returned url
            url = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            dynamic summaryBFFPayload = summaryBffResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            //Asserting the Basket Summary Passenger count
            Assert.True(BasketBFFAssertion.ValidateBasketSummaryPassengerCount(getBasket, adults, teens, child, infants, out string passengers), passengers);
        }


        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 0, 0, 0)]
        [InlineData(LocationType.JFK, LocationType.LHR, 1, 0, 1, 1)]
        public async Task BasketSummaryPricePerPassengerValidation(LocationType origin, LocationType destination, int adults, int teens, int child, int infants)
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: adults, teens: teens, child: child, infants: infants);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            var url = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForOneWay(result, FlightScenarioType.Virgin);

            //Getting basketId from the response url
            WebBFFHelper.SummaryURL(url, out string basketId, out string slice, out string offerId);

            //Need to build url for Summary link from returned url
            url = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            dynamic summaryBFFPayload = summaryBffResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            //Per Price Person validation
            Assert.True(BasketBFFAssertion.ValidateBasketSummaryPassengerPrice(getBasket, adults, teens, child, infants, out string passengers), passengers);
        }


        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 0, 0, 0)]
        [InlineData(LocationType.JFK, LocationType.LHR, 1, 0, 1, 1)]
        public async Task BasketSummaryTotalPriceValidation(LocationType origin, LocationType destination, int adults, int teens, int child, int infants)
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.Return, origin: origin, destination: destination, returnOrigin: destination, returnDestination: origin, adults: adults, teens: teens, child: child, infants: infants);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.ReadResponseAsString();

            var url = OfferSearchHelper.FindOfferForReturn(result, FlightScenarioType.Virgin);

            //Post Offer URL
            HttpResponseMessage postUrlResponse = WebBFFEndpoint.PostOffer(url, "");

            dynamic postOfferResponse = JsonConvert.DeserializeObject<dynamic>(await postUrlResponse.Content.ReadAsStringAsync());

            //Getting the Id and FareId from the response of Select and Next to build up URL for Summary
            string id = postOfferResponse.slice.id.Value.ToString();
            string fareId = postOfferResponse.slice.fareId.Value.ToString();

            //Need to call search and this time pass in the id and fareId details into the body
            var offerSearchBFFReturnRequest = OfferSearchHelper.OfferSearchBFFRequestReturn(JourneyType.Return, origin: LocationType.LHR, destination: LocationType.JFK, returnOrigin: LocationType.JFK, returnDestination: LocationType.LHR, adults: adults, teens: teens, child: child, infants: infants, id: id, fareId: fareId);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFReturnResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFReturnRequest).EnsureIsSuccessStatusCode();

            var inboundResult = offerSearchBFFReturnResponse.ReadResponseAsString();

            //Getting the link url for the SummaryBff
            var summaryUrl = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForReturn(inboundResult, FlightScenarioType.Virgin);

            //Details for URL
            WebBFFHelper.SummaryURL(summaryUrl, out string basketId, out string slice, out string offerId);

            //Manipulate URL to follow front end behaviour
            //Need to build url for Summary link from returned url
            summaryUrl = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            dynamic summaryBFFPayload = summaryBffResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            //Asserting the Basket Summary Total price
            Assert.True(BasketBFFAssertion.ValidateBasketSummaryTotalPrice(summaryBFFPayload, getBasket, out string totalPrice), totalPrice);
        }
    }
}