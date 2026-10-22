using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System;
using System.IO;
using System.Text;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class SupplementarySilverAccountControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly SupplementarySilverAccountController _controller;
        private readonly Mock<FunctionContext> _context;

        public SupplementarySilverAccountControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new SupplementarySilverAccountController(_mockMediator.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SupplementarySilverAccountController(null));
        }

        [Fact]
        public void GetSupplementarySilverAccountDetailsSuccsess()
        {
            var req = SetupTestDataRequests();
            _mockMediator.Setup(m => m.Send(It.IsAny<SupplementarySilverAccountDetailsQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<SupplementarySilverAccountResponse> { });
            var Output = _controller.GetSupplementarySilverAccountDetailsAsync(req.Object, "c220a7fb-8b85-4f4f-bac8-db94252a3e45");
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);
        }


        private Mock<HttpRequestData> SetupTestDataRequests()
        {
            var byteArray = Encoding.ASCII.GetBytes("{\"customerId\": \"c220a7fb-8b85-4f4f-bac8-db94252a3e45\"}");
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

        [Fact]
        public void DelinkSupplementarySilverAccountAsyncSuccsess()
        {
            var req = SetupTestDataRequests("MemberManagement\\SupplementarySilverDeLinkingRequest");
            _mockMediator.Setup(m => m.Send(It.IsAny<SupplementarySilverDelinkingCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<SupplementarySilverDelinkingResponse> { });
            var Output = _controller.DelinkSupplementarySilverAccountAsync(req.Object);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);
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
                response.SetupProperty(r => r.StatusCode);
                return response.Object;
            });
            return req;
        }
    }
}
