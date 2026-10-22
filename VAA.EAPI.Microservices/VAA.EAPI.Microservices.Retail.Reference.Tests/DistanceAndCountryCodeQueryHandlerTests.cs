using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Microservices.Retail.Reference.Application.Query;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Reference.Tests
{
    public class DistanceAndCountryCodeQueryHandlerTests
    {
        private readonly DistanceAndCountryCodeQueryHandler _handler;
        private readonly Mock<IScvRefDBRepository> _mockScvRefDBRepository;
        private readonly Mock<ILogger<DistanceAndCountryCodeQueryHandler>> _mockLogger;

        public DistanceAndCountryCodeQueryHandlerTests()
        {
            _mockLogger = new Mock<ILogger<DistanceAndCountryCodeQueryHandler>>();
            _mockScvRefDBRepository = new Mock<IScvRefDBRepository>();
            _handler = new DistanceAndCountryCodeQueryHandler(_mockScvRefDBRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenRepoObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new DistanceAndCountryCodeQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new DistanceAndCountryCodeQueryHandler(_mockScvRefDBRepository.Object, null));
        }

        [Fact]
        public async Task HandlerSuccess()
        {
            DistanceAndCountryCodeQuery query = new() { OriginAirportCode = "LHR", DestinationAirportCode = "JFK" };
            DistanceAndCountryCodeResponse res = new()
            {
                DistanceInMiles = 5432,
                OriginCountryCode = "GB",
                DestinationCountryCode = "US"
            };

            _mockScvRefDBRepository.Setup(m => m.GetDistanceWithCountryCode(new Distance())).Returns(Task.FromResult<DistanceAndCountryCodeResponse>(res));
            
            var result = _handler.Handle(query, CancellationToken.None);

            Assert.True(result.Result.IsSuccess);
        }
    }
}
