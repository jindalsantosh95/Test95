using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Operations.Application.Query;
using VAA.EAPI.Microservices.Operations.Triggers.Http;

namespace VAA.EAPI.Microservices.Operations.Tests.AirportInfo
{
    public class AirportInfoControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<AirportInfoController>> _mockLogger;
        private readonly AirportInfoController _controller;
        private readonly Mock<FunctionContext> _context;


        public AirportInfoControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AirportInfoController>>();
            _controller = new AirportInfoController(_mockMediator.Object, _mockLogger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AirportInfoController(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AirportInfoController(_mockMediator.Object, null));
        }

        [Fact]
        public async Task AirportInfoController_ShouldSucceed()
        {
            // Arrange
            var response = new Response<ICollection<Domain.AirportInfo.AirportInfo>>()
            {
                Status = Status.OK,
                Value = new List<Domain.AirportInfo.AirportInfo>()
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<AirportInfoQuery>(), CancellationToken.None)).ReturnsAsync(response);

            var req = new Mock<HttpRequestData>(_context.Object);
            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode, System.Net.HttpStatusCode.OK);
                response.SetupProperty(r => r.Body, new MemoryStream());

                return response.Object;
            });

            // Act
            var Output = await _controller.GetAirportInfoList(req.Object);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, Output.StatusCode);
        }
    }
}