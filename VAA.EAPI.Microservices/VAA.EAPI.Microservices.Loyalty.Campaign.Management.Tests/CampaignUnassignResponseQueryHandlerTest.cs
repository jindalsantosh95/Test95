using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignUnassignResponseQueryHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignUnassignResponseQueryHandler>> _logger;
        private readonly CampaignUnassignResponseQueryHandler _handler;
        public CampaignUnassignResponseQueryHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignUnassignResponseQueryHandler>>();
            _handler = new CampaignUnassignResponseQueryHandler(_mediator.Object, _logger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ResponseDelayTimeInMS, "0");
            Environment.SetEnvironmentVariable(Constants.Environment.ResponsePollingCount, "1");
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUnassignResponseQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new CampaignUnassignResponseQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void Handler_Faliure_With_Error()
        {
            CampaignUnassignResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignUnassignResponseQuery>("CampaignManagementMS\\CampaignUnassignResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Error });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            CampaignUnassignResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignUnassignResponseQuery>("CampaignManagementMS\\CampaignUnassignResponseQuery");
            var data = new CampaignUnassignServiceResponseData()
            {
                Value = new CampaignUnassignStatus()
                {
                    Status = Status.OK,
                    Errors = new List<Error>() { }
                }
            };
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, JsonResponseData = Json.Serialize(data) });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithCampaignUnassignStatusError()
        {
            CampaignUnassignResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignUnassignResponseQuery>("CampaignManagementMS\\CampaignUnassignResponseQuery");
            var data = new CampaignUnassignServiceResponseData()
            {
                Value = new CampaignUnassignStatus()
                {
                    Status = Status.Error,
                    Errors = new List<Error>() { new Error() { Message = "Failure" } }
                }
            };
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, JsonResponseData = Json.Serialize(data) });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessForPendingstatus()
        {
            CampaignUnassignResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignUnassignResponseQuery>("CampaignManagementMS\\CampaignUnassignResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Processing, JsonResponseData = JsonConvert.SerializeObject(new { value = "success" }) });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            CampaignUnassignResponseQuery campaignQueryData = TestDataProvider.GetTestData<CampaignUnassignResponseQuery>("CampaignManagementMS\\CampaignUnassignResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}