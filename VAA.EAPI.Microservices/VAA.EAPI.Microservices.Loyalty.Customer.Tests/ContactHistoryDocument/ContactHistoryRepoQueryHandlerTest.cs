using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.ContactHistoryDocument;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.ContactHistoryDocument
{
    public class ContactHistoryRepoQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public ContactHistoryRepoQueryHandlerTest()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactHistoryRepoQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(new List<object>());
            ContactHistoryRepoQuery query = new ContactHistoryRepoQuery("CustomerId");
            ContactHistoryRepoQueryHandler handler = new ContactHistoryRepoQueryHandler(_mockMediator.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}