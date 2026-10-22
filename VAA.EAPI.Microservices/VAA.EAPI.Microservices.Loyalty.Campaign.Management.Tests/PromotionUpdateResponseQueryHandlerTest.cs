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
    public class PromotionUpdateResponseQueryHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<PromotionUpdateResponseQueryHandler>> _logger;
        private readonly PromotionUpdateResponseQueryHandler _handler;
        public PromotionUpdateResponseQueryHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<PromotionUpdateResponseQueryHandler>>();
            _handler = new PromotionUpdateResponseQueryHandler(_mediator.Object, _logger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ResponseDelayTimeInMS, "0");
            Environment.SetEnvironmentVariable(Constants.Environment.ResponsePollingCount, "1");
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionUpdateResponseQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new PromotionUpdateResponseQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void Handler_Success_With_NoError()
        {
            PromotionUpdateResponseQuery promotionQueryData = TestDataProvider.GetTestData<PromotionUpdateResponseQuery>("CampaignManagementMS\\PromotionUpdateResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, JsonResponseData = JsonConvert.SerializeObject(new { value = "success" }) });
            var result = _handler.Handle(promotionQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        /*[Fact]
        public void HandlerSuccessProcessingStatusWithNoError()
        {
            PromotionUpdateResponseQuery promotionQueryData = TestDataProvider.GetTestData<PromotionUpdateResponseQuery>("CampaignManagementMS\\PromotionUpdateResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Processing });
            var result = _handler.Handle(promotionQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }*/

        [Fact]
        public void HandlerFailureWithException()
        {
            PromotionUpdateResponseQuery promotionQueryData = TestDataProvider.GetTestData<PromotionUpdateResponseQuery>("CampaignManagementMS\\PromotionUpdateResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(promotionQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            PromotionUpdateResponseQuery promotionQueryData = TestDataProvider.GetTestData<PromotionUpdateResponseQuery>("CampaignManagementMS\\PromotionUpdateResponseQuery");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Error });
            var result = _handler.Handle(promotionQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }


    }
}
