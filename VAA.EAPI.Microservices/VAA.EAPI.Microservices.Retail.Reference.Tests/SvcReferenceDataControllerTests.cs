using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Net;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Application.Query;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Triggers;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Reference.Tests
{
    public class SvcReferenceDataControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly SvcReferenceDataController _controller;
        private readonly Mock<IDistanceAndCountryCodeQueryHandler> _mockQueryHandler;
        private readonly Mock<FunctionContext> _context;

        public SvcReferenceDataControllerTests()
        {
            _context = new Mock<FunctionContext>();
            _mockMediator = new Mock<IMediator>();
            _mockQueryHandler = new Mock<IDistanceAndCountryCodeQueryHandler>();
            _controller = new SvcReferenceDataController(_mockMediator.Object, _mockQueryHandler.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SvcReferenceDataController(null, _mockQueryHandler.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenHandlerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SvcReferenceDataController(_mockMediator.Object, null));
        }

        [Fact]
        public async Task GetSegmentDistanceAndCountrySuccsessAsync()
        {            
            DistanceAndCountryCodeResponse res = new()
            {
                DistanceInMiles = 5321,
                OriginCountryCode = "GB",
                DestinationCountryCode = "US"
            };
            var resp = Result.Success(new Response<DistanceAndCountryCodeResponse>
            {
                Status = Status.OK,
                Value = res,
            });
            var req = MockHttpRequestData.Create(_context.Object);

            _mockQueryHandler.Setup(m => m.Handle(It.IsAny<DistanceAndCountryCodeQuery>(), CancellationToken.None)).ReturnsAsync(resp);

            var result = await _controller.GetSegmentDistanceAndCountry(req.Object, "LHR", "JFK");

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            // Read in the body
            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var response = Json.Deserialize<DistanceAndCountryCodeResponse>(body);
            Assert.Equal(5321, response.DistanceInMiles);
            Assert.Equal("GB", response.OriginCountryCode);
        }       
    }
}
