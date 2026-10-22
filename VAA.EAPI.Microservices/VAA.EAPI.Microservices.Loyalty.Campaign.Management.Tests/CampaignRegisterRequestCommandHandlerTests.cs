using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignRegisterRequestCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly Mock<ILogger<CampaignRegisterRequestCommandHandler>> _logger;
        private readonly CampaignRegisterRequestCommandHandler _handler;
        public CampaignRegisterRequestCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _logger = new Mock<ILogger<CampaignRegisterRequestCommandHandler>>();
            _handler = new CampaignRegisterRequestCommandHandler(_mediator.Object, _commonRefDBRepository.Object, _logger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignRegisterRequestCommandHandler(_mediator.Object, _commonRefDBRepository.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignRegisterRequestCommandHandler(null, _commonRefDBRepository.Object, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignRegisterRequestCommandHandler(_mediator.Object, null, _logger.Object));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignRegisterRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-2NGEETY")).ReturnsAsync(campaignData);
            var result = _handler.Handle(new CampaignRegisterRequestCommand() { CustomerId = "c220a7fb-8b85-4f4f-bac8-db94252a3e45", CampaignId = "1-2NGEETY", CampaignCode = "PPAL20", DocumentId = "adfr" }, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void Handler_Success_With_InvalidCampaignCode_Error()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignRegisterRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-2NGEETY")).ReturnsAsync(campaignData);
            var result = _handler.Handle(new CampaignRegisterRequestCommand() { CustomerId = "c220a7fb-8b85-4f4f-bac8-db94252a3e45", CampaignId = "1-2NGEETY", CampaignCode = "PPAL20", DocumentId = "adfr" }, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
        }

        [Fact]
        public void Handler_Success_With_MandatoryFieldMissing_Error()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignRegisterRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-2NGEETY")).ReturnsAsync(campaignData);
            var result = _handler.Handle(new CampaignRegisterRequestCommand() { CampaignCode = "PPAL20", DocumentId = "adfr" }, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithNullCampaignData()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            campaignData = null;
            _mediator.Setup(m => m.Send(It.IsAny<CampaignRegisterRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-2NGEETY")).ReturnsAsync(campaignData);
            var result = _handler.Handle(new CampaignRegisterRequestCommand() { CustomerId = "c220a7fb-8b85-4f4f-bac8-db94252a3e45", CampaignId = "1-2NGEETY", CampaignCode = "PPAL20", DocumentId = "adfr" }, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignRegisterRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            _commonRefDBRepository.Setup(m => m.GetCampaignDetails("1-2NGEETY")).ReturnsAsync(campaignData);
            var result = _handler.Handle(new CampaignRegisterRequestCommand() { CustomerId = "c220a7fb-8b85-4f4f-bac8-db94252a3e45", CampaignId = "1-2NGEETY", CampaignCode = "PPAL20", DocumentId = "adfr" }, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}