using Castle.Core.Logging;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.Amenity;
using VAA.EAPI.Web.Search.Models.CarbonIndex;
using VAA.EAPI.Web.Search.Models.Search.Request;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;
using VAA.EAPI.Web.Search.Trigger.Http;
using Xunit;
using static VAA.EAPI.Web.UnitTests.Framework.TestHelpers;

namespace VAA.EAPI.Web.Search.Tests
{
    public class SearchControllerTests
    {
        private readonly Mock<IMediator> mediator = new();
        private readonly Mock<ILogger<SearchController>> logger = new();
        private readonly string basketId = "60c72626-a52b-4041-906e-7085d020b17c";

        public SearchControllerTests()
        {
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            Environment.SetEnvironmentVariable("PostbackUrl", "https://dummy.com/");

        }

        [Fact]
        public void SearchController_ctor_NullMediator_ShouldThrowException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new SearchController(null, null));
        }

        [Fact]
        public void SearchController_ctor_NullMediator_ShouldSucceed()
        {
            // Act
            var controller = new SearchController(mediator.Object, logger.Object);

            // Assert
            Assert.IsType<SearchController>(controller);
        }

        [Theory]
        [InlineData("Oneway/Search-RQ", "Oneway/Search-RS", false, true)]
        [InlineData("Return/Search-OB-RQ", "Return/Search-OB-RS", true, true)]
        [InlineData("Return/Search-IB-RQ", "Return/Search-IB-RS", true, false)]
        public async Task SearchController_Search_ShouldSucceed(string rqFile, string rsFile, bool expectPriceGrid, bool is1stLeg)
        {
            /* Arrange */
            var mockReqBody = SetupHttpRequest($"Search/SearchController/{rqFile}");    // Request Payload

            if (is1stLeg)
            {
                var mockDataResponse = TestDataProvider.GetTestData<SearchResultsResponse>($"Search/SearchController/{rsFile}");
                mediator.Setup(m => m.Send(It.IsAny<SearchRequestV2>(), CancellationToken.None)).ReturnsAsync(mockDataResponse);
            }
            else
            {
                var mockDataResponse = TestDataProvider.GetTestData<Result>($"Search/SearchController/{rsFile}");
                mediator.Setup(m => m.Send(It.IsAny<SelectAndNextRequest>(), CancellationToken.None)).ReturnsAsync(mockDataResponse);
            }
            SearchController ctrl = new(mediator.Object, logger.Object);


            /* Act */
            var result = await ctrl.Search(mockReqBody.Object);

            /* Assert */
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            if (is1stLeg)
            {
                var output = Json.Deserialize<SearchResultsResponse>(body);
                if (expectPriceGrid)
                    Assert.NotNull(output.PriceGrid);
                else
                    Assert.Null(output.PriceGrid);
                Assert.NotNull(output.Calendar);
                Assert.NotNull(output.Result);
                Assert.NotNull(output.Result.Slice);
            }
            else
            {
                var output = Json.Deserialize<Result>(body);
                Assert.NotNull(output.Slice);
            }
        }


        [Fact]
        public async Task SearchController_SelectNext_ShouldSucceed()
        {
            /* Arrange */
            var prevResponse = TestDataProvider.GetTestData<SearchResultsResponse>("Search/SearchController/Return/Search-OB-RS");
            var offerId = prevResponse.Result.Slice.FlightsAndFares.First().Fares.First(f => !f.Id.IsNullOrEmpty()).Id;
            var sliceId = $"{prevResponse.Result.Slices.total}.{prevResponse.Result.Slices.current}";
            var mockReqBody = SetupHttpRequest();

            SearchController ctrl = new(mediator.Object, logger.Object);

            /* Act */
            var result = await ctrl.SelectAndNext(mockReqBody.Object, basketId, offerId, sliceId);

            /* Assert */
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var output = Json.Deserialize<Result>(body);
            Assert.NotNull(output.Slice);
            Assert.Equal(basketId, output.Slice.Id);
            Assert.Equal(string.Join('|', sliceId, offerId), output.Slice.FareId);
        }

        [Theory]
        [InlineData("Oneway/Search-RS", "Oneway/Summary-RS", true)]
        [InlineData("Return/Search-IB-RS", "Return/Summary-RS", false)]
        public async Task SearchController_Search_SelectAndSummary_ShouldSucceed(string prevResponseFile, string rsFile, bool isReturn)
        {
            /* Arrange */
            Result prevResponse;
            if (isReturn)
            {
                prevResponse = TestDataProvider.GetTestData<SearchResultsResponse>($"Search/SearchController/{prevResponseFile}").Result;
            }
            else
            {
                prevResponse = TestDataProvider.GetTestData<Result>($"Search/SearchController/{prevResponseFile}");
            }
            var offerId = prevResponse.Slice.FlightsAndFares.First().Fares.First(f => !f.Id.IsNullOrEmpty()).Id;
            var sliceId = $"{prevResponse.Slices.total}.{prevResponse.Slices.current}";
            var mockReqBody = SetupHttpRequest();

            SearchController ctrl = new(mediator.Object, logger.Object);

            /* Act */
            var result = await ctrl.SelectAndSummary(mockReqBody.Object, basketId, offerId, sliceId);

            /* Assert */
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var output = Json.Deserialize<Models.Search.Response.TravelPlusSummary.Summary>(body);
            Assert.Equal(basketId, output.Id);
            Assert.Equal(string.Join('|', sliceId, offerId), output.FareId);
            Assert.Equal(4, output.Links.Count());

        }

        [Theory]
        [InlineData("Return/Search-IB-RS", "DynamicCabin-RS", "a1t0c0i0")]
        public async Task SearchController_DynamicCabin_ShouldSucceed(string prevResponseFile, string rsFile, string passengerCounts)
        {
            /* Arrange */
            var prevResponse = TestDataProvider.GetTestData<Result>($"Search/SearchController/{prevResponseFile}");
            var offerId = prevResponse.Slice.FlightsAndFares.First().Fares.First(f => !f.Id.IsNullOrEmpty()).Id;
            var mockReqBody = SetupHttpRequest();

            CabinAmenity mockDataResponse = TestDataProvider.GetTestData<CabinAmenity>($"Search/SearchController/{rsFile}");
            mediator.Setup(m => m.Send(It.IsAny<DynamicCabinRequest>(), CancellationToken.None)).ReturnsAsync(mockDataResponse);
            SearchController ctrl = new(mediator.Object, logger.Object);


            // Response Payload
            var ctrlExpectedResponse = CSharpFunctionalExtensions.Result.Success(new Response<CabinAmenity>()
            {
                Status = Status.OK,
                Value = mockDataResponse
            });

            /* Act */
            var result = await ctrl.DynamicCabinV2(mockReqBody.Object, passengerCounts, offerId);

            /* Assert */
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var output = Json.Deserialize<CabinAmenity>(body);
            Assert.NotNull(output.CabinDetails);
        }


        [Fact]
        public async Task SearchController_CarbonIndex_ShouldSucceed()
        {
            /* Arrange */
            var carbonIndexRQ = TestDataProvider.GetTestData<CarbonIndexRequest>($"Search/SearchController/CarbonIndex-RQ");

            var mockReqBody = SetupHttpRequest();

            var mockDataResponse = TestDataProvider.GetTestData<IEnumerable<CarbonIndexResponse>>($"Search/SearchController/CarbonIndex-RS");
            mediator.Setup(m => m.Send(It.IsAny<CarbonIndexRequest>(), CancellationToken.None)).ReturnsAsync(mockDataResponse);
            SearchController ctrl = new(mediator.Object, logger.Object);


            // Response Payload
            var ctrlExpectedResponse = CSharpFunctionalExtensions.Result.Success(new Response<IEnumerable<CarbonIndexResponse>>()
            {
                Status = Status.OK,
                Value = mockDataResponse
            });

            /* Act */
            var result = await ctrl.CarbonIndex(mockReqBody.Object, carbonIndexRQ.PassengerCounts, carbonIndexRQ.OfferId);

            /* Assert */
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var output = Json.Deserialize<IEnumerable<CarbonIndexResponse>>(body);
            Assert.NotNull(output);
        }


        //[Fact]
        //public async Task SearchController_Summary_ShouldSucceed()
        //{
        //    /* Arrange */
        //    var mockReqBody = SetupHttpRequest();    // Request Payload

        //    Models.Search.Response.TravelPlusSummary.Summary mockDataResponse = TestDataProvider.GetTestData<Models.Search.Response.TravelPlusSummary.Summary>("Search/RS-Summary");
        //    mediator.Setup(m => m.Send(It.IsAny<SelectAndSummaryRequest>(), CancellationToken.None)).ReturnsAsync(mockDataResponse);
        //    SearchController ctrl = new(mediator.Object);


        //    // Response Payload
        //    var ctrlExpectedResponse = CSharpFunctionalExtensions.Result.Success(new Response<Models.Search.Response.TravelPlusSummary.Summary>()
        //    {
        //        Status = Status.OK,
        //        Value = mockDataResponse
        //    });

        //    /* Act */
        //    var result = await ctrl.Summary(mockReqBody.Object, "", "");

        //    /* Assert */
        //    Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        //    result.Body.Seek(0, SeekOrigin.Begin);
        //    StreamReader rdr = new(result.Body);
        //    var body = rdr.ReadToEnd();
        //    result.Body.Close();

        //    var output = Json.Deserialize<Models.Search.Response.TravelPlusSummary.Summary>(body);
        //    Assert.NotNull(output.Price);
        //}



        //[Fact]
        //public async Task SearchController_FareInformation_ShouldSucceed()
        //{
        //    /* Arrange */
        //    var basketId = Guid.NewGuid().ToString();
        //    var offerId = Guid.NewGuid().ToString();
        //    var sliceId = $"slice2.2";
        //    var fareBasis = "OL0I21LH";
        //    var fareFamily = "ECONOMY-LIGHT";
        //    var mockReqBody = SetupHttpRequest();    // Request Payload

        //    FlightInformation mockDataResponse = TestDataProvider.GetTestData<FlightInformation>("Search/RS-FareInformation");
        //    mediator.Setup(m => m.Send(It.IsAny<FareInfoRequest>(), CancellationToken.None)).ReturnsAsync(mockDataResponse);
        //    SearchController ctrl = new(mediator.Object);


        //    // Response Payload
        //    var ctrlExpectedResponse = CSharpFunctionalExtensions.Result.Success(new Response<FlightInformation>()
        //    {
        //        Status = Status.OK,
        //        Value = mockDataResponse
        //    });

        //    /* Act */
        //    var result = await ctrl.FareInformation(mockReqBody.Object, basketId, fareBasis, offerId, sliceId, fareFamily);

        //    /* Assert */
        //    Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        //    result.Body.Seek(0, SeekOrigin.Begin);
        //    StreamReader rdr = new(result.Body);
        //    var body = rdr.ReadToEnd();
        //    result.Body.Close();

        //    var output = Json.Deserialize<FlightInformation>(body);
        //    //Assert.NotNull(output);
        //}

        //[Fact]
        //public async Task SearchController_FareInformation_NoContent()
        //{
        //    /* Arrange */
        //    var basketId = Guid.NewGuid().ToString();
        //    var offerId = Guid.NewGuid().ToString();
        //    var sliceId = $"slice2.2";
        //    var fareBasis = "OL0I21LH";
        //    var fareFamily = "ECONOMY-LIGHT";
        //    var mockReqBody = SetupHttpRequest();    // Request Payload

        //    var mockException = new GraphQLException(new List<GraphQLError>()
        //    {
        //        new()
        //        {
        //            Extensions = new GraphQLErrorExtension
        //            {
        //                Code = ((int)HttpStatusCode.NoContent).ToString()
        //            }
        //        }
        //    });
        //    mediator.Setup(m => m.Send(It.IsAny<FareInfoRequest>(), CancellationToken.None)).ThrowsAsync(mockException);

        //    SearchController ctrl = new(mediator.Object);


        //    /* Act */
        //    var result = await ctrl.FareInformation(mockReqBody.Object, basketId, fareBasis, offerId, sliceId, fareFamily);

        //    /* Assert */
        //    Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        //}

        //[Theory]
        //[InlineData(true, true, "RS-Sort-Return-OB", "slice2.0")] // 1st Leg of a return
        //[InlineData(true, false, "RS-Sort-Return-IB", "slice2.1|0ef3cf47-dd85-4ec8-9342-f6f2f4e4d212|4dbf4e35-b6ae-4615-a030-cad705c4ca0d-FL-1")] // 2nd leg of a return
        //[InlineData(false, true, "RS-Oneway", "slice1.0")] // 1st Leg of a oneway
        //public async Task SearchController_Sort(bool isReturn, bool is1stLeg, string rsFile, string fareId)
        //{
        //    /* Arrange */
        //    var basketId = Guid.NewGuid().ToString();
        //    string selectedSort = string.Empty;
        //    string airlinesFilter = string.Empty;
        //    string departureTimesFilter = string.Empty;
        //    string connectionsFilter = string.Empty;
        //    string cabinsFilter = string.Empty;

        //    var mockReqBody = SetupHttpRequest();    // Request Payload

        //    var mockDataResponse = TestDataProvider.GetTestData<SearchResultsResponse>($"Search/{rsFile}");// Response Payload
        //    mediator.Setup(m => m.Send(It.IsAny<SortRequest>(), CancellationToken.None)).ReturnsAsync(mockDataResponse);

        //    SearchController ctrl = new(mediator.Object);

        //    /* Act */
        //    var result = await ctrl.Sort(mockReqBody.Object, basketId, fareId, selectedSort, airlinesFilter, departureTimesFilter, connectionsFilter, cabinsFilter);

        //    /* Assert */
        //    Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        //    result.Body.Seek(0, SeekOrigin.Begin);
        //    StreamReader rdr = new(result.Body);
        //    var body = rdr.ReadToEnd();
        //    System.Diagnostics.Debug.WriteLine(body);
        //    result.Body.Close();

        //    if (is1stLeg)
        //    {
        //        var output = Json.Deserialize<SearchResultsResponse>(body);
        //        if (isReturn)
        //            Assert.NotNull(output.PriceGrid);
        //        Assert.NotNull(output.Result);
        //    }
        //    else
        //    {
        //        var output = Json.Deserialize<Result>(body);
        //        Assert.NotNull(output.Slice);
        //    }
        //}

        private Mock<HttpRequestData> SetupHttpRequest(string requestBodySampleFile = "")
        {

            Mock<FunctionContext> context = new();
            context.Setup(c => c.Items).Returns(() =>
            {
                var items = new Dictionary<object, object>
                {
                    { "ClaimsPrincipal", TestUser() }
                };
                return items;
            });
            var req = new Mock<HttpRequestData>(context.Object);

            if (!requestBodySampleFile.IsNullOrEmpty())
            {
                string body = TestDataProvider.GetTestData<string>(requestBodySampleFile);
                var byteArray = Encoding.ASCII.GetBytes(body);
                var bodyStream = new MemoryStream(byteArray);
                req.Setup(m => m.Body).Returns(bodyStream);
            }
            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            return req;
        }
    }
}