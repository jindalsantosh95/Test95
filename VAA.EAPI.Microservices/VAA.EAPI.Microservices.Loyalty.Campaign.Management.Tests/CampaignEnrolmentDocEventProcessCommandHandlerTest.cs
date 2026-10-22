using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignEnrolmentDoc;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignEnrolmentDocEventProcessCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly Mock<ILogger<CampaignEnrolmentDocEventProcessCommandHandler>> _logger;
        private readonly CampaignEnrolmentDocEventProcessCommandHandler _handler;

        public CampaignEnrolmentDocEventProcessCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _logger = new Mock<ILogger<CampaignEnrolmentDocEventProcessCommandHandler>>();
            _handler = new CampaignEnrolmentDocEventProcessCommandHandler(_mediator.Object, _logger.Object, _commonRefDBRepository.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignEnrolmentDocEventProcessCommandHandler(_mediator.Object, null, _commonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignEnrolmentDocEventProcessCommandHandler(null, _logger.Object, _commonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignEnrolmentDocEventProcessCommandHandler(_mediator.Object, _logger.Object, null));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            var request = TestDataProvider.GetTestData<CampaignEnrolmentDocEventProcessCommand>("CampaignManagementMS\\CampaignEnrolmentDocCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignEnrolmentDocEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetPromotionId("1-3W-233")).ReturnsAsync("1-2OHIEB");
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void Handler_Success_With_Error()
        {
            var request = TestDataProvider.GetTestData<CampaignEnrolmentDocEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignEnrolmentDocEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetPromotionId("1-2NGEETY")).ReturnsAsync("1-2OHIEB");
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            var request = TestDataProvider.GetTestData<CampaignEnrolmentDocEventProcessCommand>("CampaignManagementMS\\CampaignEnrolmentDocCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignEnrolmentDocEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetPromotionId("1-3W-233")).ReturnsAsync("1-2OHIEB");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None));
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

    }
}
