using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class CommonRefDBServicesTest
    {
        private readonly Mock<CommonReferenceDataContext> _commonReferenceDataContext;
        private readonly CommonRefDBServices _commonRefDBService;
        private readonly Mock<ILogger<CommonRefDBServices>> _mocklogger;

        public CommonRefDBServicesTest()
        {
            _commonReferenceDataContext = new Mock<CommonReferenceDataContext>();
            _mocklogger = new Mock<ILogger<CommonRefDBServices>>();
            _commonRefDBService = new CommonRefDBServices(_commonReferenceDataContext.Object, _mocklogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommonRefDBServices(null, _mocklogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new CommonRefDBServices(_commonReferenceDataContext.Object, null));
        }
    }
}
