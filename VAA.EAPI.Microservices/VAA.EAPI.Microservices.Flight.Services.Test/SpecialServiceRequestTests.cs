using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta;
using Xunit;

namespace VAA.EAPI.Microservices.Flight.Services.Tests
{
    public sealed class SpecialServiceRequestTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;

        public SpecialServiceRequestTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);

            ConfigReader.Set("DlSSRUri", "https://specialservicerequest-api-si.vs01.vs.air4.com/v1/ssrs");
            ConfigReader.Set("x-air4_mytrips_test", "7ec41bf046024f0084c3b42f66ea4e3f");
        }

        [Fact]
        public async Task SSRService()
        {
            var SsrService = new DeltaSSRDetailsService(_httpConnector);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            var response = await SsrService.RetrieveSSRDetails("74508453-b3d7-42a7-b058-ca0b3cd88888");

            Assert.NotNull(response);
            Assert.NotNull(response.SpecialServiceRequests);
            Assert.True(response.SpecialServiceRequests.Passengers.Any());
            Assert.True(response.SpecialServiceRequests.Passengers.First().SSRSegments.Any());
            Assert.True(response.SpecialServiceRequests.Passengers.First().SSRSegments.First().SpecialServiceRequests.Any());
        }

        [Fact]
        public async Task ManageSSRService()
        {
            var SsrService = new DeltaSSRDetailsService(_httpConnector);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            Infrastructure.Delta.SSR.SSRUpsertRequest sSRUpsertRequest = new()
            {
                ApplyToAll = true,
                PassengerResourceId = Guid.NewGuid().ToString(),
                ActionType = Infrastructure.Delta.SSR.SSRModifyActionType.EDIT,
                SSRSegments = [
                    new()
                    {
                    DatedOperatingSegment = new Infrastructure.Delta.SSR.DatedOperatingSegment(){SegmentId = 1},
                    SpecialServiceRequests = [new() {Code = "WCHR",Status = "HK"}]
                    }
                ]
            };
            var response = await SsrService.ManageSSRDetailsAsync(sSRUpsertRequest, "84508453-b3d7-42a7-b058-ca0b3cd88888");

            Assert.NotNull(response);
            Assert.NotNull(response.SpecialServiceRequests);
            Assert.True(response.SpecialServiceRequests.Passengers.Any());
            Assert.True(response.SpecialServiceRequests.Passengers.First().SSRSegments.Any());
            Assert.True(response.SpecialServiceRequests.Passengers.First().SSRSegments.First().SpecialServiceRequests.Any());
        }
    }
}
