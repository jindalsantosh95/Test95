using MediatR;
using Moq;
using System;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class FCNumerToPNRResponseEventCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<TableStorageService<MemberDetailsToAddSSR>> _tableStorageService;
        private readonly FCNumerToPNRResponseEventCommandHandler _handler;

        public FCNumerToPNRResponseEventCommandHandlerTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
            _mediator = new Mock<IMediator>();
            _tableStorageService = new Mock<TableStorageService<MemberDetailsToAddSSR>>();
            _handler = new FCNumerToPNRResponseEventCommandHandler(_mediator.Object, _tableStorageService.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new FCNumerToPNRResponseEventCommandHandler(null, _tableStorageService.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenTableStorageObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new FCNumerToPNRResponseEventCommandHandler(_mediator.Object, null));
        }
    }
}
