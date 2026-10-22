using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class TargetedMembershipDocCreateCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly Mock<ILogger<TargetedMembershipDocCreateCommandHandler>> _logger;
        private readonly TargetedMembershipDocCreateCommandHandler _handler;
        public TargetedMembershipDocCreateCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _logger = new Mock<ILogger<TargetedMembershipDocCreateCommandHandler>>();
            _handler = new TargetedMembershipDocCreateCommandHandler(_mediator.Object, _commonRefDBRepository.Object, _logger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDocCreateCommandHandler(_mediator.Object, _commonRefDBRepository.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDocCreateCommandHandler(null, _commonRefDBRepository.Object, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDocCreateCommandHandler(_mediator.Object, null, _logger.Object));
        }


        [Fact]
        public void Handler_Success_With_No_Error()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDocCreateCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDocCreateCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-3W-233")).ReturnsAsync(campaignData);
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithNullCampaignId()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDocCreateCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            request.Data = request.Data.ToString().Replace("1-3W-233", "");
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDocCreateCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-3W-233")).ReturnsAsync(campaignData);
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDocCreateCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDocCreateCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-3W-233")).ReturnsAsync(campaignData);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}

