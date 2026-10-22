using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Search.Constants;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Requests.Endpoints;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch
{
    public class ReferenceMS
    {
        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [Trait("Category", Traits.ReferenceMicroserviceCiCd)]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "ReferenceMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [InlineData("countries")]
        [InlineData("airlinedetails")]
        [InlineData("aircraftdetails")]
        [InlineData("airportdetail")]
        [InlineData("cabindetails")]
        [InlineData("productidentifiers")]
        public async Task ReferenceMS_Get_Tests(string referenceCall)
        {
            // calling reference countries get end point
            HttpResponseMessage referenceMSGet = MicroserviceEndpoint.ReferenceMSGet(referenceCall).EnsureIsSuccessStatusCode();

            dynamic countriesResult = JsonConvert.DeserializeObject<dynamic>(await referenceMSGet.Content.ReadAsStringAsync());

            Assert.True(countriesResult.Count > 0, $"No {referenceCall} details returned");
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [Trait("Category", Traits.ReferenceMicroserviceCiCd)]
        [InlineData("salesbanners")]
        [InlineData("refreshsalesbanners")]
        public async Task ReferenceMS_Post_Tests(string referenceCall)
        {
            // generating reference sales banner request
            if (referenceCall == "salesbanners")
            {
                var requestPayload = OfferSearchHelper.ReferenceSalesBannersRequest("search-top");

                HttpResponseMessage salesbanners = MicroserviceEndpoint.ReferenceMSPost(requestPayload, referenceCall).EnsureIsSuccessStatusCode();

                dynamic salesbannersResult = JsonConvert.DeserializeObject<dynamic>(await salesbanners.Content.ReadAsStringAsync());

                Assert.True(((JContainer)salesbannersResult).Count > 0, "No sales banners details returned");

                string salesbannersResultAsString = await salesbanners.Content.ReadAsStringAsync();

                //Hitting new ContentMs with Sales banners
                HttpResponseMessage contenetSalesBanners = MicroserviceEndpoint.ContentMSPost(requestPayload, referenceCall).EnsureIsSuccessStatusCode();

                dynamic contentSalesBannersResult = JsonConvert.DeserializeObject<dynamic>(await salesbanners.Content.ReadAsStringAsync());

                Assert.True(((JContainer)contentSalesBannersResult).Count > 0, "No sales banners details returned");

                string contentSalesBannersResultAsString = await contenetSalesBanners.Content.ReadAsStringAsync();

                Assert.True(salesbannersResultAsString == contentSalesBannersResultAsString, "Responses were not the same");
            }
            else
            {
                // calling reference refresh sales banner post end point
                HttpResponseMessage referenceMSPost = MicroserviceEndpoint.ReferenceMSPost("", "refreshsalesbanners").EnsureIsSuccessStatusCode();

                dynamic refreshsalesbannersResult = JsonConvert.DeserializeObject<dynamic>(await referenceMSPost.Content.ReadAsStringAsync());

                string refreshsalesbannersResultAsString = await referenceMSPost.Content.ReadAsStringAsync();

                Assert.True(!string.IsNullOrEmpty(refreshsalesbannersResult), "No refresh for sales banners");

                // calling reference refresh sales banner post end point
                HttpResponseMessage contentMSPost = MicroserviceEndpoint.ContentMSPost("", "refreshsalesbanners").EnsureIsSuccessStatusCode();

                string contentRefreshSalesBannersResult = await contentMSPost.Content.ReadAsStringAsync();

                Assert.True(refreshsalesbannersResultAsString == contentRefreshSalesBannersResult, "Responses were not the same");
            }
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [Trait("Category", Traits.ReferenceMicroserviceCiCd)]
        [InlineData("salesbanners", "/far-east/india/bangalore")]
        [InlineData("salesbanners", "/far-east/india/mumbai")]
        [InlineData("salesbanners", "/far-east/india/delhi")]
        [InlineData("salesbanners", "/far-east/pakistan/islamabad")]
        [InlineData("salesbanners", "/far-east/pakistan/lahore")]
        [InlineData("salesbanners", "/united-arab-emirates/dubai")]
        [InlineData("salesbanners", "/caribbean/antigua")]
        [InlineData("salesbanners", "/caribbean/grenada")]
        [InlineData("salesbanners", "/caribbean/jamaica")]
        [InlineData("salesbanners", "/caribbean/bahamas")]
        [InlineData("salesbanners", "/caribbean/turks-and-caicos-islands")]
        [InlineData("salesbanners", "/caribbean/st-vincent-and-the-grenadines")]
        [InlineData("salesbanners", "/caribbean/st-lucia")]
        [InlineData("salesbanners", "/caribbean/barbados")]
        [InlineData("salesbanners", "/indian-ocean/maldives")]
        [InlineData("salesbanners", "/israel")]
        [InlineData("salesbanners", "/usa/texas/austin")]
        [InlineData("salesbanners", "/usa/las-vegas")]
        [InlineData("salesbanners", "/usa/new-york")]
        [InlineData("salesbanners", "/canada/toronto")]
        [InlineData("salesbanners", "/canada/montreal")]
        [InlineData("salesbanners", "/canada/vancouver")]
        [InlineData("salesbanners", "/canada/whistler")]
        [InlineData("salesbanners", "/florida/orlando")]
        [InlineData("salesbanners", "/florida/florida")]
        [InlineData("salesbanners", "/florida/miami")]
        [InlineData("salesbanners", "/florida/gulf-coast-beaches")]
        [InlineData("salesbanners", "/florida/gold-coast-beaches")]
        [InlineData("salesbanners", "/usa/california/los-angeles")]
        [InlineData("salesbanners", "/usa/california/newport-beach")]
        [InlineData("salesbanners", "/usa/california/san-francisco")]
        [InlineData("salesbanners", "/usa/california/mammoth")]
        [InlineData("salesbanners", "/usa/boston")]
        [InlineData("salesbanners", "/usa/chicago")]
        [InlineData("salesbanners", "/usa/deep-south/louisiana")]
        [InlineData("salesbanners", "/usa/deep-south/georgia")]
        [InlineData("salesbanners", "/usa/washington-dc")]
        [InlineData("salesbanners", "/usa/texas/dallas")]
        [InlineData("salesbanners", "/usa/texas/houston")]
        [InlineData("salesbanners", "/usa/arizona")]
        [InlineData("salesbanners", "/usa/utah")]
        [InlineData("salesbanners", "/africa/south-africa/johannesburg")]
        [InlineData("salesbanners", "/africa/south-africa/cape-town")]
        [InlineData("salesbanners", "/caribbean/mexico")]
        [InlineData("salesbanners", "/indian-ocean/mauritius")]
        [InlineData("refreshsalesbanners", "")]
        public async Task ContentMS_ReferenceMS_ComparisonTests(string referenceCall, string destinationUrl)
        {
            // generating sales banner request
            if (referenceCall == "salesbanners")
            {
                var salesBannersRequest = OfferSearchHelper.ReferenceSalesBannersRequest("seasonal-messaging", "LHR", "None", "", flightDirection: "return", departureDate: "2026-03-23", returnDate: "2026-03-30", 1, 0, 0, 0, destinationUrl);

                //Hitting existing ReferenceMs with Sales Banners
                HttpResponseMessage referenceSalesBanners = MicroserviceEndpoint.ReferenceMSPost(salesBannersRequest, referenceCall).EnsureIsSuccessStatusCode();

                string referenceSalesBannersResult = await referenceSalesBanners.Content.ReadAsStringAsync();

                //Hitting new ContentMs with Sales banners
                HttpResponseMessage contenetSalesBanners = MicroserviceEndpoint.ContentMSPost(salesBannersRequest, referenceCall).EnsureIsSuccessStatusCode();

                string contentSalesBannersResult = await contenetSalesBanners.Content.ReadAsStringAsync();

                Assert.True(contentSalesBannersResult == referenceSalesBannersResult, "Responses were not the same");
            }
            else
            {
                // calling reference refresh sales banner post end point
                HttpResponseMessage contentMSPost = MicroserviceEndpoint.ContentMSPost("", "refreshsalesbanners").EnsureIsSuccessStatusCode();

                string contentRefreshSalesBannersResult = await contentMSPost.Content.ReadAsStringAsync();

                // calling reference refresh sales banner post end point
                HttpResponseMessage referenceMSPost = MicroserviceEndpoint.ReferenceMSPost("", "refreshsalesbanners").EnsureIsSuccessStatusCode();

                string referenceRefreshsalesbannersResult = await referenceMSPost.Content.ReadAsStringAsync();

                Assert.True(contentRefreshSalesBannersResult == referenceRefreshsalesbannersResult, "Responses were not the same");
            }
        }
    }
}
