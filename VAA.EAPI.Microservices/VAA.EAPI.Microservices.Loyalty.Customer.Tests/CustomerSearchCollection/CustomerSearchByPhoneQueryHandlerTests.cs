using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearchCollection;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.CustomerSearchCollection;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchCollection;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerSearchCollection
{
    public class CustomerSearchByPhoneQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CustomerSearchByPhoneQueryHandler>> _mockLogger;

        public CustomerSearchByPhoneQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CustomerSearchByPhoneQueryHandler>>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchByPhoneQueryHandler(null, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchByPhoneQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task HandleShouldSucceed()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<IEnumerable<CustomerSearch>>("CustomerSearchCollection\\CustomerSearchColDocument");

            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerSearchColRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            CustomerSearchByPhoneQueryHandler handler = new CustomerSearchByPhoneQueryHandler(_mockMediator.Object, _mockLogger.Object);

            // Act
            var result = await handler.Handle(new CustomerSearchByPhoneQuery("12345678"), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("9105857024", result.Value.Value.First().LoyaltyMemberNumber);
            Assert.Equal("12345678", result.Value.Value.First().PhoneNumber);
            Assert.Equal("test45@tgmail.com", result.Value.Value.First().EmailAddress);
        }

        [Fact]
        public async Task HandleShouldThrowValidationException()
        {
            // Arrange
            CustomerSearchByPhoneQueryHandler handler = new CustomerSearchByPhoneQueryHandler(_mockMediator.Object, _mockLogger.Object);

            // Act
            var result = await handler.Handle(new CustomerSearchByPhoneQuery(""), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("G009", Json.Deserialize<List<ErrorDetails>>(result.Error).FirstOrDefault().Code);
            Assert.Equal("There was an unexpected error.", Json.Deserialize<List<ErrorDetails>>(result.Error).FirstOrDefault().Message);
        }
    }
}
