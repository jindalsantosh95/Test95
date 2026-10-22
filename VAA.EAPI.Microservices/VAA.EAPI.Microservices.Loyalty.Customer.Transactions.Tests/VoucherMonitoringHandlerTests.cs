using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.VoucherMonitoring;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services.VoucherMonitoring;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class VoucherMonitoringHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public VoucherMonitoringHandlerTests()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IVoucherMonitoringService, VoucherMonitoringServiceFake>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<IVoucherMonitoringHandler, VoucherMonitoringHandler>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task VoucherMonitoringHandler_ShouldReturnSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<IVoucherMonitoringHandler>();

            var result = await handler.Handle(
                new VoucherMonitoringEvent
                {
                    Id = "123",
                    Data = JToken.FromObject(new VoucherMonitoringRequest
                    {
                        CustomerId = "C1",
                        MemberNumber = "M1",
                        VoucherNumbers = ["V001"]
                    })
                },
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Equal(3, result.Value.Value.Vouchers.First().TransactionFound);
        }

        [Fact]
        public async Task VoucherMonitoringHandler_ShouldReturnFailure_WhenEventDataIsMissing()
        {
            var handler = _serviceProvider.GetRequiredService<IVoucherMonitoringHandler>();

            var result = await handler.Handle(new VoucherMonitoringEvent { Id = "123" }, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal("Voucher monitoring processing failed.", result.Error);
        }

        private class VoucherMonitoringServiceFake : IVoucherMonitoringService
        {
            public Task<VoucherMonitoringResponse> GetVoucherTransactionMonitoringStatus(
                VoucherMonitoringRequest request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new VoucherMonitoringResponse
                {
                    CustomerId = request.CustomerId,
                    MemberNumber = request.MemberNumber,
                    Vouchers = request.VoucherNumbers.Select(vn => new VoucherResult
                    {
                        VoucherNumber = "V001",
                        TransactionFound = 3
                    })
                });
            }
        }
    }
}
