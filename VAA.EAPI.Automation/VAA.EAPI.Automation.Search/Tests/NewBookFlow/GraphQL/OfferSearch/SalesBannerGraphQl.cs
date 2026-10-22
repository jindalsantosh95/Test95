using System.Diagnostics;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Constants;
using GraphQLEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.GraphQLEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch
{
    public class SalesBannerGraphQl
    {
        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.MLE)]
        [InlineData(LocationType.LHR, LocationType.JFK)]
        [InlineData(LocationType.LHR, LocationType.YYZ)]
        public void SalesBannerGraphQlTest(LocationType origin, LocationType destination)
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating sales banner graph ql request.
            string salesBannerGqlRequest = OfferSearchHelper.SalesBannerGQLRequest(origin: origin, destination: destination, departureDate: departureDate, returnDate: returnDate, numberOfadults: 1, flightDirection: "one-way");

            //post sales banner GRAPHQL endpoint.
            HttpResponseMessage salesBannerResponse = GraphQLEndpoint.OfferSearch(salesBannerGqlRequest, "").EnsureGQLResponseSuccess();

            dynamic salesBannerResult = JsonConvert.DeserializeObject<dynamic>(salesBannerResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(salesBannerResult.data.salesBanners.Count > 0, "No content is returned for sales banner");
        }

        //These tests has been added for local run only.
        [Theory]
        [Trait("Category", GlobalTraits.LocalRun)]
        [InlineData("/far-east/india/bangalore")]
        [InlineData("/far-east/india/mumbai")]
        [InlineData("/far-east/india/delhi")]
        [InlineData("/far-east/pakistan/islamabad")]
        [InlineData("/far-east/pakistan/lahore")]
        [InlineData("/united-arab-emirates/dubai")]
        [InlineData("/caribbean/antigua")]
        [InlineData("/caribbean/grenada")]
        [InlineData("/caribbean/jamaica")]
        [InlineData("/caribbean/bahamas")]
        [InlineData("/caribbean/turks-and-caicos-islands")]
        [InlineData("/caribbean/st-vincent-and-the-grenadines")]
        [InlineData("/caribbean/st-lucia")]
        [InlineData("/caribbean/barbados")]
        [InlineData("/indian-ocean/maldives")]
        [InlineData("/israel")]
        [InlineData("/usa/texas/austin")]
        [InlineData("/usa/las-vegas")]
        [InlineData("/usa/new-york")]
        [InlineData("/canada/toronto")]
        [InlineData("/canada/montreal")]
        [InlineData("/canada/vancouver")]
        [InlineData("/canada/whistler")]
        [InlineData("/florida/orlando")]
        [InlineData("/florida/florida")]
        [InlineData("/florida/miami")]
        [InlineData("/florida/gulf-coast-beaches")]
        [InlineData("/florida/gold-coast-beaches")]
        [InlineData("/usa/california/los-angeles")]
        [InlineData("/usa/california/newport-beach")]
        [InlineData("/usa/california/san-francisco")]
        [InlineData("/usa/california/mammoth")]
        [InlineData("/usa/boston")]
        [InlineData("/usa/chicago")]
        [InlineData("/usa/deep-south/louisiana")]
        [InlineData("/usa/deep-south/georgia")]
        [InlineData("/usa/washington-dc")]
        [InlineData("/usa/texas/dallas")]
        [InlineData("/usa/texas/houston")]
        [InlineData("/usa/arizona")]
        [InlineData("/usa/utah")]
        [InlineData("/africa/south-africa/johannesburg")]
        [InlineData("/africa/south-africa/cape-town")]
        [InlineData("/caribbean/mexico")]
        [InlineData("/indian-ocean/mauritius")]
        public void SalesBannerGraphQlDesinationUrlTest(string destinationUrl)
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating sales banner graph ql request.
            string salesBannerGqlRequest = OfferSearchHelper.SalesBannerGQLRequest(origin: LocationType.LHR, departureDate: departureDate, returnDate: returnDate, numberOfadults: 1, destinationUrl: destinationUrl, flightDirection: "return");

            Debug.WriteLine(salesBannerGqlRequest.ToString());

            //post sales banner GRAPHQL endpoint.
            HttpResponseMessage salesBannerResponse = GraphQLEndpoint.OfferSearch(salesBannerGqlRequest, "").EnsureGQLResponseSuccess();

            dynamic salesBannerResult = JsonConvert.DeserializeObject<dynamic>(salesBannerResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string response = salesBannerResult.ToString();

            Debug.WriteLine(response);

            Assert.True(salesBannerResult.data.salesBanners.Count > 0, "No content is returned for sales banner");
        }
    }
}

