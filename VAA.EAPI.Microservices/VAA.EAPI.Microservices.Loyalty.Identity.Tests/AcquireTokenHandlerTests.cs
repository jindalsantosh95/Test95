using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Security;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class AcquireTokenHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<AcquireTokenHandler>> _logger;
        private readonly Mock<ITokenService<Claim>> _tokenService;
        private readonly AcquireTokenHandler _handler;

        public AcquireTokenHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<AcquireTokenHandler>>();
            _tokenService = new Mock<ITokenService<Claim>>();
            _handler = new AcquireTokenHandler(_mediator.Object, _logger.Object, _tokenService.Object);
            IdentityTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AcquireTokenHandler(_mediator.Object, null, _tokenService.Object));
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AcquireTokenHandler(null, _logger.Object, _tokenService.Object));
        }

        [Fact]
        public void EmptyService_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AcquireTokenHandler(_mediator.Object, _logger.Object, null));
        }

        [Fact]
        public async Task ShouldReturnToken_WithValidPNRRequest()
        {
            //Arrange
            var serviceResponse = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(serviceResponse);
            _tokenService.Setup(x => x.GenerateAccessToken(It.IsAny<List<Claim>>())).Returns("dummyToken");

            //Act
            var result = await _handler.Handle(new AcquireTokenRequest() { Type = "PNR", RLOC = "CTQV8H", FirstName = "FIRSTNAME1", LastName = "LASTNAME1" }, CancellationToken.None);

            //Assert
            Assert.Equal("Ok", result.Value.Value);
        }

        [Fact]
        public async Task ShouldReturnFailure_WithExceptionOccurs()
        {
            //Arrange
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).Throws<ArgumentNullException>();
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task ShouldReturnToken_WithValidTicketRequest()
        {
            //Arrange
            var serviceResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\Air4TicketingProcessRetrieveResponse");
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(serviceResponse);
            _tokenService.Setup(x => x.GenerateAccessToken(It.IsAny<List<Claim>>())).Returns("dummyToken");
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal("Ok", result.Value.Value);
        }

        [Fact]
        public async Task ShouldReturnFailure_WhenExceptionOccurs()
        {
            //Arrange
            _mediator.Setup(x => x.Send(It.IsAny<TicketingRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).Throws<ArgumentNullException>();
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task ShouldReturnError_WhenFirstNameIsNull()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");
            request.FirstName = null;

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.FirstNameMissing, result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldReturnError_WhenLastNameIsNull()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");
            request.LastName = null;

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.LastNameMissing, result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldReturnError_WhenTypeIsNull()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");
            request.Type = null;

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.TypeMissingOrInvalid, result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldReturnError_WhenTypeIsInvalid()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");
            request.Type = "dummy";

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.TypeMissingOrInvalid, result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldReturnError_WhenTicketLengthIsIncorrect()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");
            request.TicketRef = "123";

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.InvalidETicketNumber, result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldReturnError_WhenPNRLengthIsIncorrect()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");
            request.Type = "PNR";
            request.RLOC = "ABC";

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.InvalidPNR, result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldReturnError_WhenPassengerNameForTicketIsIncorrect()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");
            request.FirstName = "dummy";

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(EAPI.Domain.Entities.Common.Status.Unauthorized, result.Value.Status);
            Assert.Equal(Constants.ErrorCodes.PassengerNameMisMatchForETicket, result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldReturnError_WhenPassengerNameForPNRIsIncorrect()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AcquireTokenRequest>("IdentityMS\\AcquireTokenHandlerRequestForTicket");
            request.Type = "PNR";
            request.LastName = "dummy";

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(EAPI.Domain.Entities.Common.Status.Unauthorized, result.Value.Status);
            Assert.Equal(Constants.ErrorCodes.PassengerNameMisMatchForPNR, result.Value.Errors.First().Code);
        }
    }
}
