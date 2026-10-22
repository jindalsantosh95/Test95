using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Account;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MembershipChange;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Account;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MembershipChange;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class MemberTierQueryHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseQueryHandler<MemberTierResponse>>> _mockLogger = new();

        public MemberTierQueryHandlerTests()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberTierQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberTierQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldReturnMemberStatusasLTG_ForGoodRequest()
        {
            // Arrange
            var mockResponsAwards = TestDataProvider.GetTestData<Response<AwardResponse>>("CustomerTransactionMS\\AwardDocLTG");
            var mockResponseFCM = TestDataProvider.GetTestData<Response<FlyingClubMemberResponse>>("CustomerTransactionMS\\FCMDocument");
            var mockResponseMC = TestDataProvider.GetTestData<Response<MembershipChangeResponse>>("CustomerTransactionMS\\MemberShipChangeDoc");
            var mockResponseTrans = TestDataProvider.GetTestData<Response<TransactionResponse>>("CustomerTransactionMS\\TransactionDoc");

            string customerID = TestDataProvider.GetTestData<string>("CustID");

            _mockMediator.Setup(m => m.Send(It.IsAny<AwardsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponsAwards);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseFCM);
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseMC);
            _mockMediator.Setup(m => m.Send(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseTrans);

            MemberTierQueryHandler handler = new(_mockMediator.Object, _mockLogger.Object);

            MemberTierQuery query = new() { CustomerId = customerID };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Value.MemberStatus.IsLifeTimeGoldMember);
        }

        [Fact]
        public async Task Handle_ShouldReturnMemberStatusasMMandWings_ForGoodRequest()
        {
            // Arrange
            var mockResponsAwards = TestDataProvider.GetTestData<Response<AwardResponse>>("CustomerTransactionMS\\AwardDocMMandWings");
            var mockResponseFCM = TestDataProvider.GetTestData<Response<FlyingClubMemberResponse>>("CustomerTransactionMS\\FCMDocument");
            var mockResponseMC = TestDataProvider.GetTestData<Response<MembershipChangeResponse>>("CustomerTransactionMS\\MemberShipChangeDoc");
            var mockResponseTrans = TestDataProvider.GetTestData<Response<TransactionResponse>>("CustomerTransactionMS\\TransactionDoc");

            string customerID = TestDataProvider.GetTestData<string>("CustID");

            _mockMediator.Setup(m => m.Send(It.IsAny<AwardsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponsAwards);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseFCM);
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseMC);
            _mockMediator.Setup(m => m.Send(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseTrans);

            MemberTierQueryHandler handler = new(_mockMediator.Object, _mockLogger.Object);

            MemberTierQuery query = new() { CustomerId = customerID };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Value.MemberStatus.IsMillionMiler);
            Assert.True(result.Value.Value.MemberStatus.IsWingsMemeber);
        }

        [Fact]
        public async Task Handle_ShouldReturnMemberStatusasTwoMM_ForGoodRequest()
        {
            // Arrange
            var mockResponsAwards = TestDataProvider.GetTestData<Response<AwardResponse>>("CustomerTransactionMS\\AwardDocMMandWings");
            var mockResponseFCM = TestDataProvider.GetTestData<Response<FlyingClubMemberResponse>>("CustomerTransactionMS\\FCMDocument2MM");
            var mockResponseMC = TestDataProvider.GetTestData<Response<MembershipChangeResponse>>("CustomerTransactionMS\\MemberShipChangeDoc");
            var mockResponseTrans = TestDataProvider.GetTestData<Response<TransactionResponse>>("CustomerTransactionMS\\TransactionDoc");

            string customerID = TestDataProvider.GetTestData<string>("CustID");

            _mockMediator.Setup(m => m.Send(It.IsAny<AwardsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponsAwards);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseFCM);
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseMC);
            _mockMediator.Setup(m => m.Send(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseTrans);

            MemberTierQueryHandler handler = new(_mockMediator.Object, _mockLogger.Object);

            MemberTierQuery query = new() { CustomerId = customerID };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Value.MemberStatus.Is2MillionMiler);
        }

        [Fact]
        public async Task Handle_ShouldReturnUpgardebyDateasOneyearFromCurrentYear_ForNoTransactionActivityinLastYear()
        {
            // Arrange
            var mockResponsAwards = TestDataProvider.GetTestData<Response<AwardResponse>>("CustomerTransactionMS\\AwardDocMMandWings");
            var mockResponseFCM = TestDataProvider.GetTestData<Response<FlyingClubMemberResponse>>("CustomerTransactionMS\\FCMDocument2MM");
            var mockResponseMC = TestDataProvider.GetTestData<Response<MembershipChangeResponse>>("CustomerTransactionMS\\MemberShipChangeDoc");
            var mockResponseTrans = TestDataProvider.GetTestData<Response<TransactionResponse>>("CustomerTransactionMS\\TransactionDoc");

            string customerID = TestDataProvider.GetTestData<string>("CustID");

            _mockMediator.Setup(m => m.Send(It.IsAny<AwardsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponsAwards);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseFCM);
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseMC);
            _mockMediator.Setup(m => m.Send(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseTrans);

            MemberTierQueryHandler handler = new(_mockMediator.Object, _mockLogger.Object);

            MemberTierQuery query = new() { CustomerId = customerID };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Value.Value.RollingTierPoints);
            Assert.Equal(DateTime.Now.AddYears(1).AddMonths(1).AddDays(-DateTime.Now.AddYears(1).AddMonths(1).Day).Date, result.Value.Value.UpgradeByDate);
        }

        [Fact]
        public async Task Handle_ShouldReturnActualUpgardebyDateandRollingTierPoints_ForTransactionActivityWithinLastYear()
        {
            // Arrange
            var mockResponsAwards = TestDataProvider.GetTestData<Response<AwardResponse>>("CustomerTransactionMS\\AwardDocMMandWings");
            var mockResponseFCM = TestDataProvider.GetTestData<Response<FlyingClubMemberResponse>>("CustomerTransactionMS\\FCMDocument2MM");
            var mockResponseMC = TestDataProvider.GetTestData<Response<MembershipChangeResponse>>("CustomerTransactionMS\\MemberShipChangeDoc");

            var mockResponseTrans = TestDataProvider.GetTestData<Response<TransactionResponse>>("CustomerTransactionMS\\TransactionDocwithPastAvctivity");
            mockResponseTrans.Value.TransactionDocuments.FirstOrDefault(x => x.CreditDetails.FirstOrDefault().CreditValue.Is("50")).ActivityDate = DateTime.UtcNow.AddMonths(-6).ToString();


            string customerID = TestDataProvider.GetTestData<string>("CustID");

            _mockMediator.Setup(m => m.Send(It.IsAny<AwardsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponsAwards);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseFCM);
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseMC);
            _mockMediator.Setup(m => m.Send(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseTrans);

            MemberTierQueryHandler handler = new(_mockMediator.Object, _mockLogger.Object);

            MemberTierQuery query = new() { CustomerId = customerID };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(50, result.Value.Value.RollingTierPoints);
            Assert.NotEqual(DateTime.Now.AddYears(1).AddMonths(1).AddDays(-DateTime.Now.AddYears(1).AddMonths(1).Day).Date, result.Value.Value.UpgradeByDate);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrueMemberStatusforMMandFalseforDisabledWings_ForGoodRequest()
        {
            // Arrange
            var mockResponsAwards = TestDataProvider.GetTestData<Response<AwardResponse>>("CustomerTransactionMS\\AwardDocMMandWings");
            mockResponsAwards.Value.Awards.First(x => x.ProductId == "1634561747").ExpiryDate = DateTime.UtcNow;

            var mockResponseFCM = TestDataProvider.GetTestData<Response<FlyingClubMemberResponse>>("CustomerTransactionMS\\FCMDocument");
            var mockResponseMC = TestDataProvider.GetTestData<Response<MembershipChangeResponse>>("CustomerTransactionMS\\MemberShipChangeDoc");
            var mockResponseTrans = TestDataProvider.GetTestData<Response<TransactionResponse>>("CustomerTransactionMS\\TransactionDoc");

            string customerID = TestDataProvider.GetTestData<string>("CustID");

            _mockMediator.Setup(m => m.Send(It.IsAny<AwardsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponsAwards);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseFCM);
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseMC);
            _mockMediator.Setup(m => m.Send(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseTrans);

            MemberTierQueryHandler handler = new(_mockMediator.Object, _mockLogger.Object);

            MemberTierQuery query = new() { CustomerId = customerID };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Value.MemberStatus.IsMillionMiler);
            Assert.False(result.Value.Value.MemberStatus.IsWingsMemeber);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrueMemberStatusforMMWing2MMandFalseforDisabledLTG_ForGoodRequest()
        {
            // Arrange
            var mockResponsAwards = TestDataProvider.GetTestData<Response<AwardResponse>>("CustomerTransactionMS\\AwardDocMMandWings");
            var mockResponseFCM = TestDataProvider.GetTestData<Response<FlyingClubMemberResponse>>("CustomerTransactionMS\\FCMDocument2MM");

            var mockResponseMC = TestDataProvider.GetTestData<Response<MembershipChangeResponse>>("CustomerTransactionMS\\MemberShipChangeDoc");
            var mockResponseTrans = TestDataProvider.GetTestData<Response<TransactionResponse>>("CustomerTransactionMS\\TransactionDocwithPastAvctivity");

            string customerID = TestDataProvider.GetTestData<string>("CustID");

            _mockMediator.Setup(m => m.Send(It.IsAny<AwardsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponsAwards);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseFCM);
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseMC);
            _mockMediator.Setup(m => m.Send(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponseTrans);

            MemberTierQueryHandler handler = new(_mockMediator.Object, _mockLogger.Object);

            MemberTierQuery query = new() { CustomerId = customerID };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Value.MemberStatus.IsMillionMiler);
            Assert.True(result.Value.Value.MemberStatus.IsWingsMemeber);
            Assert.False(result.Value.Value.MemberStatus.IsLifeTimeGoldMember);
            Assert.True(result.Value.Value.MemberStatus.Is2MillionMiler);
        }
    }
}
