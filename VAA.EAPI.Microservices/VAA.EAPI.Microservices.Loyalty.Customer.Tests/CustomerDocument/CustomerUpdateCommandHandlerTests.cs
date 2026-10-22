using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerDocument
{
    public class CustomerUpdateCommandHandlerTests
    {
        private readonly CustomerUpdateCommandHandler _handler;
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<CustomerUpdateCommandHandler>> _mockLogger;

        public CustomerUpdateCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CustomerUpdateCommandHandler>>();
            _handler = new CustomerUpdateCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerUpdateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerUpdateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task CustomerUpdateCommandHandlerSuccess()
        {
            var eventRequest = TestDataProvider.GetTestData<CustomerUpdateCommand>("CustomerMS\\CustomerDocument\\CustomerDocUpdateEvent");
            var customerDocument = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc");

            var customerResponse = Result.Success(new Response<Customers>
            {
                Status = Status.OK,
                Value = customerDocument
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerResponse);
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerRepoUpdateCommand>(), CancellationToken.None)).ReturnsAsync(Unit.Value);
            var result = await _handler.Handle(eventRequest, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }

    }
}
