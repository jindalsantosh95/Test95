using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class OfferSearchRepoQueryHandlerTests
    {
        private readonly Mock<ILogger<OfferSearchRepoQueryHandler>> _logger;
        private readonly Mock<ILogger<DistributedCacheService>> _distributedCacheServiceLogger;
        private readonly DistributedCacheService _distributedCacheService;
        private readonly OfferSearchRepoQueryHandler _handler;

        public OfferSearchRepoQueryHandlerTests()
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();

            _logger = new Mock<ILogger<OfferSearchRepoQueryHandler>>();
            _distributedCacheServiceLogger = new Mock<ILogger<DistributedCacheService>>();
            _distributedCacheService = new(new Core.Xunit.Caching.DistributedCache(), _distributedCacheServiceLogger.Object);
            _handler = new OfferSearchRepoQueryHandler(_distributedCacheService);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OfferSearchRepoQueryHandler(null));
        }

        [Fact]
        public async Task Handler_ShouldReturnResponse_WithReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OfferSearchRepoQuery>("OfferMS//OfferSearchQueryRQ");
            dynamic response = TestDataProvider.GetTestData<dynamic>("OfferMS//ResponseOfferRedis");

            await _distributedCacheService.Set(request.OfferId.Split(Constants.Application.Pipeline)[0], response, false, TimeSpan.Zero, TimeSpan.FromSeconds(60));

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(request.OfferId, result.Offers.First().OfferId);
        }
    }
}
