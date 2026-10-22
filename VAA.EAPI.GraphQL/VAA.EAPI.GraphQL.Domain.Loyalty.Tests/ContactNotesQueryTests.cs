using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class ContactNotesQueryTests
    {
        private readonly Mock<ILogger<ContactNotesQuery>> _mockLogger;
        private readonly ContactNotesQuery _contactNotesQuery;
        private readonly Mock<IContactNoteQueryProvider> _noteProvider;
        private readonly Mock<ICustomerProvider> _custProvider;

        public ContactNotesQueryTests()
        {
            _mockLogger = new Mock<ILogger<ContactNotesQuery>>();
            _contactNotesQuery = new ContactNotesQuery(_mockLogger.Object);
            _noteProvider = new Mock<IContactNoteQueryProvider>();
            _custProvider = new Mock<ICustomerProvider>();

        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactNotesQuery(null));
        }

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

        [Fact]
        public async Task ContactNotes_ShouldSucceed_WithMembID()
        {

            // Arrange
            var MembID = "1000000000";
            var cust = Framework.Tests.TestHelpers.TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _custProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cust);
            _noteProvider.Setup(n => n.Query(It.IsAny<string>())).ReturnsAsync(new List<ContactNote>());

            // Act
            var testOutput = await _contactNotesQuery.ContactNotes("", MembID, _custProvider.Object, _noteProvider.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(testOutput);

        }
    }
}
