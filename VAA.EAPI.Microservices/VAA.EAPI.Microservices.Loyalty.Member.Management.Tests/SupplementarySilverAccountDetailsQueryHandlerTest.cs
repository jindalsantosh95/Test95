using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class SupplementarySilverAccountDetailsQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly SupplementarySilverAccountDetailsQueryHandler _handler;

        public SupplementarySilverAccountDetailsQueryHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new SupplementarySilverAccountDetailsQueryHandler(_mockMediator.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SupplementarySilverAccountDetailsQueryHandler(null));
        }

        [Fact]
        public async Task HandlerWithEmptyCustomerIdError()
        {
            SupplementarySilverAccountDetailsQuery query = new()
            {
            };
            Result<Response<SupplementarySilverAccountResponse>> result = await _handler.Handle(query, CancellationToken.None);
            Assert.NotNull(result.Error);
        }

        [Fact]
        public async Task HandlerReturnsInvalidFCMDocError()
        {
            SupplementarySilverAccountDetailsQuery query = new()
            {
                CustomerId = "CustomerId001"
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = null });
            Result<Response<SupplementarySilverAccountResponse>> result = await _handler.Handle(query, CancellationToken.None);
            Assert.NotNull(result.Error);
        }

        [Fact]
        public async Task HandlerReturnsSuccessWithValidChildSSAMemberDetails()
        {
            SupplementarySilverAccountDetailsQuery query = new()
            {
                CustomerId = "CustomerId001"
            };
            FlyingClubMember flyingClubMember = new FlyingClubMember()
            {
                CustomerId = "CustomerId001",
                MemberNumber = "MemberId001",
                TierCode = "Silver",
                SSAMemberNumber = "MemberId002"
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingClubMember });
            Result<Response<SupplementarySilverAccountResponse>> result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandlerReturnsFailureWithInvalidTierCode()
        {
            SupplementarySilverAccountDetailsQuery query = new()
            {
                CustomerId = "CustomerId001"
            };
            FlyingClubMember flyingClubMember = new FlyingClubMember()
            {
                CustomerId = "CustomerId001",
                MemberNumber = "MemberId001",
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingClubMember });
            Result<Response<SupplementarySilverAccountResponse>> result = await _handler.Handle(query, CancellationToken.None);
            Assert.NotNull(result.Error);
        }

        [Fact]
        public async Task HandlerReturnsSuccessWithValidHeadDetails()
        {
            SupplementarySilverAccountDetailsQuery query = new()
            {
                CustomerId = "CustomerId001"
            };
            FlyingClubMember flyingClubMember = new FlyingClubMember()
            {
                CustomerId = "CustomerId001",
                MemberNumber = "MemberId001",
                TierCode = "Gold",
                SSAMemberNumber = ""
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingClubMember });
            _mockMediator.Setup(m => m.Send(It.IsAny<FCMDocBySSMemberNumberServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingClubMember });
            Result<Response<SupplementarySilverAccountResponse>> result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }
    }
}
