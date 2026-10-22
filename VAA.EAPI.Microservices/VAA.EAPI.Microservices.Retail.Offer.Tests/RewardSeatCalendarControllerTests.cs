using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Interfaces;
using VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{

    public sealed class RewardSeatCalendarControllerTests
    {
        [Fact]
        public async Task SearchAsync_ReturnsJsonContentTypeOnSuccess()
        {
            var service = new CountingCalendarService();
            var context = new TestFunctionContext();
            using var body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
            var request = new TestHttpRequestData(context,
                new Uri("https://retail-offer.test/api/reward-seat-calendar-availability"), body, "POST");
            request.Headers.Add("x-reward-seat-deadline-ms", "1000");
            var controller = new RewardSeatCalendarController(service, NullLogger<RewardSeatCalendarController>.Instance);

            var response = await controller.SearchAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.TryGetValues("Content-Type", out var contentTypes));
            Assert.Equal("application/json", System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Assert.Single(contentTypes)).MediaType);
            Assert.Equal(1, service.Calls);
            response.Body.Position = 0;
            var payload = Json.Deserialize<RewardSeatCalendarResponse>(await new StreamReader(response.Body).ReadToEndAsync(TestContext.Current.CancellationToken));
            Assert.True(payload.Complete);
            Assert.Empty(payload.Candidates);
        }

        [Fact]
        public async Task SearchAsync_RejectsRepeatedDeadlineHeaderWithBadRequest()
        {
            var service = new CountingCalendarService();
            var context = new TestFunctionContext();
            var request = new TestHttpRequestData(context, new Uri("https://retail-offer.test/api/reward-seat-calendar-availability"),
                method: "POST");
            request.Headers.Add("x-reward-seat-deadline-ms", ["1000", "2000"]);
            var controller = new RewardSeatCalendarController(service, NullLogger<RewardSeatCalendarController>.Instance);

            var response = await controller.SearchAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, service.Calls);
            await AssertErrorAsync(response, Constants.ErrorCodes.RewardSeatInvalidDeadline);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("0")]
        [InlineData("30001")]
        [InlineData("invalid")]
        public async Task SearchAsync_ReturnsStructuredErrorForInvalidDeadline(string? deadline)
        {
            var service = new CountingCalendarService();
            var request = new TestHttpRequestData(new TestFunctionContext(),
                new Uri("https://retail-offer.test/api/reward-seat-calendar-availability"), method: "POST");
            if (deadline is not null) request.Headers.Add("x-reward-seat-deadline-ms", deadline);
            var controller = new RewardSeatCalendarController(service, NullLogger<RewardSeatCalendarController>.Instance);

            var response = await controller.SearchAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, service.Calls);
            await AssertErrorAsync(response, Constants.ErrorCodes.RewardSeatInvalidDeadline);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SearchAsync_ReturnsSafeStructuredServiceErrors(bool invalidRequest)
        {
            var service = new CountingCalendarService
            {
                Failure = invalidRequest ? new ArgumentException("private downstream detail") : new InvalidOperationException("private downstream detail")
            };
            using var body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
            var request = new TestHttpRequestData(new TestFunctionContext(),
                new Uri("https://retail-offer.test/api/reward-seat-calendar-availability"), body, "POST");
            request.Headers.Add("x-reward-seat-deadline-ms", "1000");
            var controller = new RewardSeatCalendarController(service, NullLogger<RewardSeatCalendarController>.Instance);

            var response = await controller.SearchAsync(request, CancellationToken.None);

            Assert.Equal(invalidRequest ? HttpStatusCode.BadRequest : HttpStatusCode.BadGateway, response.StatusCode);
            await AssertErrorAsync(response, invalidRequest ? Constants.ErrorCodes.RewardSeatInvalidRequest : Constants.ErrorCodes.RewardSeatUnavailable);
        }

        [Fact]
        public async Task SearchAsync_ReturnsStructuredErrorForMalformedJson()
        {
            var service = new CountingCalendarService();
            using var body = new MemoryStream(Encoding.UTF8.GetBytes("{"));
            var request = new TestHttpRequestData(new TestFunctionContext(),
                new Uri("https://retail-offer.test/api/reward-seat-calendar-availability"), body, "POST");
            request.Headers.Add("x-reward-seat-deadline-ms", "1000");
            var controller = new RewardSeatCalendarController(service, NullLogger<RewardSeatCalendarController>.Instance);

            var response = await controller.SearchAsync(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, service.Calls);
            await AssertErrorAsync(response, Constants.ErrorCodes.RewardSeatInvalidRequest);
        }

        private static async Task AssertErrorAsync(HttpResponseData response, string expectedCode)
        {
            Assert.True(response.Headers.TryGetValues("Content-Type", out var contentTypes));
            Assert.Equal("application/json", System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Assert.Single(contentTypes)).MediaType);
            foreach (var header in VAA.EAPI.Infrastructure.Constants.Constants.SecurityHeaders)
            {
                Assert.True(response.Headers.TryGetValues(header.Key, out var values));
                Assert.Equal(header.Value, Assert.Single(values));
            }
            response.Body.Position = 0;
            var json = await new StreamReader(response.Body).ReadToEndAsync();
            var error = Assert.Single(Json.Deserialize<List<ErrorDetails>>(json));
            Assert.Equal(expectedCode, error.Code);
            Assert.False(error.Message.IsNullOrWhiteSpace());
            Assert.True(Guid.TryParse(error.Id, out _));
            Assert.DoesNotContain("private downstream detail", json);
        }

        private sealed class CountingCalendarService : IRewardSeatCalendarService
        {
            public int Calls { get; private set; }
            public Exception? Failure { get; init; }

            public Task<RewardSeatCalendarResponse> SearchAsync(RewardSeatCalendarRequest request,
                TimeSpan remainingDeadline, CancellationToken cancellationToken)
            {
                Calls++;
                if (Failure is not null) throw Failure;
                return Task.FromResult(new RewardSeatCalendarResponse
                {
                    Candidates = [],
                    MarketPolicyVersion = "v1",
                    FareFamilyMappingVersion = "v1",
                    Complete = true
                });
            }
        }
    }
}
