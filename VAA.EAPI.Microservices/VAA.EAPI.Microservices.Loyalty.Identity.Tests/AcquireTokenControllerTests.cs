using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class AcquireTokenControllerTests
    {
        private readonly Mock<IMediator> _mediator;

        public AcquireTokenControllerTests()
        {
            _mediator = new Mock<IMediator>();
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AcquireTokenController(null));
        }

        [Fact]
        public async Task ShouldReturnSuccessResponse_ForValidRequest()
        {
            //Arrange
            Mock<FunctionContext> context = new();
            var request = new Mock<HttpRequestData>(context.Object);
            List<KeyValuePair<string, string>> headers = new();
            {
                headers.Add(new KeyValuePair<string, string>("Pnr", "dummy"));
                headers.Add(new KeyValuePair<string, string>("FirstName", "dummy"));
                headers.Add(new KeyValuePair<string, string>("LastName", "dummy"));
                headers.Add(new KeyValuePair<string, string>("TicketRef", "dummy"));
                headers.Add(new KeyValuePair<string, string>("Type", "dummy"));

            };
            request.Setup(r => r.Headers).Returns(new HttpHeadersCollection(headers));
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });
            _mediator.Setup(x => x.Send(It.IsAny<AcquireTokenRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<string>() { Value = "dummyToken", Status = EAPI.Domain.Entities.Common.Status.OK });

            //Act
            var handler = new AcquireTokenController(_mediator.Object);
            var result = await handler.AcquireToken(request.Object, context.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }
    }
}
