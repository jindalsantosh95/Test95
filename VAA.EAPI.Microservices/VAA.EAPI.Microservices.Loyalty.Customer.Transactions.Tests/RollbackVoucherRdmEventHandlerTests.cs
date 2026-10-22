using Microsoft.Azure.Cosmos;
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
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class RollbackVoucherRdmEventHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;


        public RollbackVoucherRdmEventHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            ContactNoteTestHelpers.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton(CreateCosmosFactoryMock().Object)
                .AddSingleton<ServiceResponseInsertCommandHandler>()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<IRollbackVoucherRdmEventHandler, RollbackVoucherRdmEventHandler>()
                .AddSingleton<TransactionCancelEventHandler>()
                .AddSingleton<TransactionCommandStatusQueryHandler>()
                .AddSingleton(typeof(ITableStorageService<ReferenceDetails>), typeof(Core.Xunit.Azure.TableStorageService<ReferenceDetails>))
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
                .AddSingleton<ReferenceCacheHelper>()
                .AddSingleton<IReferenceService, ReferenceService>()
                .AddSingleton<AwardsQueryHandler>()
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
                                        AwardId = "45678"
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
                    config.RegisterServicesFromAssemblyContaining(typeof(AwardsQueryHandler));
                    config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQuery));
                })
                .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
            InsertRefDataTableEntity();
        }

        [Fact]
        public async Task RollbackVoucherRdmEventHandler_ShouldReturnSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<IRollbackVoucherRdmEventHandler>();

            var rollbackReq = new RollbackVoucherRdmRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                RollbackSource = "TEST",
                RollbackInitiatedBy = "TestUser",
                VoucherNumber = "A001"
            };

            var cancelEvent = new RollbackVoucherRdmEvent { Id = "123", Data = JToken.FromObject(rollbackReq) };

            var result = await handler.Handle(cancelEvent, CancellationToken.None);
            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task RollbackVoucherRdmEventHandler_MissingMandatoryAttribute()
        {
            var handler = _serviceProvider.GetRequiredService<IRollbackVoucherRdmEventHandler>();

            var rollbackReq = new RollbackVoucherRdmRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                RollbackSource = "TEST",
                RollbackInitiatedBy = "TestUser",
            };

            var cancelEvent = new RollbackVoucherRdmEvent { Id = "123", Data = JToken.FromObject(rollbackReq) };

            var result = await handler.Handle(cancelEvent, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
        }

        [Fact]
        public async Task RollbackVoucherRdmEventHandler_VoucherValidationFailure()
        {
            var handler = _serviceProvider.GetRequiredService<IRollbackVoucherRdmEventHandler>();

            var rollbackReq = new RollbackVoucherRdmRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                RollbackSource = "TEST",
                RollbackInitiatedBy = "TestUser",
                VoucherNumber = "A0011"
            };

            var cancelEvent = new RollbackVoucherRdmEvent { Id = "123", Data = JToken.FromObject(rollbackReq) };

            var result = await handler.Handle(cancelEvent, CancellationToken.None);
            Assert.True(result.IsFailure);
            Assert.Equal("Voucher Number not found", result.Error);
        }

        private Mock<ICosmosClientFactory> CreateCosmosFactoryMock()
        {
            var containerMock = new Mock<Container>();

            containerMock
                .Setup(x => x.GetItemQueryIterator<object>(
                    It.IsAny<QueryDefinition>(),
                    It.IsAny<string?>(),
                    It.IsAny<QueryRequestOptions>()))
                .Returns((QueryDefinition queryDefinition, string continuationToken, QueryRequestOptions options) =>
                {
                    List<object> docs = new List<object>();
                    var queryText = queryDefinition.GetQueryParameters().LastOrDefault().Value?.ToString() ?? string.Empty;

                    if (queryText.Contains("Award"))
                        docs = [
                            new
                            {
                                AwardNumber = "A001",
                                ProductId = "P1",
                                Status = "Available",
                                StartDate = DateTime.UtcNow.AddDays(-1),
                                ExpiryDate = DateTime.UtcNow.AddDays(30),
                                TransactionId = "TX001",
                                CustomerId = "12345",
                                AwardId = "45678"
                            }];

                    if (queryText.Contains("Transaction"))
                        docs = [
                            new
                            {
                                DocType = "Transaction",
                                CustomerId = "12345",
                                MemberNumber = "1500283115",
                                PartnerId = "PR1",
                                TransactionId = "TX001",
                                StatusCode = "Processed",
                                TransactionDate = DateTime.UtcNow.ToString("o"),
                                CreatedOn = DateTime.UtcNow,
                                LastUpdatedOn = DateTime.UtcNow,
                                AwardId = "45678",
                                TransactionType = "Accrual",
                                ProductId = "P22"
                            }];


                    var responseMock = new Mock<FeedResponse<object>>();

                    responseMock.Setup(x => x.GetEnumerator())
                        .Returns(() => docs.GetEnumerator());

                    responseMock.Setup(x => x.Resource).Returns(docs);

                    var iteratorMock = new Mock<FeedIterator<object>>();

                    iteratorMock.SetupSequence(x => x.HasMoreResults)
                    .Returns(true)
                    .Returns(false);


                    iteratorMock
                        .Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(responseMock.Object);

                    return iteratorMock.Object;
                });

            var clientMock = new Mock<CosmosClient>();

            clientMock
                .Setup(x => x.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(containerMock.Object);

            var cosmosClientFactory = new Mock<ICosmosClientFactory>();

            cosmosClientFactory
                .Setup(x => x.GetClient(It.IsAny<string>(), null))
                .Returns(clientMock.Object);

            return cosmosClientFactory;
        }

        private void InsertRefDataTableEntity()
        {
            var tableService = _serviceProvider.GetService<ITableStorageService<ReferenceDetails>>()!;

            List<ReferenceDetails> refList = [
                new ReferenceDetails {
                    PartitionKey = "Product",
                    RowKey = "P1",
                    ProductName = "Test Voucher",
                    ProductType = "Electronic Voucher",
                    ProductDescription = "Test Description",
                    ProductCategory = "Electronic Voucher"
                },
                new ReferenceDetails{
                    PartitionKey = "Product",
                    RowKey = "P22",
                    ProductName = "Voucher Points",
                    ProductType = "Electronic Voucher",
                    ProductDescription = "Test Description",
                    ProductCategory = "Electronic Voucher"
                },
                new ReferenceDetails{
                    PartitionKey = "Partner",
                    RowKey = "PR1",
                    PartnerName = "Test Partner",
                    PartnerCode = "PR",
                    PartnerType = "Airline"
                }];

            foreach (var entity in refList)
            {
                tableService.InsertOrMergeAsync(entity).GetAwaiter().GetResult();
            }
        }
    }
}
