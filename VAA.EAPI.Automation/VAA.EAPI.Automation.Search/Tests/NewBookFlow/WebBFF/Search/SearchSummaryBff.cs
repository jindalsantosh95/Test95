using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Search.Constants;
using VAA.EAPI.Automation.Search.Requests.Endpoints;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;
using WebBFFEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.WebBFFEndpoint;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.WebBFF.Search
{
    public class SearchSummaryBff
    {
        [Theory]
        [Trait("Category", Traits.SearchBffSummaryCiCd)]
        [InlineData(LocationType.LHR, LocationType.LOS)]
        [InlineData(LocationType.MAN, LocationType.ATL)]
        [InlineData(LocationType.JFK, LocationType.LHR)]
        [InlineData(LocationType.BOS, LocationType.LHR)]
        public void SearchSummaryBFFTests(LocationType origin, LocationType destination)
        {
            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, FareFamilyType.ECON, FlightType.VS);

            Assert.True(summaryLinks.Any(), $"Offer not returned for the fare family->{FareFamilyType.ECON} and flight type passed->VS");

            var summaryUrl = summaryLinks.FirstOrDefault();

            //Getting basketId from the response url
            WebBFFHelper.SummaryURL(summaryUrl, out string basketId, out string slice, out string offerId);

            //Need to build url for Summary link from returned url
            summaryUrl = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            var summaryResult = JsonConvert.DeserializeObject<JToken>(summaryBffResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var summaryFlightsResult = summaryResult.SelectTokens("$..flights").Select(x => x.ToString()).ToArray();

            Assert.True(summaryFlightsResult.Any(), $"Summary bff flights result not exists : {summaryResult}");
        }

        [Fact]
        [Trait("Category", Traits.SearchBffSummaryCiCd)]
        public async Task RetrieveFCMemberDetails()
        {
            Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable("Red", 2000);

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            HttpResponseMessage fcMemberDetailsResponse = WebBFFEndpoint.GetMemberDetailsBFF(b2cToken).EnsureIsSuccessStatusCode();

            dynamic fcMemberResult = JsonConvert.DeserializeObject<dynamic>(await fcMemberDetailsResponse.Content.ReadAsStringAsync());

            Assert.True(WebBFFAssertions.ValidateFCMemberDetails(fcMembershipNumber, "Red", fcMemberResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }
    }
}