using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.MemberAccountDetails;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class MemberAccountReferenceDataQueryHandlerTest
    {
        private readonly Mock<ILogger<MemberAccountReferenceDataQueryHandler>> _logger;
        private readonly MemberAccountReferenceDataQueryHandler _handler;
        private readonly Mock<IMediator> _mediator;

        public MemberAccountReferenceDataQueryHandlerTest()
        {
            _logger = new Mock<ILogger<MemberAccountReferenceDataQueryHandler>>();
            _mediator = new Mock<IMediator>();
            _handler = new MemberAccountReferenceDataQueryHandler(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberAccountReferenceDataQueryHandler(null, _logger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new MemberAccountReferenceDataQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var request = new MemberAccountReferenceDataQuery();
            var referenceData = TestDataProvider.GetTestData<MemberAccountReferenceData>("MemberManagement\\RetrieveMemberAccountReferenceData");
            _mediator.Setup(x => x.Send(It.IsAny<MemberAccountReferenceDataCommand>(), CancellationToken.None)).ReturnsAsync(new Response<MemberAccountReferenceData>() { Status = Status.OK, Value = referenceData });
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.NotNull(result.Value.Value);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerFailWithError()
        {
            var request = new MemberAccountReferenceDataQuery();
            var referenceData = TestDataProvider.GetTestData<MemberAccountReferenceData>("MemberManagement\\RetrieveMemberAccountReferenceData");
            _mediator.Setup(x => x.Send(It.IsAny<MemberAccountReferenceDataCommand>(), CancellationToken.None)).Throws(new Exception());
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
    }
}
