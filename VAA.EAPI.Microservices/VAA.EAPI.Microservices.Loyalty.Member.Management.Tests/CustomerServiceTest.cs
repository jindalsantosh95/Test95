using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class CustomerServiceTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CustomerService>> _mockLogger;
        private readonly CustomerService _customerService;

        public CustomerServiceTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CustomerService>>();
            _customerService = new CustomerService(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerService(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerService(null, _mockLogger.Object));
        }

        [Fact]
        public async Task GetCustomerShouldSucceedwithValidCustomerId()
        {
            string customerID = "D971F4AF-0DD3-84D1-E053-6941D90A4EF2";

            CustomerCustMS customerCustMScustomer = new CustomerCustMS()
            {
                CustomerId = customerID
            };
            CustomerDTO customer = new CustomerDTO()
            {
                Customer = customerCustMScustomer
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerDetailsServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<CustomerDTO> { Status = Status.OK, Value = customer });

            var result = await _customerService.GetCustomer(customerID, null);
            var value = result.Customer;
            Assert.NotNull(value);
        }

        [Fact]
        public async Task GetCustomerShouldSucceedwithInValidCustomerId()
        {
            string customerID = null;

            CustomerDTO customer = new CustomerDTO()
            {
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerDetailsServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<CustomerDTO> { Status = Status.OK, Value = customer });

            var result = await _customerService.GetCustomer(customerID, null);
            var value = result.Customer;
            Assert.Null(value);
        }

        [Fact]
        public async Task GetCustomerShouldShouldThrowException()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerDetailsServiceQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _customerService.GetCustomer("XXXX", null));
        }

        [Fact]
        public async Task GetCustomerDocumentShouldSucceed()
        {
            Customer customerResponse = TestDataProvider.GetTestData<Customer>("MemberManagement\\CustomerDocument");

            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<Customer> { Status = Status.OK, Value = customerResponse });

            var result = await _customerService.GetCustomerDocument("XXXXX");
            Assert.NotNull(result.CustomerId);
        }

        [Fact]
        public async Task GetCustomerDocumentShouldThrowException()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerDocumentServiceQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _customerService.GetCustomerDocument("XXXXX"));
        }

        [Fact]
        public async Task GetCustomerSearchDocumentShouldSucceed()
        {
            CustomerSearch customerSearchResponse = TestDataProvider.GetTestData<CustomerSearch>("MemberManagement\\CustomerSearchDocument");

            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerSearchDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<CustomerSearch> { Status = Status.OK, Value = customerSearchResponse });

            var result = await _customerService.GetCustomerSearchDocument("XXXXX");
            Assert.NotNull(result.CustomerId);
        }

        [Fact]
        public async Task GetCustomerSearchDocumentShouldThrowException()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerSearchDocumentServiceQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _customerService.GetCustomerSearchDocument("XXXXX"));
        }
    }
}
