using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class BirthdayBonusCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<BirthdayBonusCommandHandler>> _logger;
        private readonly BirthdayBonusCommandHandler _birthdayBonusHandler;
        public BirthdayBonusCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<BirthdayBonusCommandHandler>>();
            _birthdayBonusHandler = new BirthdayBonusCommandHandler(_mediator.Object, _logger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new BirthdayBonusCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new BirthdayBonusCommandHandler(null, _logger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var request = new BirthdayBonusCommand() { Id = Guid.NewGuid() };
            var memberList = TestDataProvider.GetTestData<List<BirthdayBonusMemberDetails>>("MemberManagement\\MemberContactData");
            var productDetail = TestDataProvider.GetTestData<Product>("MemberManagement\\ProductData");
            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocument");
            flyingclubDoc.TierCode = "Gold";
            _mediator.Setup(mediator => mediator.Send(It.IsAny<GetMembersForBirthdayBonusQuery>(), CancellationToken.None)).ReturnsAsync(memberList);
            _mediator.Setup(mediator => mediator.Send(It.IsAny<GetProductRefrenceDetailsQuery>(), CancellationToken.None)).ReturnsAsync(productDetail);
            _mediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), CancellationToken.None));
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None));
            var result = await _birthdayBonusHandler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(Constants.Application.BirthdayBonusProcessSuccessMessage, result);
        }

        [Fact]
        public async Task HandlerSuccessWithEventReqException()
        {
            var request = new BirthdayBonusCommand() { Id = Guid.NewGuid() };
            var memberList = TestDataProvider.GetTestData<List<BirthdayBonusMemberDetails>>("MemberManagement\\MemberContactData");
            var productDetail = TestDataProvider.GetTestData<Product>("MemberManagement\\ProductData");
            _mediator.Setup(mediator => mediator.Send(It.IsAny<GetMembersForBirthdayBonusQuery>(), CancellationToken.None)).Throws(new NullReferenceException()); ;
            _mediator.Setup(mediator => mediator.Send(It.IsAny<GetProductRefrenceDetailsQuery>(), CancellationToken.None)).ReturnsAsync(productDetail);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), CancellationToken.None)).Throws(new NullReferenceException());
            var result = await _birthdayBonusHandler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(Constants.Application.Faliure, result);
        }

        [Fact]
        public async Task HandlerSuccessWithProductDetailsException()
        {
            var request = new BirthdayBonusCommand() { Id = Guid.NewGuid() };
            var memberList = TestDataProvider.GetTestData<List<BirthdayBonusMemberDetails>>("MemberManagement\\MemberContactData");
            var productDetail = TestDataProvider.GetTestData<Product>("MemberManagement\\ProductData");
            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocument");
            flyingclubDoc.TierCode = "Gold";
            _mediator.Setup(mediator => mediator.Send(It.IsAny<GetMembersForBirthdayBonusQuery>(), CancellationToken.None)).ReturnsAsync(memberList);
            _mediator.Setup(mediator => mediator.Send(It.IsAny<GetProductRefrenceDetailsQuery>(), CancellationToken.None)).Throws(new NullReferenceException()); ;
            _mediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), CancellationToken.None));
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), CancellationToken.None)).Throws(new NullReferenceException());
            var result = await _birthdayBonusHandler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(Constants.Application.Faliure, result);
        }
    }

}
