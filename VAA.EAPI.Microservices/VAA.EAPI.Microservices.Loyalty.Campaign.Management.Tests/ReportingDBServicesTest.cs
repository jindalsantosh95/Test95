using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class ReportingDBServicesTest
    {
        private readonly Mock<ReportingDataContext> _reportingDataContext;
        private readonly Mock<ILogger<ReportingDBServices>> _mocklogger;
        private readonly ReportingDBServices _reportingDBServices;
        public ReportingDBServicesTest()
        {
            _reportingDataContext = new Mock<ReportingDataContext>();
            _mocklogger = new Mock<ILogger<ReportingDBServices>>();
            _reportingDBServices = new ReportingDBServices(_reportingDataContext.Object, _mocklogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenDataContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ReportingDBServices(null, _mocklogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ReportingDBServices(_reportingDataContext.Object, null));
        }

        [Fact]
        public async Task GetAssociatedMemberCountWithError()
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => _reportingDBServices.GetAssociatedMemberCount("XYZ"));
        }
    }
}
