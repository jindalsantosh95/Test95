using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerSearchCommands;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerSearchDocument
{
    public class CustomerSearchUpdateCommandHandlerTests
    {
        private readonly CustomerSerachUpdateCommandHandler _handler;
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<CustomerSerachUpdateCommandHandler>> _mockLogger;

        public CustomerSearchUpdateCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CustomerSerachUpdateCommandHandler>>();
            _handler = new CustomerSerachUpdateCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSerachUpdateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSerachUpdateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task CampaignSearchUpdateEventHandlerSuccess()
        {
            var eventRequest = TestDataProvider.GetTestData<CustomerSearchUpdateCommand>("CustomerSearch\\CustomerSearchUpdateEvent");
            var customerSearchDocument = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument");

            var customerSearchResponse = Result.Success(new Response<CustomerSearch>
            {
                Status = Status.OK,
                Value = customerSearchDocument
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerSearchQuery>(), CancellationToken.None)).ReturnsAsync(customerSearchResponse);
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerSearchRepoUpdateCommand>(), CancellationToken.None)).ReturnsAsync(Unit.Value);
            var result = await _handler.Handle(eventRequest, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }
    }
}
