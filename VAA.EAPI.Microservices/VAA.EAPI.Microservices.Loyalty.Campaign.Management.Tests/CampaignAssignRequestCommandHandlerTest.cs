using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignAssignRequestCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly Mock<ILogger<CampaignAssignRequestCommandHandler>> _logger;
        private readonly CampaignAssignRequestCommandHandler _handler;
        public CampaignAssignRequestCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _logger = new Mock<ILogger<CampaignAssignRequestCommandHandler>>();
            _handler = new CampaignAssignRequestCommandHandler(_mediator.Object, _commonRefDBRepository.Object, _logger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignAssignRequestCommandHandler(_mediator.Object, _commonRefDBRepository.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignAssignRequestCommandHandler(null, _commonRefDBRepository.Object, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignAssignRequestCommandHandler(_mediator.Object, null, _logger.Object));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignRequestCommand request = TestDataProvider.GetTestData<CampaignAssignRequestCommand>("CampaignManagementMS\\CampaignAssignValidRequest");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-2NGEETY")).ReturnsAsync(campaignData);
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }


        [Fact]
        public void HandlerSuccessWithErrorWhenCampaignDataIsNull()
        {
            CampaignDetails campaignData = null;
            CampaignAssignRequestCommand request = TestDataProvider.GetTestData<CampaignAssignRequestCommand>("CampaignManagementMS\\CampaignAssignValidRequest");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-2NGEETY")).ReturnsAsync(campaignData);
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }


        [Fact]
        public void HandlerFailWithcloudEventException()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignRequestCommand request = TestDataProvider.GetTestData<CampaignAssignRequestCommand>("CampaignManagementMS\\CampaignAssignValidRequest");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-2NGEETY")).ReturnsAsync(campaignData);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None));
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerSuccessWithErrorWhenValidationFails()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignRequestCommand request = TestDataProvider.GetTestData<CampaignAssignRequestCommand>("CampaignManagementMS\\CampaignAssignValidRequest");
            request.CampaignCode = null;
            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-2NGEETY")).ReturnsAsync(campaignData);
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }
    }
}
