using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.PartnersRetrieval;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class PartnersRetrievalResponseQueryHandlerTest
    {
        private readonly Mock<ILogger<PartnersRetrievalResponseQueryHandler>> _logger;
        private readonly Mock<IMediator> _mediator;
        private readonly PartnersRetrievalResponseQueryHandler _handler;

        public PartnersRetrievalResponseQueryHandlerTest()
        {
            _logger = new Mock<ILogger<PartnersRetrievalResponseQueryHandler>>();
            _mediator = new Mock<IMediator>();
            _handler = new PartnersRetrievalResponseQueryHandler(_mediator.Object, _logger.Object);
        }


        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PartnersRetrievalResponseQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new PartnersRetrievalResponseQueryHandler(null, _logger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var partnersResponse = TestDataProvider.GetTestData<PartnersResponse>("MemberManagement\\PartnersResponse");
            _mediator.Setup(m => m.Send(It.IsAny<PartnersRetrieveCommand>(), CancellationToken.None)).ReturnsAsync(new Response<PartnersResponse> { Status = Status.OK, Value = partnersResponse });
            PartnersRetrievalQuery query = new() { };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task HandlerSuccessWithException()
        {
            var partnersResponse = TestDataProvider.GetTestData<PartnersResponse>("MemberManagement\\PartnersResponse");
            _mediator.Setup(m => m.Send(It.IsAny<PartnersRetrieveCommand>(), CancellationToken.None)).ThrowsAsync(new Exception());
            PartnersRetrievalQuery query = new() { };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
    }
}
