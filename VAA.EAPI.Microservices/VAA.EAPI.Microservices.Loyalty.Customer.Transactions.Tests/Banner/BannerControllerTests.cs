using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using System.Linq;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests.Banner
{
    public class BannerControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();
        private readonly Mock<ILogger<BannerController>> _mockloggerr = new();
        private readonly Mock<FunctionContext> _context = new();
        private readonly BannerController _controller;

        public BannerControllerTests()
        {
            _controller = new BannerController(_mockMediator.Object, _mockloggerr.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BannerController(null, _mockloggerr.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BannerController(_mockMediator.Object, null));
        }
        
        [Fact]
        public async Task GetBannerAsync_Success()
        {
            // Arrange
            var banner = TestDataProvider.GetTestData<object>("CustomerTransactionMS\\BannerDoc");

            var bannerResponse = Result.Success(new Response<BannerResponse>()
            {
                Status = Status.OK,
                Value = new BannerResponse(string.Empty, new List<object>{ banner })
            });

            _mockMediator.Setup(x => x.Send(It.IsAny<BannerQuery>(), CancellationToken.None)).ReturnsAsync(bannerResponse);

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            var result = await _controller.GetBannerDetailsByCustomerId(req.Object, "testCustomerID");

            var response = await ReadResponseBodyAsync<BannerDetail>(result);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Banner", response.DocType);
            Assert.Equal("163950bc-da97-4464-9fa2-617c15dc08ff", response.CustomerId);
            Assert.IsType<List<Banners>>(response.Banners);
            Assert.Equal(3, response.Banners.Count());
            Assert.Equal("testBanner1", response.Banners.First().BannerCode);
        }

        [Fact]
        public async Task GetStatusForPatchCommandAsync_Success()
        {
            // Arrange
            var banner = TestDataProvider.GetTestData<BannerDetail>("CustomerTransactionMS\\BannerDoc");
            var correlationId = "e81b1c92-6b54-496d-aa21-bfbdeff7745e";

            var statusResponse = Result.Success(new Response<BannerCommandQueryResponse>()
            {
                Status = Status.OK,
                Value = new BannerCommandQueryResponse()
                {
                    Banner = banner,
                    CommandStatus = Status.OK,
                    RequestId = correlationId
                }
            });

            _mockMediator.Setup(x => x.Send(It.IsAny<BannerCommandStatusQuery>(), CancellationToken.None)).ReturnsAsync(statusResponse);

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            var result = await _controller.GetStatusOfBannerCommand(req.Object, correlationId);

            var response = await ReadResponseBodyAsync<BannerCommandQueryResponse>(result);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.IsType<BannerCommandQueryResponse>(response);
            Assert.Null(response.Errors);
            Assert.IsType<BannerDetail>(response.Banner);
            Assert.Equal("Banner", response.Banner.DocType);
            Assert.Equal("163950bc-da97-4464-9fa2-617c15dc08ff", response.Banner.CustomerId);
            Assert.Equal(3, response.Banner.Banners.Count());
            Assert.Equal(Status.OK, response.CommandStatus);

        }

        [Fact]
        public async Task GetStatusForPatchCommandAsync_Filure()
        {
            // Arrange
            var banner = TestDataProvider.GetTestData<BannerDetail>("CustomerTransactionMS\\BannerDoc");
            var correlationId = "e81b1c92-6b54-496d-aa21-bfbdeff7745e";

            var statusResponse = Result.Success(new Response<BannerCommandQueryResponse>()
            {
                Status = Status.OK,
                Value = new BannerCommandQueryResponse()
                {
                    Banner = null,
                    CommandStatus = Status.Error,
                    RequestId = correlationId,
                    Errors = new List<ErrorDetails>()
                }
            });

            _mockMediator.Setup(x => x.Send(It.IsAny<BannerCommandStatusQuery>(), CancellationToken.None)).ReturnsAsync(statusResponse);

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            var result = await _controller.GetStatusOfBannerCommand(req.Object, correlationId);

            var response = await ReadResponseBodyAsync<BannerCommandQueryResponse>(result);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.IsType<BannerCommandQueryResponse>(response);
            Assert.Null(response.Banner);
            Assert.NotNull(response.Errors);
            Assert.Equal(Status.Error, response.CommandStatus);
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

        private static async Task<T> ReadResponseBodyAsync<T>(HttpResponseData response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(response.Body);
            string body = await reader.ReadToEndAsync();
            response.Body.Close();

            return JsonConvert.DeserializeObject<T>(body);
        }
    }
}
