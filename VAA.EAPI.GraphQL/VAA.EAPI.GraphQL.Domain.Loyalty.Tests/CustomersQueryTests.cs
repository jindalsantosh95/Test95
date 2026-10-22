using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CustomersQueryTests
    {
        private readonly Mock<ILogger<ICustomerLogger>> _logger;
        private readonly Mock<ICustomerProvider> _custProvider;

        public CustomersQueryTests()
        {
            _logger = new Mock<ILogger<ICustomerLogger>>();
            _custProvider = new Mock<ICustomerProvider>();
        }

        [Fact]
        public void Constructor_ThrowException_NullReferences()
        {
            // Null Customer Service
            Assert.Throws<ArgumentNullException>(() => new CustomersQuery(null, _logger.Object));

            // Null Logger Service
            Assert.Throws<ArgumentNullException>(() => new CustomersQuery(_custProvider.Object, null));
        }

        [Fact]
        public void Custimer_ThrowException_NullReference() { }

        /*
        [Fact]
        public async Task ContactNotes_ThrowException_NullReferences()
        {
            // Null NotesProvider
            await Assert.ThrowsAsync<ArgumentNullException>(() => _contactNotesQuery.ContactNotes("xxx", "xxx", _custProvider.Object, null, _mockLogger.Object));
            // Null Customer provider
            await Assert.ThrowsAsync<ArgumentNullException>(() => _contactNotesQuery.ContactNotes("xxx", "xxx", null, _noteProvider.Object, _mockLogger.Object));
            // No cust ID or Memb ID
            await Assert.ThrowsAsync<ArgumentException>(() => _contactNotesQuery.ContactNotes("", "", _custProvider.Object, _noteProvider.Object, _mockLogger.Object));
        }
        */

        [Fact]
        public async Task Customer_ShouldSucceed_WithMembID()
        {

            // Arrange
            var MembID = "1000000000";
            var cust = Framework.Tests.TestHelpers.TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");

            _custProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cust);

            var classUnderTest = new CustomersQuery(_custProvider.Object, _logger.Object);

            // Act
            var testOutput = await classUnderTest.Customer("", MembID);

            // Assert
            Assert.NotNull(testOutput);
            Assert.Equal(cust.Customer.CustomerId, testOutput.CustomerId);


        }

        [Fact]
        public async Task Customer_ShouldSucceed_WithMCustID()
        {

            // Arrange
            var cust = Framework.Tests.TestHelpers.TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");

            _custProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cust);

            var classUnderTest = new CustomersQuery(_custProvider.Object, _logger.Object);


            // Act
            var testOutput = await classUnderTest.Customer(cust.Customer.CustomerId, "");

            // Assert
            Assert.NotNull(testOutput);
            Assert.Equal(cust.Customer.CustomerId, testOutput.CustomerId);
        }
    }
}