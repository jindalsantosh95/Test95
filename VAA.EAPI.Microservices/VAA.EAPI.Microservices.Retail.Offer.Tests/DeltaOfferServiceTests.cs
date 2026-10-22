using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class DeltaOfferServiceTests
    {
        private readonly Mock<IDeltaOfferService> _deltaOfferService;
        private readonly Mock<ILogger<FlexiDateSearchQueryHandler>> _logger;

        public DeltaOfferServiceTests()
        {
            _deltaOfferService = new Mock<IDeltaOfferService>();
            _logger = new Mock<ILogger<FlexiDateSearchQueryHandler>>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenDeltaRQIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new FlexiDateSearchQueryHandler(null, _logger.Object));
        }
    }
}
