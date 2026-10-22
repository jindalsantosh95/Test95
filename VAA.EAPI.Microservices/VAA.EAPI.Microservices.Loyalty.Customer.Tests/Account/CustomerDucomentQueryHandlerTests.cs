using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Account;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Account;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.Account
{
    public class CustomerDucomentQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly CustomerDucomentQueryHandler _mockHandler;

        public CustomerDucomentQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockHandler = new CustomerDucomentQueryHandler(_mockMediator.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerDucomentQueryHandler(null));
        }

        [Fact]
        public async Task Handler_ThrowError_ForNullMemberNumber()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<CustomerDucomentQuery>("CustomerMS\\Account\\CustomerDucomentQueryRequest");
            request.MemberNumber = null;

            //Act//Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mockHandler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldSucceed_ForGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<CustomerDucomentQuery>("CustomerMS\\Account\\CustomerDucomentQueryRequest");
            var customerSearchResponse = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument");

            _mockMediator.Setup(x => x.Send(It.IsAny<CustomerSearchDocRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customerSearchResponse);

            _mockMediator.Setup(x => x.Send(It.IsAny<CustomerDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CSharpFunctionalExtensions.Result<EAPI.Domain.Entities.Common.Response<Customers>>());

            //Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }
    }
}
