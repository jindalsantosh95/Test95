using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Constants;
using Chooose = VAA.EAPI.Automation.Search.Requests.Factory.Chooose;
using ChooseResponse = VAA.EAPI.Automation.Search.Models.SustainabilityMS.ChooseResponse;
using MicroserviceEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.MicroserviceEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;
using SustainabiltyResponse = VAA.EAPI.Automation.Search.Models.SustainabilityMS.SustainabiltyResponse;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch
{
    public class Sustainability : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public Sustainability(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForChooose("choose");
        }

        [Theory]
        [Trait("Category", Traits.SustainabilityMicroserviceCiCd)]
        [Trait("Category", GlobalTraits.LocalRun)]
        [InlineData(LocationType.LHR, LocationType.JFK, CabinType.Economy, 9)]
        [InlineData(LocationType.LHR, LocationType.JFK, CabinType.Business, 5)]
        [InlineData(LocationType.LHR, LocationType.JFK, CabinType.Premium, 1)]
        public void Sustainability_TC01_ConnectivityTest(LocationType origin, LocationType destination, CabinType cabinType, int passengersCount)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating chooose request
            var chooseRequest = Chooose.ChooseRequest(origin: origin, destination: destination, "", deptartureDate, cabinType.ToString());

            //post chooose API
            HttpResponseMessage chooseResponse = Chooose.CreateChooseRequest(tokenFixture.GetToken("choose"), chooseRequest, passengersCount).EnsureIsSuccessStatusCode();

            ChooseResponse[] chooseResult = JsonConvert.DeserializeObject<ChooseResponse[]>(chooseResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Generating sustainability request
            var sustainabilityRequest = MicroserviceRequestBuilder.SustainabilityMSRequest(origin: origin, destination: destination, "", deptartureDate, cabinType.ToString(), passengersCount);

            //post sustainability API
            HttpResponseMessage sustainabilityResponse = MicroserviceEndpoint.Sustainability(sustainabilityRequest).EnsureIsSuccessStatusCode();

            SustainabiltyResponse[] sustainabilityResult = JsonConvert.DeserializeObject<SustainabiltyResponse[]>(sustainabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(OfferSearchAssertions.ValidateSustainabilityResponse(chooseResult, sustainabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }
    }
}