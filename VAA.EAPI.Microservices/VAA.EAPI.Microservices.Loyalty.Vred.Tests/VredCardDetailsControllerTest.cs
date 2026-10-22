using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails;
using VAA.EAPI.Microservices.Loyalty.Vred.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class VredCardDetailsControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<FunctionContext> _context;

        public VredCardDetailsControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new VredCardDetailsController(null));
        }

        [Fact]
        public async Task GetVredCardDetailsSuccsess()
        {
            // Arrange
            var req = SetupTestDataRequests(); 

            var dataResponse = TestDataProvider.GetTestData<Response<VredCardDetailsResponse>>("VRedMS\\VredCardDetailsResponse");
            _mockMediator.Setup(m => m.Send(It.IsAny<VredCardDetailsQuery>(), CancellationToken.None)).ReturnsAsync(dataResponse);
                      
            // Act
            VredCardDetailsController controller = new(_mockMediator.Object);
            var result = await controller.GetVredCardDetailsByMemberID(req.Object, "9106913081");

            Assert.NotNull(result);
        }

        private Mock<HttpRequestData> SetupTestDataRequests()
        {
            var byteArray = Encoding.ASCII.GetBytes("{\"memberNumber\": \"9106913081\"}");
            var bodyStream = new MemoryStream(byteArray);
            var req = new Mock<HttpRequestData>(_context.Object);

            req.Setup(m => m.Body).Returns(bodyStream);

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
