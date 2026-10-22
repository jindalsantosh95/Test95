using MediatR;
using Moq;
using System;
using VAA.EAPI.Microservices.Retail.Order.Data.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class OrderDataControllerTest
    {
        private readonly Mock<IMediator> _mediator;

        public OrderDataControllerTest()
        {
            _mediator = new Mock<IMediator>();
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderDataController(null));
        }


    }
}
