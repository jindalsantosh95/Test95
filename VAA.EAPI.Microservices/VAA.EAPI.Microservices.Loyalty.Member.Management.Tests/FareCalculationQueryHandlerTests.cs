using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FarePointCalc;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class FareCalculationQueryHandlerTests
    {
        private readonly Mock<ILogger<FareCalculationQueryHandler>> _mockLogger;
        private readonly FareCalculationQueryHandler _handler;
        public FareCalculationQueryHandlerTests()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
            _mockLogger = new Mock<ILogger<FareCalculationQueryHandler>>();
            _handler = new FareCalculationQueryHandler(_mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FareCalculationQueryHandler(null));
        }

        [Fact]
        public async Task HandlerSuccess()
        {
            var request = TestDataProvider.GetTestData<FareCalculationRequest>("MemberManagement\\FareCalculationRequest");
            FareCalculationQuery query = new FareCalculationQuery { FareCalculationRequest = request };

            var result = await _handler.Handle(query, CancellationToken.None);
           
            Assert.Equal(120000, result.Value.Value.TotalFarePoints);
            Assert.Equal(110000, result.Value.Value.PaxFares.FirstOrDefault().FarePoints);
            Assert.Equal("JACQUES", result.Value.Value.PaxFares.FirstOrDefault().FirstName);
            Assert.Equal("RICK", result.Value.Value.PaxFares.FirstOrDefault().LastName);
        }
    }
}
