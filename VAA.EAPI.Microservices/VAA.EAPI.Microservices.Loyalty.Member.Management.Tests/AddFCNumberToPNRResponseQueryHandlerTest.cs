using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class AddFCNumberToPNRResponseQueryHandlerTest
    {
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly AddFCNumberToPNRResponseQueryHandler _handler;

        public AddFCNumberToPNRResponseQueryHandlerTest()
        {
            _httpConnector = new Mock<IHttpConnector>();
            _handler = new AddFCNumberToPNRResponseQueryHandler(_httpConnector.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenHttpObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddFCNumberToPNRResponseQueryHandler(null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task HandlerSuccessWithError()
        {
            var request = new AddFCNumberToPNRResponseQuery { CorrelationId = Guid.NewGuid().ToString() };
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
