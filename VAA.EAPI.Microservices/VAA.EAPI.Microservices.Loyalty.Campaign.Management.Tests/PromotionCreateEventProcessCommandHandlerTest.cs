using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionCreateEventProcessCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<PromotionCreateEventProcessCommandHandler>> _mockLogger;
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly PromotionCreateEventProcessCommandHandler _handler;

        public PromotionCreateEventProcessCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<PromotionCreateEventProcessCommandHandler>>();
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _handler = new PromotionCreateEventProcessCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCommonRefDBRepository.Object);
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionCreateEventProcessCommandHandler(_mockMediator.Object, null, _mockCommonRefDBRepository.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new PromotionCreateEventProcessCommandHandler(null, _mockLogger.Object, _mockCommonRefDBRepository.Object));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            var promotionCreateRequestCommand = TestDataProvider.GetTestData<PromotionCreateEventProcessCommand>("CampaignManagementMS\\PromotionCreateEventProcessCommand");
            _mockMediator.Setup(x => x.Send(It.IsAny<PromotionCreateCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var result = _handler.Handle(promotionCreateRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerFailureWithCreateError()
        {
            var promotionCreateRequestCommand = TestDataProvider.GetTestData<PromotionCreateEventProcessCommand>("CampaignManagementMS\\PromotionCreateEventProcessCommand");
            _mockMediator.Setup(x => x.Send(It.IsAny<PromotionCreateCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Failure" });
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var result = _handler.Handle(promotionCreateRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerFailureWithTechnicalError()
        {
            var promotionCreateRequestCommand = TestDataProvider.GetTestData<PromotionCreateEventProcessCommand>("CampaignManagementMS\\PromotionCreateEventProcessCommand");
            _mockMediator.Setup(x => x.Send(It.IsAny<PromotionCreateCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(promotionCreateRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

    }
}
