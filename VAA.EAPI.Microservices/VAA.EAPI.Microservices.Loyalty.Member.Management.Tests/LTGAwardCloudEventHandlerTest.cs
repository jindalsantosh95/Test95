using MediatR;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class LTGAwardCloudEventHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly LTGAwardCloudEventHandler _handler;

        public LTGAwardCloudEventHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new LTGAwardCloudEventHandler(_mockMediator.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LTGAwardCloudEventHandler(null));
        }

        [Fact]
        public void HandlerShouldSuccessWithException()
        {
            LTGAwardCloudEvent cloudEvent = null;
            var result = _handler.Handle(cloudEvent, CancellationToken.None);
            Assert.NotNull(result.Exception);
        }

        [Fact]
        public void HandlerShouldSuccessWithValidCammand()
        {
            var eventRequest = TestDataProvider.GetTestData<LTGAwardCloudEvent>("MemberManagement\\LTGAwardEvent");
            _mockMediator.Setup(m => m.Send(It.IsAny<LTGEmailTriggerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<string> { Value = "Success" });
            var result = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.Null(result.Exception);
            Assert.True(result.Result.IsSuccess);
        }
    }
}
