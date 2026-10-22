using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services.VoucherMonitoring;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class VoucherMonitoringServiceTests
    {
        private readonly IServiceProvider _serviceProvider;

        public VoucherMonitoringServiceTests()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<FakeCustomerTransactionDataProvider>()
                .AddSingleton<ICustomerTransactionDataProvider>(provider =>
                    provider.GetRequiredService<FakeCustomerTransactionDataProvider>())
                .AddSingleton<IReferenceService, FakeReferenceService>()
                .AddSingleton<IVoucherMonitoringService, VoucherMonitoringService>()
                .BuildServiceProvider();

        }

        [Fact]
        public async Task GetVoucherTransactionMonitoringStatus_ShouldReturnSuccess_WhenAllThreeTransactionsFound()
        {
            var service = _serviceProvider.GetRequiredService<IVoucherMonitoringService>();

            var result = await service.GetVoucherTransactionMonitoringStatus(
                new VoucherMonitoringRequest
                {
                    CustomerId = "C1",
                    MemberNumber = "M1",
                    VoucherNumbers = ["V001"]
                },
                CancellationToken.None);

            var voucher = Assert.Single(result.Vouchers);

            Assert.Equal("C1", result.CustomerId);
            Assert.Equal("V001", voucher.VoucherNumber);
            Assert.Equal(3, voucher.TransactionFound);
            Assert.Equal(VoucherTxnProcessingStatus.Success, voucher.TransactionProcessingStatus);
        }

        [Fact]
        public async Task GetVoucherTransactionMonitoringStatus_ShouldReturnFailed_WhenVoucherIsNotFound()
        {
            var service = _serviceProvider.GetRequiredService<IVoucherMonitoringService>();

            var result = await service.GetVoucherTransactionMonitoringStatus(
                new VoucherMonitoringRequest
                {
                    CustomerId = "C1",
                    MemberNumber = "M1",
                    VoucherNumbers = ["U009"]
                },
                CancellationToken.None);

            var voucher = Assert.Single(result.Vouchers);

            Assert.Equal(VoucherTxnProcessingStatus.Failed, voucher.TransactionProcessingStatus);
            Assert.Equal(0, voucher.TransactionFound);
            Assert.Equal("Voucher Not Found.", voucher.Summary);
        }

        private class FakeCustomerTransactionDataProvider : ICustomerTransactionDataProvider
        {
            public Task<IEnumerable<Award>> GetAwardsAsync(string customerId, CancellationToken cancellationToken)
            {
                return Task.FromResult<IEnumerable<Award>>(
                [
                    new Award
                    {
                        CustomerId = "C1",
                        AwardNumber = "V001",
                        AwardId = "A1",
                        Status = "Used"
                    }
                ]);
            }

            public Task<IEnumerable<TransactionDocument>> GetTransactionsAsync(string customerId, CancellationToken cancellationToken)
            {
                var now = DateTime.UtcNow;

                return Task.FromResult<IEnumerable<TransactionDocument>>(
                [
                    new TransactionDocument
                    {
                        CustomerId = "C1",
                        TransactionId = "TX-ACCRUAL",
                        AwardId = "A1",
                        ProductId = "P3",
                        TransactionType = "Accrual",
                        TransactionSubType = "Product",
                        TransactionValue = "500",
                        CreatedOn = now.AddSeconds(-2),
                        FlightDetails = new Flightdetails { ExternalReservationNumber = "BR1" }
                    },
                    new TransactionDocument
                    {
                        CustomerId = "C1",
                        TransactionId = "TX-ZERO",
                        AwardId = "A1",
                        ProductId = "P1",
                        TransactionType = "Redemption",
                        TransactionSubType = "Product",
                        TransactionValue = "0",
                        CreatedOn = now.AddSeconds(-1),
                        FlightDetails = new Flightdetails { ExternalReservationNumber = "BR1" }
                    },
                    new TransactionDocument
                    {
                        CustomerId = "C1",
                        TransactionId = "TX-MAIN",
                        AwardId = string.Empty,
                        ProductId = "P2",
                        TransactionType = "Redemption",
                        TransactionSubType = "Product",
                        TransactionValue = "500",
                        CreatedOn = now,
                        FlightDetails = new Flightdetails { ExternalReservationNumber = "BR1" }
                    }
                ]);
            }
        }

        private class FakeReferenceService : IReferenceService
        {
            public Task<IEnumerable<Product>> GetAllProductsAsync()
            {
                return Task.FromResult<IEnumerable<Product>>(
                [
                    new Product { ProductId = "P1", ProductName = "Test Product" },
                    new Product { ProductId = "P2", ProductName = "Flight Redemption" },
                    new Product { ProductId = "P3", ProductName = "Voucher Points" }
                ]);
            }

            public Task<IEnumerable<Partner>> GetAllPartnersAsync()
            {
                return Task.FromResult<IEnumerable<Partner>>(Array.Empty<Partner>());
            }
        }
    }
}
