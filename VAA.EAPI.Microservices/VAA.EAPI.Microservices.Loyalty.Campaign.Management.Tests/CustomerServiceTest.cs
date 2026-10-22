using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
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
            CustomerDTO customerResponse = TestDataProvider.GetTestData<CustomerDTO>("CampaignManagementMS\\CustomerDTO");
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerDetailsServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<CustomerDTO> { Status = Status.OK, Value = customerResponse });
            var result = await _customerService.GetCustomer(customerResponse.Customer.CustomerId, null);
            var value = result.Customer;
            Assert.NotNull(value);
        }
    }
}
