using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class AddressChangeEventCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;
        private readonly Mock<ILogger<AddressChangeEventCommandHandler>> _mockLogger;
        private readonly AddressChangeEventCommandHandler _handler;

        public AddressChangeEventCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mockLogger = new Mock<ILogger<AddressChangeEventCommandHandler>>();
            _handler = new AddressChangeEventCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerProvider.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressChangeEventCommandHandler(null, _mockLogger.Object, _mockCustomerProvider.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new AddressChangeEventCommandHandler(_mockMediator.Object, null, _mockCustomerProvider.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new AddressChangeEventCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public async Task HandlerShouldSuccess()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressChangeEventCommand>("MemberManagement\\AddressChangeEventData");
            Customer customerResponse = TestDataProvider.GetTestData<Customer>("MemberManagement\\CustomerDocument");
            var custSearchResponse = TestDataProvider.GetTestData<CustomerSearch>("MemberManagement\\CustomerSearchDocument");

            _mockCustomerProvider.Setup(x => x.GetCustomerDocument(It.IsAny<string>())).ReturnsAsync(customerResponse);
            _mockCustomerProvider.Setup(x => x.GetCustomerSearchDocument(It.IsAny<string>())).ReturnsAsync(custSearchResponse);
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountEmailTriggerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldSuccessWihtValidationFailure()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressChangeEventCommand>("MemberManagement\\AddressChangeEventData");

            eventRequest.Data = eventRequest.Data.ToString().Replace("D971F4B8-61BB-84D1-E053-6941D90A4EF2", "");

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldFailWhenCustDocIsEmpty()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressChangeEventCommand>("MemberManagement\\AddressChangeEventData");
            Customer customerResponse = null;

            _mockCustomerProvider.Setup(x => x.GetCustomerDocument(It.IsAny<string>())).ReturnsAsync(customerResponse);

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task HandlerShouldFailWhenCustSearchDocIsEmpty()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressChangeEventCommand>("MemberManagement\\AddressChangeEventData");
            Customer customerResponse = TestDataProvider.GetTestData<Customer>("MemberManagement\\CustomerDocument");
            CustomerSearch custSearchResponse = null;

            _mockCustomerProvider.Setup(x => x.GetCustomerDocument(It.IsAny<string>())).ReturnsAsync(customerResponse);
            _mockCustomerProvider.Setup(x => x.GetCustomerSearchDocument(It.IsAny<string>())).ReturnsAsync(custSearchResponse);

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task HandlerShouldThrowException()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressChangeEventCommand>("MemberManagement\\AddressChangeEventData");
            Customer customerResponse = TestDataProvider.GetTestData<Customer>("MemberManagement\\CustomerDocument");
            var custSearchResponse = TestDataProvider.GetTestData<CustomerSearch>("MemberManagement\\CustomerSearchDocument");

            _mockCustomerProvider.Setup(x => x.GetCustomerDocument(It.IsAny<string>())).ReturnsAsync(customerResponse);
            _mockCustomerProvider.Setup(x => x.GetCustomerSearchDocument(It.IsAny<string>())).ReturnsAsync(custSearchResponse);
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountEmailTriggerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
    }
}
