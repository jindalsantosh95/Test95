using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class MemberTierUpdateEventHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly MemberTierUpdateEventHandler _handler;
        private readonly Mock<ILogger<MemberTierUpdateEventHandler>> _logger;

        public MemberTierUpdateEventHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<MemberTierUpdateEventHandler>>();
            _handler = new MemberTierUpdateEventHandler(_mockMediator.Object, _logger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberTierUpdateEventHandler(null, null));

            Assert.Throws<ArgumentNullException>(() => new MemberTierUpdateEventHandler(null, _logger.Object));

            Assert.Throws<ArgumentNullException>(() => new MemberTierUpdateEventHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void HandlerShouldThrowExceptiononInvalidCloudEvent()
        {
            var eventRequest = TestDataProvider.GetTestData<MemberTierUpdateEvent>("MemberManagement\\MemberTierUpdateEvent");

            var resp = _handler.Handle(null, CancellationToken.None);

            Assert.NotNull(resp.Exception);
        }

        [Fact]
        public void HandlerShouldThrowExceptiononInvalidCloudEventData()
        {
            MemberTierUpdateEvent eventRequest = TestDataProvider.GetTestData<MemberTierUpdateEvent>("MemberManagement\\MemberTierUpdateEvent");
            eventRequest.Data = null;

            var resp = _handler.Handle(eventRequest, CancellationToken.None);

            Assert.NotNull(resp.Exception);
        }

        [Fact]
        public void HandlerShouldReturnErrorOnNullInitiatedByAttribute()
        {
            MemberTierUpdateEvent eventRequest = TestDataProvider.GetTestData<MemberTierUpdateEvent>("MemberManagement\\MemberTierUpdateEvent");
            MemberTierUpdateEventData eventData = new MemberTierUpdateEventData()
            {
                InitiatedBy = null,
                CustomerId = "CustomerId001",
                OldTier = "Gold",
                NewTier = "Red"
            };
            eventRequest.Data = JsonSerializer.Serialize(eventData);

            var resp = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.False(resp.Result.IsSuccess);
        }

        [Fact]
        public void HandlerShouldReturnErrorOnEmptyCustomerIdAttribute()
        {
            MemberTierUpdateEvent eventRequest = TestDataProvider.GetTestData<MemberTierUpdateEvent>("MemberManagement\\MemberTierUpdateEvent");
            MemberTierUpdateEventData eventData = new MemberTierUpdateEventData()
            {
                InitiatedBy = "InitiatedBy",
                CustomerId = " ",
                OldTier = "Gold",
                NewTier = "Red"
            };
            eventRequest.Data = JsonSerializer.Serialize(eventData);

            var resp = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.False(resp.Result.IsSuccess);
        }

        [Fact]
        public void HandlerShouldReturnErrorOnEmptyOldTierAttribute()
        {
            MemberTierUpdateEvent eventRequest = TestDataProvider.GetTestData<MemberTierUpdateEvent>("MemberManagement\\MemberTierUpdateEvent");
            MemberTierUpdateEventData eventData = new MemberTierUpdateEventData()
            {
                InitiatedBy = "InitiatedBy",
                CustomerId = "CustomerId",
                OldTier = "",
                NewTier = "Red"
            };
            eventRequest.Data = JsonSerializer.Serialize(eventData);

            var resp = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.False(resp.Result.IsSuccess);
        }

        [Fact]
        public void HandlerShouldReturnErrorOnEmptyNewTierAttribute()
        {
            MemberTierUpdateEvent eventRequest = TestDataProvider.GetTestData<MemberTierUpdateEvent>("MemberManagement\\MemberTierUpdateEvent");
            MemberTierUpdateEventData eventData = new MemberTierUpdateEventData()
            {
                InitiatedBy = "InitiatedBy",
                CustomerId = "CustomerId",
                OldTier = "Gold",
                NewTier = ""
            };
            eventRequest.Data = JsonSerializer.Serialize(eventData);

            var resp = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.False(resp.Result.IsSuccess);
        }


        [Fact]
        public void HandlerShouldReturnSuccessForValidEvent()
        {
            MemberTierUpdateEvent eventRequest = TestDataProvider.GetTestData<MemberTierUpdateEvent>("MemberManagement\\MemberTierUpdateEvent");
            var delinkOnTierUpdateResponse = new Response<string>
            {
                Status = Status.OK,
                Value = Constants.Application.EventInitiatedMessage
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<SSADelinkOnTierUpdateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(delinkOnTierUpdateResponse);

            var resp = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(resp.Result.IsSuccess);
        }
    }
}