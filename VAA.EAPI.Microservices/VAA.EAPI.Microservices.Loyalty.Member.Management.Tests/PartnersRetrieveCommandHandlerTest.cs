using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.PartnersRetrieval;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class PartnersRetrieveCommandHandlerTest
    {
        private readonly Mock<ICachedPartners> _mockCachedPartners;
        private readonly Mock<ILogger<PartnersRetrieveCommandHandler>> _mockLogger;
        private readonly PartnersRetrieveCommandHandler _handler;
        private readonly Mock<FunctionContext> _context;

        public PartnersRetrieveCommandHandlerTest()
        {
            _mockCachedPartners = new Mock<ICachedPartners>();
            _context = new Mock<FunctionContext>();
            _mockLogger = new Mock<ILogger<PartnersRetrieveCommandHandler>>();
            _handler = new PartnersRetrieveCommandHandler(_mockCachedPartners.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PartnersRetrieveCommandHandler(null, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new PartnersRetrieveCommandHandler(_mockCachedPartners.Object, null));
        }

        [Fact]
        public void HandlerShouldSuccessWithNoError()
        {
            PartnersRetrieveCommand req = null;
            var partnersResponse = TestDataProvider.GetTestData<PartnersResponse>("MemberManagement\\PartnersResponse");
            _mockCachedPartners.Setup(x => x.GetCachedPartnersReferenceData()).ReturnsAsync(new Response<PartnersResponse> { Status = Status.OK, Value = partnersResponse });
            var result = _handler.Handle(req, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerShouldSuccessWithException()
        {
            PartnersRetrieveCommand req = null;
            var partnersResponse = TestDataProvider.GetTestData<PartnersResponse>("MemberManagement\\PartnersResponse");
            _mockCachedPartners.Setup(x => x.GetCachedPartnersReferenceData()).Throws(new Exception());
            var result = _handler.Handle(req, System.Threading.CancellationToken.None);
            Assert.NotNull(result.Exception);
        }
    }
}
