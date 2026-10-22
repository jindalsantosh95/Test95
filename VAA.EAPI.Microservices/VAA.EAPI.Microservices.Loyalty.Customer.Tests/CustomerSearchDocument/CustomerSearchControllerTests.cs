using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerSearchDocument
{
    public class CustomerSearchControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CustomerSearchController>> _logger;
        private readonly Mock<CustomerSearchController> _handler;
        public CustomerSearchControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CustomerSearchController>>();
            // ContactNoteTestHelpers.CreateTestEnvironmentSettings();
            _handler = new Mock<CustomerSearchController>(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchController(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchController(_mediator.Object, null));
        }

        [Fact]
        public async Task GetCustomerSearchByCustomerID_ShouldSucceed_WithGoodCustID()
        {
            // Arrange
            string memberNumber = TestDataProvider.GetTestData<string>("CustomerSearch\\MemberNumber");
            var dataResponse = TestDataProvider.GetTestData<CSharpFunctionalExtensions.Result<Response<CustomerSearch>>>("CustomerSearch\\CustomerSearchGetResponse");
            _mediator.Setup(m => m.Send(It.IsAny<CustomerSearchQuery>(), CancellationToken.None)).ReturnsAsync(dataResponse);

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            CustomerSearchController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.FetchCustomerSearchDocumentByMemberNumberAsync(req.Object, memberNumber);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            // Read in the body
            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var output = Json.Deserialize<CustomerSearch>(body);
            Assert.Equal(output.LoyaltyMemberNumber, memberNumber);
        }
    }
}
