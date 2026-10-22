using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignCreate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignCreateRequestCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignCreateRequestCommandHandler>> _logger;
        private readonly CampaignCreateRequestCommandHandler _handler;

        public CampaignCreateRequestCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignCreateRequestCommandHandler>>();
            _handler = new CampaignCreateRequestCommandHandler(_mediator.Object, _logger.Object);

            ConfigReader.Set(Constants.Environment.ServiceBusTopicName, "dummyValue1");
            ConfigReader.Set(Constants.Environment.CampaignCreateResponseUrl, "http://localhost:7071/api/campaigncreate/response/{correlationid}");

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignCreateRequestCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignCreateRequestCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            CampaignCreateRequestCommand campaignData = TestDataProvider.GetTestData<CampaignCreateRequestCommand>("CampaignManagementMS\\CampaignCreateValid");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignCreateRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None));
            var result = _handler.Handle(campaignData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithError()
        {
            CampaignCreateRequestCommand campaignData = TestDataProvider.GetTestData<CampaignCreateRequestCommand>("CampaignManagementMS\\CampaignCreateInvalid");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignCreateRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None));
            var result = _handler.Handle(campaignData, System.Threading.CancellationToken.None);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            CampaignCreateRequestCommand campaignData = TestDataProvider.GetTestData<CampaignCreateRequestCommand>("CampaignManagementMS\\CampaignCreateValid");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignCreateRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None));
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(campaignData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}
