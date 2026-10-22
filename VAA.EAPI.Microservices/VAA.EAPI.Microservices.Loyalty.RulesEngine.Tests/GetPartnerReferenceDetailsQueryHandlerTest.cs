using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Query;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Helper.Cache.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class GetPartnerReferenceDetailsQueryHandlerTest
    {
        private readonly Mock<ILogger<GetPartnerReferenceDetailsQuery>> _logger;
        private readonly Mock<ICachedVSPartner> _cachedPartner;
        private readonly GetPartnerReferenceDetailsQueryHandler _handler;

        public GetPartnerReferenceDetailsQueryHandlerTest()
        {
            _logger = new Mock<ILogger<GetPartnerReferenceDetailsQuery>>();
            _cachedPartner = new Mock<ICachedVSPartner>();
            _handler = new GetPartnerReferenceDetailsQueryHandler(_cachedPartner.Object, _logger.Object);
        }


        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetPartnerReferenceDetailsQueryHandler(_cachedPartner.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCachedPartnerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new GetPartnerReferenceDetailsQueryHandler(null, _logger.Object));
        }


        [Fact]
        public async Task HandlerSuccessWithNoErrorAsync()
        {
            var request = new GetPartnerReferenceDetailsQuery() { PartnerCode = "XX" };
            var partnerId = "0000";
            _cachedPartner.Setup(m => m.GetCachedVSPartnerData("XX")).ReturnsAsync(partnerId);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.Equal("0000", result);
        }

        [Fact]
        public async Task HandlerSuccessWithException()
        {
            var request = new GetPartnerReferenceDetailsQuery() { PartnerCode = null };
            _cachedPartner.Setup(m => m.GetCachedVSPartnerData(null)).ThrowsAsync(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
