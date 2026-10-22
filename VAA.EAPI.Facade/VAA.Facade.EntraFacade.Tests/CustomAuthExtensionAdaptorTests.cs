using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.Facade.EntraFacade.Application.Command;
using VAA.Facade.EntraFacade.Application.Interfaces;
using VAA.Facade.EntraFacade.Domain;
using Xunit;

namespace VAA.Facade.EntraFacade.Tests
{
    public class CustomAuthExtensionAdaptorTests
    {
        private readonly Mock<IOAuthTokenProvider> _mockTokenProvider;
        private readonly Mock<IHttpConnector> _mockHttpConnector;
        private readonly CustomAuthExtensionAdaptor _adaptor;

        public CustomAuthExtensionAdaptorTests()
        {
            _mockTokenProvider = new Mock<IOAuthTokenProvider>();
            _mockHttpConnector = new Mock<IHttpConnector>();
            _adaptor = new CustomAuthExtensionAdaptor(_mockTokenProvider.Object, _mockHttpConnector.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenTokenProviderIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CustomAuthExtensionAdaptor(null, _mockHttpConnector.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpConnectorIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CustomAuthExtensionAdaptor(_mockTokenProvider.Object, null));
        }

        [Fact]
        public async Task CustomEmailOtp_ShouldReturnSuccessResponse_WhenValidRequestProvided()
        {
            // Arrange
            var entraRequest = new EntraRequest();
            var expectedToken = "test-token";
            string dataType = "microsoft.graph.OnOtpSendResponseData";
                            ;
            var expectedResponse = new ResponseObject(dataType)
            {
                Data = new Data(dataType)
                {
                    Actions = new List<ResponseAction>()
                    {
                        new ResponseAction("microsoft.graph.OtpSend.continueWithDefaultBehavior", new ClaimsResponse())
                        {
                        }
                    }
                }
            };


            var cancellationToken = CancellationToken.None;

            _mockTokenProvider
                .Setup(x => x.AcquireTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedToken);

            _mockHttpConnector.Setup(m => m.SendAsync<ResponseObject>(It.IsAny<string>(),
                                             It.IsAny<Dictionary<string, string>>(),
                                             It.IsAny<string>(),
                                             It.IsAny<HttpMethod>(),
                                             It.IsAny<string>(),
                                             It.IsAny<bool>())).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _adaptor.CustomEmailOtp(entraRequest, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);
            Assert.Equal(expectedResponse, result.Value);
            Assert.Null(result.Errors);
        }

        [Fact]
        public async Task CustomToken_ShouldReturnSuccessResponse_WhenValidRequestProvided()
        {
            // Arrange
            var entraRequest = new EntraRequest();
            var expectedToken = "test-token";
            string dataType = "microsoft.graph.onTokenIssuanceStartResponseData";
            var claims = new ClaimsResponse()
                                {
                                    MemberNumber = "1562790",
                                    MemberStatus = "ACTIVE"
                                }
                            ;

            var expectedResponse = new ResponseObject(dataType)
            {
                Data = new Data(dataType)
                {
                    Actions = new List<ResponseAction>()
                    {
                        new ResponseAction("microsoft.graph.tokenIssuanceStart.provideClaimsForToken", claims)
                        {
                        }
                    }
                }
            };

            var cancellationToken = CancellationToken.None;

            _mockTokenProvider
                .Setup(x => x.AcquireTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedToken);

            _mockHttpConnector.Setup(m => m.SendAsync<ResponseObject>(It.IsAny<string>(),
                                         It.IsAny<Dictionary<string, string>>(),
                                         It.IsAny<string>(),
                                         It.IsAny<HttpMethod>(),
                                         It.IsAny<string>(),
                                         It.IsAny<bool>())).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _adaptor.CustomToken(entraRequest, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);
            Assert.Equal(expectedResponse, result.Value);
            Assert.Null(result.Errors);
        }
    }
}