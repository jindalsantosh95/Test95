using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionsDownload;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionDownloadQueryHandlerTest
    {
        private readonly Mock<ICommonRefDBServices> _commonRefDBServices;
        private readonly Mock<IPromotionListDownloadService> _promotionListDownloadService;
        private readonly Mock<ILogger<PromotionDownloadQueryHandler>> _mockLogger;
        private readonly PromotionDownloadQueryHandler _handler;

        public PromotionDownloadQueryHandlerTest()
        {
            _mockLogger = new Mock<ILogger<PromotionDownloadQueryHandler>>();
            _commonRefDBServices = new Mock<ICommonRefDBServices>();
            _promotionListDownloadService = new Mock<IPromotionListDownloadService>();
            _handler = new PromotionDownloadQueryHandler(_commonRefDBServices.Object, _promotionListDownloadService.Object, _mockLogger.Object);
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionDownloadQueryHandler(_commonRefDBServices.Object, _promotionListDownloadService.Object, null));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenReportingDBServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionDownloadQueryHandler(null, _promotionListDownloadService.Object, _mockLogger.Object));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenPromotionEnrolledMemberServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionDownloadQueryHandler(_commonRefDBServices.Object, null, _mockLogger.Object));
        }

        [Fact]
        public async Task HandlerShouldSuccess()
        {
            var response = new PromotionsDownloadResponse() { PromotionsFileURL = "dummyurl" };
            var promoDetails = new List<PromotionsDownload>() { new PromotionsDownload() { PromotionId = "123", PromotionName = "test" } };
            _commonRefDBServices.Setup(x => x.GetPromotionslist()).ReturnsAsync(promoDetails);
            _promotionListDownloadService.Setup(m => m.CreateXlStatementAndUploadToBlob(promoDetails)).ReturnsAsync(new Response<string> { });
            PromotionDownloadQuery query = new() { };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldFail()
        {
            var response = new PromotionsDownloadResponse() { PromotionsFileURL = "dummyurl" };
            var promoDetails = new List<PromotionsDownload>() { new PromotionsDownload() { PromotionId = "123", PromotionName = "test" } };
            _commonRefDBServices.Setup(x => x.GetPromotionslist()).ReturnsAsync(promoDetails);
            _promotionListDownloadService.Setup(m => m.CreateXlStatementAndUploadToBlob(promoDetails)).Throws(new Exception());
            PromotionDownloadQuery query = new() { };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
        }
    }
}
