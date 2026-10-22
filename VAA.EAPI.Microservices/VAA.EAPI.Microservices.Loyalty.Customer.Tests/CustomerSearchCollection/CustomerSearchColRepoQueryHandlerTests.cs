using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearchCollection;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchCollection;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerSearchCollection
{
    public class CustomerSearchColRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;

        public CustomerSearchColRepoQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchColRepoQueryHandler(null));
        }


        [Fact]
        public async Task HandleShouldSucceed()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerSearchCollection\\CustomerSearchColDocument");

            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockResponse);
            CustomerSearchColRepoQueryHandler handler = new CustomerSearchColRepoQueryHandler(_mockMediator.Object);

            // Act
            var result = await handler.Handle(new CustomerSearchColRepoQuery(new CustomerSearchByPhoneQuery("12345678")), CancellationToken.None);

            // Assert
            Assert.Equal("9105857024", result.ToList().First().LoyaltyMemberNumber);
            Assert.Equal("12345678", result.ToList().First().PhoneNumber);
            Assert.Equal("test45@tgmail.com", result.ToList().First().EmailAddress); Assert.NotNull(result);
        }

    }
}
