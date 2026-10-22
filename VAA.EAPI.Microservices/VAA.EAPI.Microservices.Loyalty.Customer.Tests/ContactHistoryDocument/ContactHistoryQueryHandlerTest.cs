using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.ContactHistoryDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.ContactHistoryDocument;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.ContactHistoryDocument
{
    public class ContactHistoryQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public ContactHistoryQueryHandlerTest()
        {
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactHistoryQueryHandler(null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            // Arrange
            string customerID = "CustID";
            _mockMediator.Setup(s => s.Send(It.IsAny<ContactHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ContactHistory>());
            ContactHistoryQueryHandler handler = new ContactHistoryQueryHandler(_mockMediator.Object);

            ContactHistoryQuery query = new ContactHistoryQuery(customerID);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Null_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<ContactHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            ContactHistoryQueryHandler handler = new ContactHistoryQueryHandler(_mockMediator.Object);
            ContactHistoryQuery query = new ContactHistoryQuery(null);

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Empty_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<ContactHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            ContactHistoryQueryHandler handler = new ContactHistoryQueryHandler(_mockMediator.Object);
            ContactHistoryQuery query = new ContactHistoryQuery("");

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }
    }
}
