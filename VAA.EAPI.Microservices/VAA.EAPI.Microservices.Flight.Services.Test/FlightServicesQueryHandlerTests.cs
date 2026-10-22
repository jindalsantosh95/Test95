using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Flight.Services.Application.Query;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Helper.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Flight.Services.Tests
{
    public class FlightServiceQueryHandlerTests
    {        
        private readonly Mock<ICachedSsrDetails> _mockCachedSsrDetails;
        private readonly Mock<ILogger<FlightServiceQueryHandler>> _mockLogger;
        private readonly Mock<IDistributedCacheService> _mockDistibutedCacheService;


        public FlightServiceQueryHandlerTests()
        {            
            _mockLogger = new Mock<ILogger<FlightServiceQueryHandler>>();
            _mockCachedSsrDetails = new Mock<ICachedSsrDetails>();
            _mockDistibutedCacheService = new Mock<IDistributedCacheService>();
        }


        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new FlightServiceQueryHandler(_mockCachedSsrDetails.Object, _mockLogger.Object, null)); ;
        }

    }
}
