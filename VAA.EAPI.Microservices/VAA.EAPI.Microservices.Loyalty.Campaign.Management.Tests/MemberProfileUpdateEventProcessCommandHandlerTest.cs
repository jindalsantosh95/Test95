using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.MemberProfileUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class MemberProfileUpdateEventProcessCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly Mock<ILogger<MemberProfileUpdateEventProcessCommandHandler>> _logger;
        private readonly MemberProfileUpdateEventProcessCommandHandler _handler;

        public MemberProfileUpdateEventProcessCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _logger = new Mock<ILogger<MemberProfileUpdateEventProcessCommandHandler>>();
            _handler = new MemberProfileUpdateEventProcessCommandHandler(_mediator.Object, _commonRefDBRepository.Object, _logger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberProfileUpdateEventProcessCommandHandler(_mediator.Object, _commonRefDBRepository.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberProfileUpdateEventProcessCommandHandler(null, _commonRefDBRepository.Object, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberProfileUpdateEventProcessCommandHandler(_mediator.Object, null, _logger.Object));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            var request = TestDataProvider.GetTestData<MemberProfileUpdateEventProcessCommand>("CampaignManagementMS\\CampaignRegisterProfileUpdateCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<MemberProfileUpdateEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWhenCampaignTypeDoesNotMatch()
        {
            var request = TestDataProvider.GetTestData<MemberProfileUpdateEventProcessCommand>("CampaignManagementMS\\CampaignRegisterProfileUpdateCloudEvent");
            request.Data = request.Data.ToString().Replace("Fast Track to Gold", "");
            _mediator.Setup(m => m.Send(It.IsAny<MemberProfileUpdateEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
        }

        [Fact]
        public void HandlerSuccessWithErrorInvalidDocStatus()
        {
            var request = TestDataProvider.GetTestData<MemberProfileUpdateEventProcessCommand>("CampaignManagementMS\\CampaignRegisterProfileUpdateCloudEvent");
            request.Data = request.Data.ToString().Replace("Registered", "Targeted");
            _mediator.Setup(m => m.Send(It.IsAny<MemberProfileUpdateEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            var request = TestDataProvider.GetTestData<MemberProfileUpdateEventProcessCommand>("CampaignManagementMS\\CampaignRegisterProfileUpdateCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<MemberProfileUpdateEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}


