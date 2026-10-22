using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.AddressHistory;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.AddressHistoryDocument
{
    public class AddressHistoryControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<AddressHistoryController> _handler;

        public AddressHistoryControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _handler = new Mock<AddressHistoryController>(_mediator.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressHistoryController(null));
        }

        [Fact]
        public async Task GetAddressHistoryByCustomerID_ShouldSucceed_WithGoodCustID()
        {
            // Arrange
            string customerId = "D971F4B8-B029-84D1-E053-6941D90A4EF2";
            var dataResponse = TestDataProvider.GetTestData<IEnumerable<AddressHistory>>("CustomerMS\\AddressHistory\\ListOfAddressHistory");
            _mediator.Setup(m => m.Send(It.IsAny<AddressHistoryQuery>(), CancellationToken.None)).ReturnsAsync(dataResponse);

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            AddressHistoryController controller = new(_mediator.Object);
            var result = await controller.FetchAddressHistory(req.Object, customerId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            // Read in the body
            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var output = Json.Deserialize<IEnumerable<AddressHistory>>(body);
            Assert.Equal(output.FirstOrDefault().CustomerId, customerId);
        }

        [Fact]
        public async Task GetAddressHistoryByCustomerID_Exception()
        {
            // Arrange
            string customerId = string.Empty;
            var dataResponse = TestDataProvider.GetTestData<IEnumerable<AddressHistory>>("CustomerMS\\AddressHistory\\ListOfAddressHistory");
            _mediator.Setup(m => m.Send(It.IsAny<AddressHistoryQuery>(), CancellationToken.None)).Throws(new FluentValidation.ValidationException("Cust Id Missing"));

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            AddressHistoryController controller = new(_mediator.Object);
            var result = await controller.FetchAddressHistory(req.Object, customerId);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
        }
    }
}
