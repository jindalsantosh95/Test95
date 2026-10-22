using Moq;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerAssignAwardTests
{
    public class AuthorizationMapperTest
    {

        private readonly Mock<ICustomerService<CustomersDTO>> _mockService;

        public AuthorizationMapperTest()
        {
            _mockService = new Mock<ICustomerService<CustomersDTO>>();
        }
    }
}
