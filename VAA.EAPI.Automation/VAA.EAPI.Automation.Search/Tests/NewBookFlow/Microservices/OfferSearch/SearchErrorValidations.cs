using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Constants;
using MicroserviceEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.MicroserviceEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;
using SearchErrorResponse = VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse.SearchErrorResponse;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch
{
    public class SearchErrorValidations
    {
        public static string env = Config.Environment;
        private const string errorCode = "G052";
        private const string errorSource = "Delta services";

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OfferMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [Trait("SharedServiceLayer", "DirectMS")]
        public void SharedServicesSignOff_OfferSearch_InvalidOriginContract() =>
            OfferSearch_TC04_WithoutOriginErrorValidation();

        [Fact(Skip = "Skipping as this test not in scope for now")]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferSearch_TC01_PtcErrorValidation()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.ANU, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 3);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsUnprocessableEntityStatusCode();

            SearchErrorResponse[] searchErrorResponse = JsonConvert.DeserializeObject<SearchErrorResponse[]>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(OfferSearchAssertions.ValidateOfferSearchMSErrorResponse(searchErrorResponse, errorCode, errorSource, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Fact(Skip = "Skipping as this test not in scope for now")]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferSearch_TC02_WithoutPtcErrorValidation()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 0, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsUnprocessableEntityStatusCode();

            SearchErrorResponse[] searchErrorResponse = JsonConvert.DeserializeObject<SearchErrorResponse[]>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(OfferSearchAssertions.ValidateOfferSearchMSErrorResponse(searchErrorResponse, errorCode, errorSource, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Fact(Skip = "Skipping as this test not in scope for now")]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferSearch_TC03_WithoutDateErrorValidation()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, "", locationOne: LocationType.LHR, locationTwo: LocationType.ANU, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsUnprocessableEntityStatusCode();

            SearchErrorResponse[] searchErrorResponse = JsonConvert.DeserializeObject<SearchErrorResponse[]>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(OfferSearchAssertions.ValidateOfferSearchMSErrorResponse(searchErrorResponse, errorCode, errorSource, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferSearch_TC04_WithoutOriginErrorValidation()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, "", locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsUnprocessableEntityStatusCode();

            SearchErrorResponse[] searchErrorResponse = JsonConvert.DeserializeObject<SearchErrorResponse[]>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(OfferSearchAssertions.ValidateOfferSearchMSErrorResponse(searchErrorResponse, errorCode, errorSource, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferSearch_TC05_PastDateErrorValidation()
        {
            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, "2023-01-01", locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsUnprocessableEntityStatusCode();

            SearchErrorResponse[] searchErrorResponse = JsonConvert.DeserializeObject<SearchErrorResponse[]>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(OfferSearchAssertions.ValidateOfferSearchMSErrorResponse(searchErrorResponse, errorCode, errorSource, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }
    }
}
