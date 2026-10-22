using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class VouchersQueryHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<IMediator> _mediator;

        private readonly Mock<ILogger<TransactionBaseQueryHandler<AwardResponse>>> _awardLogger;
        private readonly Mock<ILogger<TransactionBaseQueryHandler<FlyingClubMemberResponse>>> _memberLogger;
        private readonly Mock<ILogger<TransactionBaseQueryHandler<TransactionResponse>>> _transactionLogger;

        public VouchersQueryHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _mediator = new Mock<IMediator>();
            _awardLogger = new Mock<ILogger<TransactionBaseQueryHandler<AwardResponse>>>();
            _memberLogger = new Mock<ILogger<TransactionBaseQueryHandler<FlyingClubMemberResponse>>>();
            _transactionLogger = new Mock<ILogger<TransactionBaseQueryHandler<TransactionResponse>>>();

            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
            SetupMediatorMocks();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton(_mediator.Object)
                .AddSingleton<IReferenceService>(_ => CreateReferenceServiceMock())
                .AddSingleton(_ => new AwardsQueryHandler(_mediator.Object, _awardLogger.Object))
                .AddSingleton(_ => new FlyingClubMemberQueryHandler(_mediator.Object, _memberLogger.Object))
                .AddSingleton(_ => new TransactionDocumentQueryHandler(_mediator.Object, _transactionLogger.Object))
                .AddSingleton<IVouchersQueryHandler, VouchersQueryHandler>()
                .BuildServiceProvider();
        }

        private void SetupMediatorMocks()
        {

            _mediator
                .Setup(m => m.Send(
                    It.Is<CustomerTransactionRepoQuery>(q =>
                        q.DocType == ConfigReader.Get(EnvironmentSettings.AwardDocType)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>
                {
                    new
                    {
                        AwardNumber = "A001",
                        ProductId = "P1",
                        Status = "Available",
                        StartDate = DateTime.UtcNow.AddDays(-1),
                        ExpiryDate = DateTime.UtcNow.AddDays(30),
                        TransactionId = "TX001",
                        GiftedAwardDate = DateTime.UtcNow,
                        GiftedFromCustomerId = "BCD"
                    },
                     new
                    {
                        AwardNumber = "W001",
                        ProductId = "W1",
                        Status = "Available",
                        StartDate = DateTime.UtcNow.AddDays(-1),
                        ExpiryDate = DateTime.UtcNow.AddDays(30),
                        TransactionId = "TX001"
                    }
                });

            _mediator
                .Setup(m => m.Send(
                    It.Is<CustomerTransactionRepoQuery>(q =>
                        q.DocType == ConfigReader.Get(EnvironmentSettings.FlyingClubMemberDocType)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>
                {
                    new
                    {
                        DocType = "FlyingClubMember",
                        CustomerId = "12345",
                        MemberNumber = "1500283115",
                        MemberType = "Individual",
                        NameOnCard = "Test User",
                        TierCode = "Red",
                        Status = "Active",
                        CreatedOn = DateTime.UtcNow,
                        LastUpdatedOn = DateTime.UtcNow,
                        MemberAttributes = new List<object>(),
                        MemberShipCardDetails = new { }
                    }
                });

            _mediator
                .Setup(m => m.Send(
                    It.Is<CustomerTransactionRepoQuery>(q =>
                        q.DocType == ConfigReader.Get(EnvironmentSettings.TransactionDocType)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>
                {
                    new
                    {
                        DocType = "Transaction",
                        CustomerId = "12345",
                        MemberNumber = "1500283115",
                        PartnerId = "PR1",
                        TransactionId = "TX001",
                        StatusCode = "Completed",
                        TransactionDate = DateTime.UtcNow.ToString("o"),
                        CreatedOn = DateTime.UtcNow,
                        LastUpdatedOn = DateTime.UtcNow
                    }
                });
        }

        private IReferenceService CreateReferenceServiceMock()
        {
            var mock = new Mock<IReferenceService>();

            mock.Setup(x => x.GetAllProductsAsync())
                .ReturnsAsync(new List<Product>
                {
                    new Product
                    {
                        ProductId = "P1",
                        ProductName = "Test Voucher",
                        ProductType = "Electronic Voucher",
                        ProductDescription = "Test Description",
                        CategoryCode = "CAT1"
                    },
                    new Product
                    {
                        ProductId = "W1",
                        ProductName = "Top Member",
                        ProductType = "Award",
                        ProductDescription = "Top Member",
                        CategoryCode = "MEM"
                    }
                });

            mock.Setup(x => x.GetAllPartnersAsync())
                .ReturnsAsync(new List<Partner>
                {
                    new Partner
                    {
                        PartnerId = "PR1",
                        PartnerName = "Test Partner"
                    }
                });

            return mock.Object;
        }

        [Fact]
        public async Task GetVouchersByCustomerIdAsync_ShouldReturnVoucherList()
        {
            var handler = _serviceProvider.GetRequiredService<IVouchersQueryHandler>();

            var query = new VouchersQuery { CustomerId = "12345" };

            var result = await handler.GetVouchersByCustomerIdAsync(query);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value?.Value);

            var vouchers = result.Value.Value;
            Assert.Single(vouchers);

            var voucher = vouchers.First();

            Assert.Equal("Test Voucher", voucher.VoucherName);
            Assert.Equal("A001", voucher.VoucherNumber);
            Assert.Equal("Available", voucher.Status);
            Assert.Equal("Test Description", voucher.Description);
            Assert.Equal("Test Partner", voucher.Source);
            Assert.Equal("CAT1", voucher.Category);
            Assert.Equal(Constants.Application.WingsAvailable, voucher.MaxValue);
            Assert.True(voucher.IsGifted);
        }

        [Fact]
        public async Task GetVouchersByCustomerIdAsync_ShouldReturnFailure_WhenCustomerIdIsNull()
        {
            var handler = _serviceProvider.GetRequiredService<IVouchersQueryHandler>();

            var query = new VouchersQuery { CustomerId = null };

            var result = await handler.GetVouchersByCustomerIdAsync(query);

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task GetVouchersByCustomerIdAsync_ShouldReturnExpiredStatus_WhenExpiryDateIsPast()
        {

            _mediator
                .Setup(m => m.Send(
                    It.Is<CustomerTransactionRepoQuery>(q =>
                        q.DocType == ConfigReader.Get(EnvironmentSettings.AwardDocType)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>
                {
                    new
                    {
                        AwardNumber = "A002",
                        ProductId = "P1",
                        Status = Constants.Application.Available,
                        StartDate = DateTime.UtcNow.AddDays(-20),
                        ExpiryDate = DateTime.UtcNow.AddDays(-1), 
                        TransactionId = "TX001"
                    }
                });

            var handler = _serviceProvider.GetRequiredService<IVouchersQueryHandler>();

            var query = new VouchersQuery { CustomerId = "12345" };

            var result = await handler.GetVouchersByCustomerIdAsync(query);

            Assert.True(result.IsSuccess);

            var voucher = result.Value.Value.First();

            Assert.Equal(Constants.Application.Expired, voucher.Status);
        }
    }
}
