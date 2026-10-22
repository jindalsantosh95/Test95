using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class SSADelinkOnMemberStatusUpdateCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<SSADelinkOnMemberStatusUpdateCommandHandler>> _mockLogger;
        private readonly SSADelinkOnMemberStatusUpdateCommandHandler _handler;

        public SSADelinkOnMemberStatusUpdateCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<SSADelinkOnMemberStatusUpdateCommandHandler>>();
            _handler = new SSADelinkOnMemberStatusUpdateCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SSADelinkOnMemberStatusUpdateCommandHandler(null, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new SSADelinkOnMemberStatusUpdateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void HandlerShouldSuccessWithException()
        {
            SSADelinkOnMemberStatusUpdateCommand command = null;
            var result = _handler.Handle(command, CancellationToken.None);
            Assert.NotNull(result.Exception);
        }

        [Fact]
        public void HandlerShouldReturnErorrForNullHeadCustomerId()
        {
            SSADelinkOnMemberStatusUpdateCommand command = new SSADelinkOnMemberStatusUpdateCommand()
            {
                HeadCustomerId = null,
                InitiatedBy = "Test"
            };
            var result = _handler.Handle(command, CancellationToken.None);
            Assert.False(result.Result.IsSuccess);
        }

        [Fact]
        public void HandlerShouldReturnErorrForEmptyInitiatedBy()
        {
            SSADelinkOnMemberStatusUpdateCommand command = new SSADelinkOnMemberStatusUpdateCommand()
            {
                HeadCustomerId = "123",
                InitiatedBy = ""
            };
            var result = _handler.Handle(command, CancellationToken.None);
            Assert.False(result.Result.IsSuccess);
        }

        [Fact]
        public void HandlerShouldReturnSuccessForValidRequest()
        {
            SSADelinkOnMemberStatusUpdateCommand command = new SSADelinkOnMemberStatusUpdateCommand()
            {
                HeadCustomerId = "123",
                InitiatedBy = "Test"
            };

            var headFCMDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithGroupId");
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = headFCMDoc });

            var childFCMDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithSSAMemberId");
            _mockMediator.Setup(m => m.Send(It.IsAny<FCMDocBySSMemberNumberServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = childFCMDoc });

            var response = new Response<SupplementarySilverDelinkingResponse>
            {
                Status = Status.OK,
                Value = new SupplementarySilverDelinkingResponse() { Message = Constants.Application.HoHLinkingInitiatedMessage }
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<SupplementarySilverDelinkingCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            var result = _handler.Handle(command, CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
        }

        [Fact]
        public void HandlerShouldReturnFailureForInValidHeadMember()
        {
            SSADelinkOnMemberStatusUpdateCommand command = new SSADelinkOnMemberStatusUpdateCommand()
            {
                HeadCustomerId = "123",
                InitiatedBy = "Test"
            };

            FlyingClubMember headFCMDoc = null;
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = headFCMDoc });

            var result = _handler.Handle(command, CancellationToken.None);
            Assert.False(result.Result.IsSuccess);
        }


        [Fact]
        public void HandlerShouldReturnFailureForInValidChildMember()
        {
            SSADelinkOnMemberStatusUpdateCommand command = new SSADelinkOnMemberStatusUpdateCommand()
            {
                HeadCustomerId = "123",
                InitiatedBy = "Test"
            };

            var headFCMDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithGroupId");
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = headFCMDoc });

            FlyingClubMember childFCMDoc = null;
            _mockMediator.Setup(m => m.Send(It.IsAny<FCMDocBySSMemberNumberServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = childFCMDoc });

            var result = _handler.Handle(command, CancellationToken.None);
            Assert.False(result.Result.IsSuccess);
        }

    }
}
