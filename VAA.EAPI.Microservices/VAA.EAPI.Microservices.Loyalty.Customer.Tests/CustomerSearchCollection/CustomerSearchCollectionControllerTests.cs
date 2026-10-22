using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearchCollection;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.CustomerSearchCollection;
using VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerSearchCollection
{
    public class CustomerSearchCollectionControllerTests
    {
        private readonly Mock<IMediator> _mediator;

        public CustomerSearchCollectionControllerTests()
        {
            _mediator = new Mock<IMediator>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchCollectionController(null));
        }

        [Fact]
        public async Task FetchCustomerSearchByPhoneNumberAsyncSuccess()
        {
            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);
            var mockResponse = TestDataProvider.GetTestData<IEnumerable<CustomerSearch>>("CustomerSearchCollection\\CustomerSearchColDocument");

            _mediator.Setup(m => m.Send(It.IsAny<CustomerSearchByPhoneQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IEnumerable<CustomerSearch>> { Status = EAPI.Domain.Entities.Common.Status.OK, Value = mockResponse });

            CustomerSearchCollectionController controller = new(_mediator.Object);
            var result = await controller.FetchCustomerSearchByPhoneNumberAsync(req.Object, "12345678");

            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);

            // Read in the body
            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            List<CustomerSearch> output = Json.Deserialize<List<CustomerSearch>>(body);
            Assert.Equal("9105857024", output.First().LoyaltyMemberNumber);
            Assert.Equal("12345678", output.First().PhoneNumber);
            Assert.Equal("test45@tgmail.com", output.First().EmailAddress);
        }
    }
}
