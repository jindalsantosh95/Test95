using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Seat.Tests
{
    public class SeatAvailabilityControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly SeatAvailabilityController _controller;
        private readonly Mock<FunctionContext> _context;

        public SeatAvailabilityControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new SeatAvailabilityController(_mediator.Object);
            _context = new Mock<FunctionContext>();
            
            SeatAvailabilityEnvSetup.CreateTestEnvironmentSettings();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new SeatAvailabilityController(null));

        }

        [Fact]
        public void Controller_ShouldFail_WithBadRequest()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object);
            SeatAvailabilityController controller = new SeatAvailabilityController(_mediator.Object);

            //Act
            var result = controller.RetrieveSeatMap(request.Object);

            //Asserts
            Assert.False(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task Controllertest_RetrieveSeatMapWithPNR_ShouldSucceed()
        {
            //Arrange            
            string body = TestDataProvider.GetTestData<string>(@"SeatMS\SeatMsRequestPNR");
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });
            _mediator.Setup(m => m.Send(It.IsAny<SeatAvailabilityQuery>(), CancellationToken.None)).ReturnsAsync(new Response<Domain.SeatAvailability.SeatAvailabilityResponse>() { Status = Status.OK });

            //Act
            var result = await _controller.RetrieveSeatMap(request.Object);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        }

        [Fact]
        public async Task Controllertest_RetrieveSeatMapWithoutPNR_ShouldSucceed()
        {
            //Arrange            
            string body = TestDataProvider.GetTestData<string>(@"SeatMS\SeatMsRequestWOPNR");
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });
            _mediator.Setup(m => m.Send(It.IsAny<SeatAvailabilityQuery>(), CancellationToken.None)).ReturnsAsync(new Response<Domain.SeatAvailability.SeatAvailabilityResponse>() { Status = Status.OK });

            //Act
            var result = await _controller.RetrieveSeatMap(request.Object);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        }
    }
}