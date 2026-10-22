using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionCreateResponseQueryHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<PromotionCreateResponseQueryHandler>> _logger;
        private readonly PromotionCreateResponseQueryHandler _handler;
        public PromotionCreateResponseQueryHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<PromotionCreateResponseQueryHandler>>();
            _handler = new PromotionCreateResponseQueryHandler(_mediator.Object, _logger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ResponseDelayTimeInMS, "0");
            Environment.SetEnvironmentVariable(Constants.Environment.ResponsePollingCount, "1");

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionCreateResponseQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new PromotionCreateResponseQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            PromotionCreateResponseQuery promotionQueryData = new PromotionCreateResponseQuery() { CorrelationId = "10043534654qwe", Type = "test" };
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Error });
            var result = _handler.Handle(promotionQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        /*[Fact]
        public void HandlerSuccessProcessingStatusWithNoError()
        {
            PromotionCreateResponseQuery promotionQueryData = new PromotionCreateResponseQuery() { CorrelationId = "10043534654qwe", Type = "test" };
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Processing });
            var result = _handler.Handle(promotionQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }*/

        [Fact]
        public void HandlerFailureWithException()
        {
            PromotionCreateResponseQuery promotionQueryData = new PromotionCreateResponseQuery() { CorrelationId = "10043534654qwe", Type = "test" };
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(promotionQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerSuccessCompletedStatusWithNoError()
        {
            PromotionCreateResponseQuery promotionQueryData = new PromotionCreateResponseQuery() { CorrelationId = "10043534654qwe", Type = "test" };
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, JsonResponseData = JsonConvert.SerializeObject(new { value = "success" }) });
            var result = _handler.Handle(promotionQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
        }
    }
}
