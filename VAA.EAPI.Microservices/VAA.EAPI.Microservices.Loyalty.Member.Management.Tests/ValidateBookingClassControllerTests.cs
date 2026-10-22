using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class ValidateBookingClassControllerTest
    {
        private readonly Mock<ILogger<ValidateBookingClassController>> _mockLogger;
        private readonly Mock<IMediator> _mockMediator;
        private readonly ValidateBookingClassController _controller;
        private readonly Mock<FunctionContext> _context;

        public ValidateBookingClassControllerTest()
        {
            _mockLogger = new Mock<ILogger<ValidateBookingClassController>>();
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new ValidateBookingClassController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ValidateBookingClassController(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ValidateBookingClassController(null, _mockLogger.Object));
        }

        [Fact]
        public async Task ValidateBookingClassSuccess()
        {
            // Arrange
            var req = SetupTestDataRequests();

            var queryResponse = new BookingDetailsResult
            {
                BookClass = "Y",
                CabinClass = "Y",
                DestinationCountryCode = "US",
                DistanceInMiles = 1689,
                OriginCountryCode = "UK"
            };

            var response = new Response<BookingDetailsResult>()
            {
                Status = Status.OK,
                Errors = null,
                IgnoreNullValues = true,
                Value = queryResponse
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<ValidateBookingClassQuery>(), CancellationToken.None)).ReturnsAsync(response);

            // Act
            var output = await _controller.ValidateBooking(req.Object);
            var result = await ReadResponseData<BookingDetailsResult>(output);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, output.StatusCode);
            Assert.NotNull(result);
            Assert.IsType<BookingDetailsResult>(result);
        }


        private Mock<HttpRequestData> SetupTestDataRequests()
        {
            var req = new Mock<HttpRequestData>(_context.Object);
            req.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes("{}")));
            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });
            return req;
        }

        private static async Task<T> ReadResponseData<T>(HttpResponseData responseData)
        {
            responseData.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(responseData.Body);
            var responseBody = await reader.ReadToEndAsync();
            return Json.Deserialize<T>(responseBody);
        }
    }
}