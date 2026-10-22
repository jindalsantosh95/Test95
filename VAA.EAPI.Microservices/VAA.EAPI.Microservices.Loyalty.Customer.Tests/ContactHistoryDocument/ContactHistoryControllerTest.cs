using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.ContactHistoryDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.ContactHistoryDocument
{
    public class ContactHistoryControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly ContactHistoryController _controller;
        private readonly Mock<FunctionContext> _context;

        public ContactHistoryControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new ContactHistoryController(_mockMediator.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ContactHistoryController(null));
        }

        [Fact]
        public void FetchContactHistoryDocumentsByCustomerIdSuccsess()
        {
            string custID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(m => m.Send(It.IsAny<ContactHistoryQuery>(), CancellationToken.None)).ReturnsAsync(new List<ContactHistory>());
            var req = MockHttpRequestData.Create(_context.Object);

            var result = _controller.FetchContactHistoryDocumentsByCustomerIdAsync(req.Object, custID);
            Assert.Equal(HttpStatusCode.OK, result.Result.StatusCode);
        }
    }
}
