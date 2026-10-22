using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignCreateResponseQueryHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignCreateResponseQueryHandler>> _logger;
        private readonly CampaignCreateResponseQueryHandler _handler;
        public CampaignCreateResponseQueryHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignCreateResponseQueryHandler>>();
            _handler = new CampaignCreateResponseQueryHandler(_mediator.Object, _logger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ResponseDelayTimeInMS, "0");
            Environment.SetEnvironmentVariable(Constants.Environment.ResponsePollingCount, "1");

            CampaignManagementHelpers.CreateTestEnvironmentSettings();

        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignCreateResponseQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new CampaignCreateResponseQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            CampaignCreateResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignCreateResponseQuery>("CampaignManagementMS\\CampaignCreateResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, JsonResponseData = JsonConvert.SerializeObject(new { value = "success" }) });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithError()
        {
            CampaignCreateResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignCreateResponseQuery>("CampaignManagementMS\\CampaignCreateResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Error });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessForPendingstatus()
        {
            CampaignCreateResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignCreateResponseQuery>("CampaignManagementMS\\CampaignCreateResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Processing, JsonResponseData = JsonConvert.SerializeObject(new { value = "success" }) });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            CampaignCreateResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignCreateResponseQuery>("CampaignManagementMS\\CampaignCreateResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}
