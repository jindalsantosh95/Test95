using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class BirthdayEmailCommandTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<BirthdayEmailCommandHandler>> _logger;
        private readonly BirthdayEmailCommandHandler _birthdayEmailHandler;
        public BirthdayEmailCommandTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<BirthdayEmailCommandHandler>>();
            _birthdayEmailHandler = new BirthdayEmailCommandHandler(_mediator.Object, _logger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new BirthdayEmailCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new BirthdayEmailCommandHandler(null, _logger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var request = new BirthdayEmailCommand() { Id = Guid.NewGuid() };
            var memberBirthdayEmailResponse = TestDataProvider.GetTestData<List<MemberContactData>>("MemberManagement\\MemberContactData");
            _mediator.Setup(mediator => mediator.Send(It.IsAny<GetMembersForBirthdayEmailQuery>(), CancellationToken.None)).ReturnsAsync(memberBirthdayEmailResponse);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), CancellationToken.None));
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None));
            var result = await _birthdayEmailHandler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(Constants.Application.BirthdayEmailProcessSuccessMessage, result);
        }

        [Fact]
        public async Task HandlerSuccessWithException()
        {
            var request = new BirthdayEmailCommand() { Id = Guid.NewGuid() };
            var memberBirthdayEmailResponse = TestDataProvider.GetTestData<List<MemberContactData>>("MemberManagement\\MemberContactData");
            _mediator.Setup(mediator => mediator.Send(It.IsAny<GetMembersForBirthdayEmailQuery>(), CancellationToken.None)).ReturnsAsync(memberBirthdayEmailResponse);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), CancellationToken.None)).Throws(new NullReferenceException());
            var result = await _birthdayEmailHandler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(Constants.Application.Faliure, result);
        }

        [Fact]
        public async Task HandlerSuccessWithCloudEventException()
        {
            var request = new BirthdayEmailCommand() { Id = Guid.NewGuid() };
            var memberBirthdayEmailResponse = TestDataProvider.GetTestData<List<MemberContactData>>("MemberManagement\\MemberContactData");
            _mediator.Setup(mediator => mediator.Send(It.IsAny<GetMembersForBirthdayEmailQuery>(), CancellationToken.None)).ReturnsAsync(memberBirthdayEmailResponse);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), CancellationToken.None));
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws(new NullReferenceException());
            var result = await _birthdayEmailHandler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(Constants.Application.Faliure, result);
        }
    }
}
