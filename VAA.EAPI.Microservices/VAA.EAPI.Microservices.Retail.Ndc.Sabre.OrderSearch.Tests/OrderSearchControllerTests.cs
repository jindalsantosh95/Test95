using MediatR;
using Moq;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Triggers.Http;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Tests
{
    public class OrderSearchControllerTests
    {
        private readonly Mock<IMediator> _mediator;

        public OrderSearchControllerTests()
        {
            _mediator = new Mock<IMediator>();
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderSearchController(null));
        }

    }
}
