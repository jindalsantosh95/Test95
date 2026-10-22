using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Reflection;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class VoucherTxnsPnrUpdateCommandStatusQueryHandlerTest
    {
        private readonly IServiceProvider _serviceProvider;

        public VoucherTxnsPnrUpdateCommandStatusQueryHandlerTest()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ServiceResponseQueryHandler>()
                .AddSingleton<IVoucherTxnsPnrUpdateCommandStatusQuery, VoucherTxnsPnrUpdateCommandStatusQueryHandler>()
                .AddSingleton<ITableStorageService<ServiceResponseDatabaseEntity>>(_ =>
                 {
                     var mock = new Mock<ITableStorageService<ServiceResponseDatabaseEntity>>();

                     mock.Setup(x => x.RetrieveAsync(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(new ServiceResponseDatabaseEntity
                         {
                             PartitionKey = "TestPartition",
                             RowKey = "123",
                             Status = "Completed",
                             ResponseData = "{\"IsFailure\":false,\"IsSuccess\":true,\"Value\":{\"status\":4,\"value\":[{\"transactionId\":\"178\",\"transactionType\":\"Redemption\",\"awardId\":\"487\"}]}}"
                         });

                     return mock.Object;
                 })
                .AddMediatR(config =>
                {
                    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                })
                .BuildServiceProvider();
        }

        [Fact]
        public async Task VoucherTxnsPnrUpdateCommandStatusQueryHandler_SuccessScenario()
        {
            var handler = _serviceProvider.GetRequiredService<IVoucherTxnsPnrUpdateCommandStatusQuery>();

            var result = await handler.Handle(new VoucherTxnsPnrUpdateCommandStatusQuery { RequestId = "123" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);
        }
    }
}
