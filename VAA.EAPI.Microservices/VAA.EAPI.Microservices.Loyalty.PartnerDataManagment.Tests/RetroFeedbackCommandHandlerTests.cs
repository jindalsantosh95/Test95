using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class RetroFeedbackCommandHandlerTest
    {
      
        private readonly RetroFeedbackCommandHandler _handler;
        private readonly Mock<IFlownDbRepository> _flownDbRepository;
        private readonly Mock<ILogger<RetroFeedbackCommandHandler>> _mockLogger;

        public RetroFeedbackCommandHandlerTest()
        {
            PDMTestHelpers.CreateTestEnvironmentSettings();
            _flownDbRepository = new Mock<IFlownDbRepository>();
            _mockLogger = new Mock<ILogger<RetroFeedbackCommandHandler>>();
            _handler = new RetroFeedbackCommandHandler(_flownDbRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task HandlerShouldThrowExceptionForNullRetroFeedbackCommand()
        {
            RetroFeedbackCommand command = null;

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result.Errors);
            Assert.Equal(Status.Error, result.Status);
            Assert.IsType<ErrorDetails>(result.Errors.First());
            Assert.Equal("Technical issue encountered", result.Errors.First().Message);
        }

        [Fact]
        public async Task HandlerShouldGiveErrorStatusForNullRetroFeedbackRequest()
        {
            RetroFeedbackCommand command = new RetroFeedbackCommand()
            {
                RetroFeedbackRequest = null,
            };
            
            var result = await _handler.Handle(command, CancellationToken.None);
            
            Assert.Equal(Status.Error, result.Status);
        }

        [Fact]
        public async Task HandlerShouldValidateRetroFeedbackRequest_EDC997()
        {
            var request = TestDataProvider.GetTestData<RetroFeedbackRequest>("PartnerDataManagmentMS\\OnlineRetroCreditFeedbackRequest");
            request.UniqueIdentificationNumber =string.Empty;
            var creditCommand = new RetroFeedbackCommand()
            {
                RetroFeedbackRequest = request
            };

            var result = await _handler.Handle(creditCommand, CancellationToken.None);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC997);
        }

        [Fact]
        public async Task HandlerShouldReturnSuccessForValidRetroFeedback_EDC999()
        {
            var request = TestDataProvider.GetTestData<RetroFeedbackRequest>("PartnerDataManagmentMS\\OnlineRetroCreditFeedbackRequest");
            var creditCommand = new RetroFeedbackCommand()
            {
                RetroFeedbackRequest = request
            };
            
            _flownDbRepository.Setup(m => m.UpdateFlownDetailsWithRetroFeedbackAsync(It.IsAny<RetroFeedbackRequest>())).ReturnsAsync(true);
            var result = await _handler.Handle(creditCommand, CancellationToken.None);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC999);
        }

        [Fact]
        public async Task HandlerShouldReturnNotFoundEDCForWrongRetroFeedback_EDC107()
        {
            var request = TestDataProvider.GetTestData<RetroFeedbackRequest>("PartnerDataManagmentMS\\OnlineRetroCreditFeedbackRequest");
            var creditCommand = new RetroFeedbackCommand()
            {
                RetroFeedbackRequest = request
            };
            
            _flownDbRepository.Setup(m => m.UpdateFlownDetailsWithRetroFeedbackAsync(It.IsAny<RetroFeedbackRequest>())).ReturnsAsync(false);
            
            var result = await _handler.Handle(creditCommand, CancellationToken.None);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC107);
        }
    }
}
