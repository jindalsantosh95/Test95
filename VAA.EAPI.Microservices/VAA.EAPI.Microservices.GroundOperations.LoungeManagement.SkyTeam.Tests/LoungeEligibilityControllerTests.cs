using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Application.Command;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Tests
{
    public class LoungeEligibilityContollerTests
    {
        private readonly Mock<ILogger<LoungeEligibilityController>> _mockLogger;
        private readonly Mock<IMediator> _mockMediator;
        private readonly LoungeEligibilityController _controller;
        private readonly Mock<FunctionContext> _context;

        public LoungeEligibilityContollerTests()
        {
            _mockLogger = new Mock<ILogger<LoungeEligibilityController>>();
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new LoungeEligibilityController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoungeEligibilityController(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoungeEligibilityController(null, _mockLogger.Object));
        }

        [Fact]
        public async Task Eligibility()
        {
            var req = SetupTestDataRequests("EligibilityMS\\EligibilityRequest");
            req.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Content-Type", "application/json") }));
            _mockMediator.Setup(m => m.Send(It.IsAny<LoungeEligibilityCommand>(), CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<LoungeEligibilityResponse> { Value = new LoungeEligibilityResponse { TrackId = "TRACK987654321", EligibilityCode = "ELIGIBLE" } });

            var output = await _controller.Eligibility(req.Object);

            var retroResult = await EligibilityTestHelpers.ReadResponseData<LoungeEligibilityResponse>(output);

            Assert.Equal(System.Net.HttpStatusCode.OK, output.StatusCode);
            Assert.NotNull(retroResult);
            Assert.Equal("ELIGIBLE", retroResult.EligibilityCode);
        }

        [Fact]
        public async Task EligibilityOverride()
        {
            var req = SetupTestDataRequests("OverrideMS\\OverrideRequest");
            req.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Content-Type", "application/json") }));
            _mockMediator.Setup(m => m.Send(It.IsAny<LoungeEligibilityOverrideCommand>(), CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<LoungeEligibilityOverrideResponse> { Value = new LoungeEligibilityOverrideResponse { TrackId = "TRACK987654321", TransactionId = "ABC123456789", } });

            var output = await _controller.EligibilityOverride(req.Object);

            var retroResult = await EligibilityTestHelpers.ReadResponseData<LoungeEligibilityOverrideResponse>(output);

            Assert.Equal(System.Net.HttpStatusCode.OK, output.StatusCode);
            Assert.NotNull(retroResult);
            Assert.Equal("ABC123456789", retroResult.TransactionId);
        }

        private Mock<HttpRequestData> SetupTestDataRequests(string requestBodySampleFile)
        {
            var req = new Mock<HttpRequestData>(_context.Object);
            if (requestBodySampleFile != null)
            {
                string body = TestDataProvider.GetTestData<string>(requestBodySampleFile);
                var byteArray = Encoding.ASCII.GetBytes(body);
                var bodyStream = new MemoryStream(byteArray);
                req.Setup(m => m.Body).Returns(bodyStream);
            }

            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode, System.Net.HttpStatusCode.OK);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            return req;
        }
    }
}
