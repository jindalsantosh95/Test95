using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Passenger.Application.Commands;
using VAA.EAPI.Microservices.Retail.Passenger.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Passenger.Tests
{
    public class PassengerControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<PassengerController>> _mockLogger;
        private readonly Mock<FunctionContext> _context;
        private readonly PassengerController _controller;

        public PassengerControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _mockLogger = new Mock<ILogger<PassengerController>>();
            _controller = new PassengerController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task TestController_AddPassengerSuccess()
        {
            //Arrange
            var request = SetupTestDataRequests("PassengerMS\\AddPassengerRequest");

            _mockMediator.Setup(m => m.Send(It.IsAny<AddPassengerCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<Domain.Entities.AddResponseType>() { });

            //Act
            var result = await _controller.Add(request.Object, _context.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Accepted, result.StatusCode);
        }
        [Fact]
        public void TestController_AddPassengerFailure()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object); //Empty request

            //Act
            var result = _controller.Add(request.Object, _context.Object);

            //Assert
            Assert.False(result.IsCompletedSuccessfully);

        }
        [Fact]
        public async Task TestController_AddPTCCodeSuccess()
        {
            //Arrange

            var request = SetupTestDataRequests("PassengerMS\\AddPTCCodesRequest");


            _mockMediator.Setup(m => m.Send(It.IsAny<AddPTCCodeCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<Domain.Entities.AddPTCCodesResponseType>() { });

            //Act
            var result = await _controller.AddPTCCodes(request.Object, _context.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Accepted, result.StatusCode);
        }
        [Fact]
        public void TestController_AddPTCCodeFailure()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object); //Empty request

            //Act
            var result = _controller.AddPTCCodes(request.Object, _context.Object);

            //Assert
            Assert.False(result.IsCompletedSuccessfully);

        }

        [Fact]
        public async Task TestController_AddPhoneSuccess()
        {
            //Arrange
            var request = SetupTestDataRequests("PassengerMS\\AddPhoneRequest");

            _mockMediator.Setup(m => m.Send(It.IsAny<AddPhoneCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<Domain.Entities.AddPhoneResponseType>() { });

            //Act
            var result = await _controller.AddPhone(request.Object, _context.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Accepted, result.StatusCode);
        }
        [Fact]
        public void TestController_AddPhoneFailure()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object); //Empty request

            //Act
            var result = _controller.AddPhone(request.Object, _context.Object);

            //Assert
            Assert.False(result.IsCompletedSuccessfully);

        }
        [Fact]
        public async Task TestController_ManageMultipleTravelDocsAndSecureFlightSuccess()
        {
            //Arrange
            var request = SetupTestDataRequests("PassengerMS\\ManageMultipleTravelDocsAndSecureFlightRequest");


            _mockMediator.Setup(m => m.Send(It.IsAny<ManageMultipleTravelDocsAndSecureFlightCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<Domain.Entities.ManageMultipleTravelDocsAndSecureFlightResponseType>() { });

            //Act
            var result = await _controller.ManageMultipleTravelDocsAndSecureFlight(request.Object, _context.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Accepted, result.StatusCode);
        }
        [Fact]
        public void TestController_ManageMultipleTravelDocsAndSecureFlightFailure()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object); //Empty request

            //Act
            var result = _controller.ManageMultipleTravelDocsAndSecureFlight(request.Object, _context.Object);

            //Assert
            Assert.False(result.IsCompletedSuccessfully);

        }

        [Fact]
        public async Task TestController_ManageMultipleInfantSuccess()
        {
            var request = SetupTestDataRequests("PassengerMS\\ManageMultipleTravelDocsAndSecureFlightRequest");


            _mockMediator.Setup(m => m.Send(It.IsAny<ManageMultipleInfantCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<Domain.Entities.ManageMultipleInfantV2Response>() { });

            //Act
            var result = await _controller.ManageMultipleInfantV2(request.Object, _context.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Accepted, result.StatusCode);

        }
        [Fact]
        public void TestController_ManageMultipleInfantFailure()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object); //Empty request

            //Act
            var result = _controller.ManageMultipleInfantV2(request.Object, _context.Object);

            //Assert
            Assert.False(result.IsCompletedSuccessfully);

        }
        private Mock<HttpRequestData> SetupTestDataRequests(string requestBodySampleFile)
        {

            //Arrange
            string body = TestDataProvider.GetTestData<string>(requestBodySampleFile);
            var byteArray = Encoding.ASCII.GetBytes("{\"channelId\":null}");
            var bodyStream = new MemoryStream(byteArray);

            var req = new Mock<HttpRequestData>(_context.Object);
            req.Setup(m => m.Body).Returns(bodyStream);
            req.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("channelId", "VHOLS") }));
            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                return response.Object;
            });
            return req;
        }
    }
}
