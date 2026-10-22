using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignCommandResponseQueryHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignCommandResponseQueryHandler>> _logger;
        private readonly CampaignCommandResponseQueryHandler _handler;
        public CampaignCommandResponseQueryHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignCommandResponseQueryHandler>>();
            _handler = new CampaignCommandResponseQueryHandler(_mediator.Object, _logger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ResponseDelayTimeInMS, "0");
            Environment.SetEnvironmentVariable(Constants.Environment.ResponsePollingCount, "1");

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignCommandResponseQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new CampaignCommandResponseQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void Handler_Faliure_With_Error()
        {
            CampaignCommandResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignCommandResponseQuery>("CampaignManagementMS\\CampaignUnassignResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Error });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            CampaignCommandResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignCommandResponseQuery>("CampaignManagementMS\\CampaignUnassignResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Processing });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            CampaignCommandResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignCommandResponseQuery>("CampaignManagementMS\\CampaignUnassignResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}
