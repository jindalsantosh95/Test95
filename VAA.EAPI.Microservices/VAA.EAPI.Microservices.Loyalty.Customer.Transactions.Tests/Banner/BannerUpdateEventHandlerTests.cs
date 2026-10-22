using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using System.Threading;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Banner;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Banner;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using CSharpFunctionalExtensions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests.Banner
{
    public class BannerUpdateEventHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();
        private readonly Mock<ILogger<BannerUpdateEventHandler>> _mockLogger = new();
        private readonly BannerUpdateEventHandler _handler;

        public BannerUpdateEventHandlerTests()
        {
            _handler = new BannerUpdateEventHandler(_mockMediator.Object, _mockLogger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BannerUpdateEventHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BannerUpdateEventHandler(_mockMediator.Object, null));
        }


        [Fact]
        public async Task BannerUpdateEventHandler_UpdateExisitingBannerSuccessAsync()
        {
            //Arrange
            var banner = TestDataProvider.GetTestData<BannerDetail>("CustomerTransactionMS\\BannerDoc");

            var bannerResponse = Result.Success(new Response<BannerResponse>()
            {
                Status = Status.OK,
                Value = new BannerResponse(string.Empty, new List<object> { banner })
            });

            var updateEventHandlerRequest = new TransactionBaseEvent()
            {
                Data = JToken.FromObject(new BannerCommandBase()
                {
                    Banner = banner
                })
            };

            //Act
            _mockMediator.Setup(x => x.Send(It.IsAny<BannerQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(bannerResponse.Value);

            var result = await _handler.Handle(updateEventHandlerRequest, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task BannerUpdateEventHandler_AddNewBannerSuccessAsync()
        {
            //Arrange
            var newBanner = new Banners()
            {
                BannerCode = "TestBanner4",
            };

            var newBannerRequest = new BannerDetail()
            {
                Banners = new List<Banners>
                {
                    newBanner
                }
            };

            var banner = TestDataProvider.GetTestData<BannerDetail>("CustomerTransactionMS\\BannerDoc");
            var bannerResponse = Result.Success(new Response<BannerResponse>()
            {
                Status = Status.OK,
                Value = new BannerResponse(string.Empty, new List<object> { banner })
            });

            var updateEventHandlerRequest = new TransactionBaseEvent()
            {
                Data = JToken.FromObject(new BannerCommandBase()
                {
                    Banner = newBannerRequest
                })
            };

            //Act
            _mockMediator.Setup(x => x.Send(It.IsAny<BannerQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(bannerResponse.Value);

            var result = await _handler.Handle(updateEventHandlerRequest, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }
    }
}
