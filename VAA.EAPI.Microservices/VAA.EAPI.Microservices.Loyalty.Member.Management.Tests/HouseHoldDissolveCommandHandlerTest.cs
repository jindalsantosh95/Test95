using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseHoldDissolveCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;
        private readonly Mock<ILogger<HouseHoldDissolveCommandHandler>> _mockLogger;
        private readonly HouseHoldDissolveCommandHandler _handler;

        public HouseHoldDissolveCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mockLogger = new Mock<ILogger<HouseHoldDissolveCommandHandler>>();
            _handler = new HouseHoldDissolveCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerProvider.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new HouseHoldDissolveCommandHandler(null, _mockLogger.Object, _mockCustomerProvider.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new HouseHoldDissolveCommandHandler(_mockMediator.Object, null, _mockCustomerProvider.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new HouseHoldDissolveCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public async Task HandlerWithError()
        {
            var response = new Response<string>
            {
                Status = Status.OK,
                Value = "Request processed successfully"
            };
            var eventRequest = TestDataProvider.GetTestData<HouseHoldDissolveCommand>("MemberManagement\\HouseHoldDissolveCommandEventData");
            var request = JsonSerializer.Serialize(eventRequest);
            HouseHoldDissolveCommand dissolveRequest = Json.Deserialize<HouseHoldDissolveCommand>(Convert.ToString(eventRequest.Data));

            string customerID = "D971F4AF-0DD3-84D1-E053-6941D90A4EF2";
            CustomerCustMS customerCustMScustomer = new CustomerCustMS()
            {
                CustomerId = customerID
            };
            CustomerDTO customer = new CustomerDTO()
            {
                Customer = customerCustMScustomer
            };
            var cust = _mockCustomerProvider.Setup(m => m.GetCustomer(customerID, null)).ReturnsAsync(customer);
            var gropDoc = TestDataProvider.GetTestData<HouseHoldGroup>("MemberManagement\\GroupDocument");
            _mockMediator.Setup(m => m.Send(It.IsAny<GroupDocumentServiceQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountDelinkingCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            var result = await _handler.Handle(eventRequest, CancellationToken.None);
        }
    }
}
