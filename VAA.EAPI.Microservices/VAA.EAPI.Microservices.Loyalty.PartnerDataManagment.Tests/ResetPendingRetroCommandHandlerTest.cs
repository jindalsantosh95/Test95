using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class ResetPendingRetroCommandHandlerTest
    {
        private readonly Mock<ILogger<ResetPendingRetroCommandHandler>> _mockLogger;
        private readonly ResetPendingRetroCommandHandler _handler;
        private readonly Mock<IFlownDbRepository> _flownDbRepository;

        public ResetPendingRetroCommandHandlerTest()
        {
            PDMTestHelpers.CreateTestEnvironmentSettings();
            _mockLogger = new Mock<ILogger<ResetPendingRetroCommandHandler>>();
            _flownDbRepository = new Mock<IFlownDbRepository>();
            _handler = new ResetPendingRetroCommandHandler(_flownDbRepository.Object, _mockLogger.Object);
        }
       
        [Fact]

        public async Task HandlerShouldThrowExceptionForNullResetPendingRetroCommand()
        {
            ResetPendingRetroCommand command = null;

            await Assert.ThrowsAsync<NullReferenceException>(async () => await _handler.Handle(command, CancellationToken.None));
        }

        [Fact]

        public async Task HandlerShouldThrowExceptionForNullResetPendingRetroRequest()
        {
            ResetPendingRetroCommand command = new ResetPendingRetroCommand()
            {
                Days = null,
            };

            _flownDbRepository.Setup(m => m.ResetPendingFeedbackAutoRetroAsync(command.Days)).Throws(new NullReferenceException());

            await Assert.ThrowsAsync<NullReferenceException>(async () => await _handler.Handle(command, CancellationToken.None));
        }

        [Fact]

        public async Task HandlerShouldReturnSuccessResetPendingFeedbackAutoRetro()
        {
            ResetPendingRetroCommand command = new ResetPendingRetroCommand()
            {
                Days = "7",
            };
            
            _flownDbRepository.Setup(m => m.ResetPendingFeedbackAutoRetroAsync(command.Days)).ReturnsAsync(true);
           
            var result = await _handler.Handle(command, CancellationToken.None);
            
            Assert.NotNull(result);
            Assert.Equal(result, Constants.Application.ResetPendingFeedBackAutoRetroProcessSuccessMessage);
        }
    }
}
