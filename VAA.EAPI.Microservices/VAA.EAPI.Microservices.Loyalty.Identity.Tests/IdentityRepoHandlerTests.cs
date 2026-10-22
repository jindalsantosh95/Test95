using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class IdentityRepoHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<IdentityRepoHandler>> _mockIdentityRepoLogger;

        public IdentityRepoHandlerTests()
        {
            _mockIdentityRepoLogger = new Mock<ILogger<IdentityRepoHandler>>();
            _mockMediator = new Mock<IMediator>();
            ConfigReader.Set("CustomMemberStatusAttribute", "dummyValue1");
            ConfigReader.Set("CustomMemberTypeAttribute", "dummyValue2");
            ConfigReader.Set("CustomSecurityAnswer1Attribute", "dummyValue3");
            ConfigReader.Set("CustomSecurityAnswer2Attribute", "dummyValue4");
            ConfigReader.Set("CustomSecurityQuestion1Attribute", "dummyValue5");
            ConfigReader.Set("CustomSecurityQuestion2Attribute", "dummyValue6");
            ConfigReader.Set("CustomCustomerIdAttribute", "dummyValue7");
            ConfigReader.Set("ExtensionPrefix", "dummyValue8");
            ConfigReader.Set("AccountEnabled", "dummyValue9");
            ConfigReader.Set("DisplayName", "dummyValue10");
            ConfigReader.Set("GivenName", "dummyValue11");
            ConfigReader.Set("Surname", "dummyValue12");
            ConfigReader.Set("SignInNames", "dummyValue13");
            ConfigReader.Set("PasswordProfile", "dummyValue14");
            ConfigReader.Set("userPrincipalName", "dummyValue16");
            ConfigReader.Set("mailNickname", "dummyValue17");
            ConfigReader.Set("mail", "dummyValue18");
            ConfigReader.Set("B2CDomain", "dummy");
            ConfigReader.Set("Identities", "dummyIdentity");
        }

        [Fact]
        public void NullLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new IdentityRepoHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void NullMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new IdentityRepoHandler(null, _mockIdentityRepoLogger.Object));
        }

        [Fact]
        public async Task FailedRequest_ShouldReturnFalse()
        {

            // Arrange
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

            IdentityRepoHandler handler = new IdentityRepoHandler(mockMediator.Object, _mockIdentityRepoLogger.Object);
            IdentityRepoCommand request = new IdentityRepoCommand(new CreateCustomerEntities()
            );

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SucceededRequest_ShouldReturnTrue()
        {

            // Arrange
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

            IdentityRepoHandler handler = new IdentityRepoHandler(mockMediator.Object, _mockIdentityRepoLogger.Object);
            IdentityRepoCommand request = new IdentityRepoCommand(new CreateCustomerEntities()
            );

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result);
        }
    }
}
