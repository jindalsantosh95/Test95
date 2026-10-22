using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using System.Reflection;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class PatchVoucherRelatedTxnsPnrEventHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;


        public PatchVoucherRelatedTxnsPnrEventHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            ContactNoteTestHelpers.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<ServiceResponseInsertCommandHandler>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<IPatchVoucherRelatedTxnsPnrEventHandler, PatchVoucherRelatedTxnsPnrEventHandler>()
                .AddSingleton<TransactionUpdateEventHandler>()
                .AddSingleton<TransactionCommandStatusQueryHandler>()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<ITableStorageService<ServiceResponseDatabaseEntity>>(_ =>
                {
                    var mock = new Mock<ITableStorageService<ServiceResponseDatabaseEntity>>();

                    mock.Setup(x => x.RetrieveAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(new ServiceResponseDatabaseEntity
                        {
                            PartitionKey = "TestPartition",
                            RowKey = "TestRow",
                            Status = "Completed",
                            ResponseData = "{\"IsFailure\":false,\"IsSuccess\":true,\"Value\":{\"status\":4,\"value\":{\"document\":{\"customerId\":\"12345\",\"createdOn\":\"07-07-2026\",\"createdBy\":\"test\",\"lastUpdatedOn\":\"07-07-2026\",\"lastUpdatedBy\":\"test\",\"memberNumber\":\"150\",\"transactionValue\":\"10000\",\"partnerId\":\"1-1\",\"transactionDate\":\"07-07-2026\",\"productId\":\"171\",\"transactionId\":\"1783\",\"statusCode\":\"Processed\",\"transactionSubType\":\"Product\",\"transactionChannel\":\"TEST\",\"activityDate\":\"07-07-2026\",\"transactionType\":\"Accrual\",\"statementText\":\"test\"}}}}"
                        });

                    return mock.Object;
                })
                .AddSingleton<IVoucherRelatedTransactionQueryHandler>(_ =>
                {
                    var mock = new Mock<IVoucherRelatedTransactionQueryHandler>();

                    mock.Setup(x => x.GetVoucherRelatedTransactions(It.IsAny<VoucherRelatedTransactionQuery>()))
                        .ReturnsAsync(new EAPI.Domain.Entities.Common.Response<TransactionResponse>
                        {
                            Status = Status.OK,
                            Value = new TransactionResponse
                            {
                                TransactionDocuments = new List<TransactionDocument>
                                {
                                    new TransactionDocument
                                    {
                                        TransactionId = "TX001",
                                        CustomerId = "12345",
                                        TransactionType = "Accrual",
                                        StatusCode = "Processed",
                                        TransactionDate = DateTime.UtcNow.AddMinutes(-30).ToString(),
                                        AwardId = "45678",
                                        FlightDetails = new ()
                                        {
                                            ExternalReservationNumber = "TEST"
                                        }
                                    }
                                }
                            }
                        });

                    return mock.Object;
                }
                )
                .AddMediatR(config =>
                {
                    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                    config.RegisterServicesFromAssemblyContaining(typeof(ServiceResponseInsertCommandHandler));
                })
                .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task PatchVoucherRelatedTxnsEventHandler_ShouldReturnSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<IPatchVoucherRelatedTxnsPnrEventHandler>();

            var updateReq = new PatchVoucherRelatedTxnsPnrRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                PNR = "TXTX",
                InitiatedBy = "TestUser",
                VoucherNumbers = ["A001"]
            };

            var patchEvent = new PatchVoucherRelatedTxnsPnrEvent { Id = "123", Data = JToken.FromObject(updateReq) };

            var result = await handler.Handle(patchEvent, CancellationToken.None);
            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task PatchVoucherRelatedTxnsEventHandler_MissingMandatoryAttribute()
        {
            var handler = _serviceProvider.GetRequiredService<IPatchVoucherRelatedTxnsPnrEventHandler>();

            var updateReq = new PatchVoucherRelatedTxnsPnrRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                PNR = "TEST",
                InitiatedBy = null,
                VoucherNumbers = ["A001"]
            };

            var patchEvent = new PatchVoucherRelatedTxnsPnrEvent { Id = "123", Data = JToken.FromObject(updateReq) };

            var result = await handler.Handle(patchEvent, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
        }
    }
}
