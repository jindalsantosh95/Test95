using VAA.EAPI.Automation.Heimdall.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Helpers;
using System.Xml;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.Air4Services
{
    public class OfferOrderPri
    {
        [Trait("Category", GlobalTraits.AIR4ServicesHealthCheck)]
        [Trait("Tag", GlobalTraits.AIR4Services)]
        [Fact]
        public async Task DeltaEndToEnd()
        {
            var tokenResponse = JsonConvert.DeserializeObject<JObject>(DeltaEndpoints.GenerateToken("Offer").EnsureIsSuccessStatusCode().ReadResponseAsString());

            Assert.False(string.IsNullOrEmpty(tokenResponse["access_token"].ToString()), $"bearer token was not returned from {ConfigReader.Get("awsEndpoint")}");

            string bearer = tokenResponse["access_token"].ToString();

            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferGraphqlRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaOfferGraphqlProduction(bearer, deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offers returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];
            var responsePayload = offerSearchResponse.ReadResponseAsString();

            var sizeInBytes = responsePayload.Length;

            //Generating delta offer summary request
            var deltaOfferSummaryRequest = Core.Delta.Factory.OfferOrder.DeltaOfferGraphqlRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerSummaryResponse = Core.Delta.Endpoint.OfferOrder.DeltaOfferGraphqlProduction(bearer, deltaOfferSummaryRequest).EnsureIsSuccessStatusCode();

            var content = await offerSummaryResponse.Content.ReadAsStringAsync();
            var offerSummaryResult = JsonConvert.DeserializeObject<JObject>(content);

            // Safely navigate to selectedOfferSets
            var selectedOfferSets = offerSummaryResult["data"]?["searchOffers"]?["selectedOfferSets"] as JArray;

            Assert.NotNull(selectedOfferSets); // Ensures the property exists and is an array
            Assert.True(selectedOfferSets.Count > 0, "Offer returned was not greater than 0");

            string tokenResponseXml = DeltaEndpoints.GenerateToken("Order").EnsureIsSuccessStatusCode().ReadResponseAsString();

            XmlDocument tokenResponseXmlDocument = new XmlDocument();
            tokenResponseXmlDocument.LoadXml(tokenResponseXml);

            Assert.False(string.IsNullOrEmpty(tokenResponseXmlDocument.SelectSingleNode("/Root/access_token").InnerText), $"access token was not returned from {ConfigReader.Get("oauthProd")}");

            var token = tokenResponseXmlDocument.SelectSingleNode("/Root/access_token").InnerText;

            HttpResponseMessage generatePri = DeltaEndpoints.PriGeneration(token).EnsureIsSuccessStatusCode();

            var generatePriResponse = JsonConvert.DeserializeObject<JObject>(await generatePri.Content.ReadAsStringAsync());

            Assert.False(string.IsNullOrEmpty(generatePriResponse["paymentReferenceId"].ToString()), $"Payment reference Id was not returned from {ConfigReader.Get("deltaPriGeneration")}");

            var pri = generatePriResponse["paymentReferenceId"].ToString();

            var url = $"https://enterprisepaymentsHPP.vs.air4.com/hpp?paymentReferenceId={pri}";

            await HostedPaymentAutomation.ValidateAsync(url, expectPayPal: true);
        }
    }
}
