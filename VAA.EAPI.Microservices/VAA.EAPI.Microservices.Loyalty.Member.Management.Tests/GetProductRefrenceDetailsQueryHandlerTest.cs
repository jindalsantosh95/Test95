using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class GetProductRefrenceDetailsQueryHandlerTest
    {
        private readonly Mock<ILogger<GetProductRefrenceDetailsQueryHandler>> _logger;
        private readonly GetProductRefrenceDetailsQueryHandler _handler;
        private readonly Mock<ICommonRefDBServices> _commonRefDBServices;

        public GetProductRefrenceDetailsQueryHandlerTest()
        {
            _commonRefDBServices = new Mock<ICommonRefDBServices>();
            _logger = new Mock<ILogger<GetProductRefrenceDetailsQueryHandler>>();
            _handler = new GetProductRefrenceDetailsQueryHandler(_commonRefDBServices.Object, _logger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetProductRefrenceDetailsQueryHandler(_commonRefDBServices.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenDbServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetProductRefrenceDetailsQueryHandler(null, _logger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            string ProductName = "Birthday Bonus";
            var request = new GetProductRefrenceDetailsQuery();
            var productResponse = TestDataProvider.GetTestData<Product>("MemberManagement\\ProductData");
            _commonRefDBServices.Setup(m => m.GetProductReferenceDataByName(It.IsAny<string>())).ReturnsAsync(productResponse);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.NotNull(result.ProductId);
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task HandlerSuccessWithException()
        {
            string ProductName = "Birthday Bonus";
            GetProductRefrenceDetailsQuery request = null;
            var productResponse = TestDataProvider.GetTestData<Product>("MemberManagement\\ProductData");
            _commonRefDBServices.Setup(m => m.GetProductReferenceDataByName(ProductName)).ThrowsAsync(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
