using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.WebBFF;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;
using WebBFFEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.WebBFFEndpoint;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Production.WebBFF
{
    public class SearchBFF
    {
        public static string env = Config.Environment;

        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        public void SearchBFF_OneWay()
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: LocationType.LHR, destination: LocationType.JFK, adults: 1, teens: 0, child: 0, infants: 0);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            //getting dynamic cabin link from offer response
            OfferSearchHelper.GetOfferIdAndPassengersCountsFromDynamicCabinLink(offerSearchBFFResponse, out string id, out string passengersCounts, out string dynamicCabinLink);

            //get call for dynamic cabin link
            var dynamicCabinResponse = WebBFFEndpoint.GetLink(dynamicCabinLink).EnsureSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            var url = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForOneWay(result, FlightScenarioType.Virgin);

            //Getting basketId from the response url
            WebBFFHelper.ProductionSummaryUrl(url, out string basketId, out string slice, out string offerId);

            //Need to build url for Summary link from returned url
            url = Config.TestResourcesVirginAtlantic.GetUrl($"/search/summary/{basketId}/{slice}|{offerId}");

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            OfferSearchHelper.GetCarbonIndexLinkFromResponse(summaryBffResponse, out string carbonIndexLink);

            // get call for carbon index link
            var carbonIndexResponse = WebBFFEndpoint.GetLink(carbonIndexLink).EnsureSuccessStatusCode();

            var outboundResponse = offerSearchBFFResponse.ReadResponseAsString();

            var sortLink = WebBFFHelper.GetSortLinkBySortName(outboundResponse, "PRICE");

            // get call for sort link
            var sortResponse = WebBFFEndpoint.GetLink(sortLink).EnsureSuccessStatusCode();

            var filterLink = WebBFFHelper.GetFilterLinkByFilterName(outboundResponse, "Virgin Atlantic");

            // get call for filter link
            var filterResponse = WebBFFEndpoint.GetLink(filterLink).EnsureSuccessStatusCode();
        }

        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        public async Task SearchBFF_Return()
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.Return, origin: LocationType.LHR, destination: LocationType.JFK, returnOrigin: LocationType.JFK, returnDestination: LocationType.LHR, adults: 1, teens: 0, child: 0, infants: 0, rewardFlag: false);

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
            var offerSearchBFFReturnRequest = OfferSearchHelper.OfferSearchBFFRequestReturn(JourneyType.Return, origin: LocationType.LHR, destination: LocationType.JFK, returnOrigin: LocationType.JFK, returnDestination: LocationType.LHR, adults: 1, teens: 0, child: 0, infants: 0, id: id, fareId: fareId, rewardFlag: false);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFReturnResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFReturnRequest).EnsureIsSuccessStatusCode();

            var inboundResult = offerSearchBFFReturnResponse.ReadResponseAsString();

            //Getting the link url for the SummaryBff
            var summaryUrl = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForReturn(inboundResult, FlightScenarioType.Virgin);

            //Post Offer URL
            HttpResponseMessage summaryUrlResponse = WebBFFEndpoint.PostOffer(summaryUrl, "");

            //Getting basketId,slice and offerId from the summary url
            WebBFFHelper.ProductionSummaryUrl(summaryUrl, out string basketId, out string slice, out string offerId);

            //Need to build url for Summary link from returned url
            summaryUrl = Config.TestResourcesVirginAtlantic.GetUrl($"/search/summary/{basketId}/{slice}|{offerId}");

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            var summaryUpgradeUrl = OfferSearchHelper.ReturnSummaryUpgradeLinks(summaryBffResponse)[0];

            //Calling summary upgrade url
            HttpResponseMessage summaryUpgradeResponse = WebBFFEndpoint.PostOffer(summaryUpgradeUrl, "");
        }
    }
}