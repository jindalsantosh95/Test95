using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Tests
{
    public class CreateSessionCommandHandlerTests
    {
        private readonly Mock<ICreateSession> _createSession;
        private readonly CreateSessionCommandHandler _handler;

        public CreateSessionCommandHandlerTests()
        {
            _createSession = new Mock<ICreateSession>();
            _handler = new CreateSessionCommandHandler(_createSession.Object);

            ConfigReader.Set("SabreUsername", "123");
            ConfigReader.Set("SabrePassword", "ABC");
            ConfigReader.Set("FromPartyId", "1");
            ConfigReader.Set("ToPartyId", "2");
            ConfigReader.Set("PCC", "123");
            ConfigReader.Set("ConversationId", "123");
            ConfigReader.Set("Domain", "xyz");
            ConfigReader.Set("keyvaulturi", "https://tempuri.org");

        }

        [Fact]
        public void HandlerShouldThrowException_WhenCreateSessionObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CreateSessionCommandHandler(null));
        }

        [Fact]
        public async Task HandlerShouldReturnToken_WithSuccessfulServiceCall()
        {
            //Arrange
            _createSession.Setup(x => x.CreateSessionAsync(It.IsAny<SessionCreateRQRequest>())).ReturnsAsync(new SessionCreateRQResponse()
            {
                Security = new() { BinarySecurityToken = new() { Value = "dummyToken" } },
                SessionCreateRS = new() { status = "Approved" }
            });

            //Act
            var result = await _handler.Handle(new CreateSessionCommand(), new CancellationToken());

            //Assert
            Assert.Equal("dummyToken", result);
        }

        [Fact]
        public async Task HandlerShouldReturnNull_WithSuccessfulServiceCallWithDiffStatus()
        {
            //Arrange
            _createSession.Setup(x => x.CreateSessionAsync(It.IsAny<SessionCreateRQRequest>())).ReturnsAsync(new SessionCreateRQResponse()
            {
                SessionCreateRS = new() { status = "Incomplete" }
            });

            //Act
            var result = await _handler.Handle(new CreateSessionCommand(), new CancellationToken());

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task HandlerShouldReturnNull_WithUnsuccessfulServiceCall()
        {
            //Arrange
            _createSession.Setup(x => x.CreateSessionAsync(It.IsAny<SessionCreateRQRequest>())).ReturnsAsync(new SessionCreateRQResponse());

            //Act
            var result = await _handler.Handle(new CreateSessionCommand(), new CancellationToken());

            //Assert
            Assert.Null(result);
        }
    }
}