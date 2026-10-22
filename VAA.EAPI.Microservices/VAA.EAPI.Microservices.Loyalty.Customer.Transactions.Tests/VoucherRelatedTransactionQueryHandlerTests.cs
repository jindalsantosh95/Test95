using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Reflection;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class VoucherRelatedTransactionQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public VoucherRelatedTransactionQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
            Environment.SetEnvironmentVariable(Constants.EnvironmentSettings.TransactionRecentWindowHours, "1");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton(CreateCosmosFactoryMock().Object)
                .AddSingleton<AwardsQueryHandler>()
                .AddSingleton<TransactionDocumentQueryHandler>()
                .AddSingleton<IVoucherRelatedTransactionQueryHandler, VoucherRelatedTransactionQueryHandler>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddMediatR(config =>
                {
                    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                    config.RegisterServicesFromAssemblyContaining(typeof(AwardsQueryHandler));
                    config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQuery));
                })
                .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task VoucherRelatedHandler_ShouldReturnSuccess_When_RecentTransactionExists()
        {
            var handler = _serviceProvider.GetRequiredService<IVoucherRelatedTransactionQueryHandler>();

            var query = new VoucherRelatedTransactionQuery
            {
                CustomerId = "12345",
                VoucherNumber = "A001"
            };

            var result = await handler.GetVoucherRelatedTransactions(query);
            var transactions = result.Value.Value.TransactionDocuments;
            var txn = transactions.First();

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);
            Assert.NotEmpty(result.Value.Value.TransactionDocuments);
            Assert.Equal("TX001", txn.TransactionId);
            Assert.Equal("45678", txn.AwardId);
        }

        [Fact]
        public async Task ShouldReturnFailure_When_MandatoryFieldIsMissing()
        {
            var handler = _serviceProvider.GetRequiredService<IVoucherRelatedTransactionQueryHandler>();

            var query = new VoucherRelatedTransactionQuery
            {
                CustomerId = "12345",
                VoucherNumber = ""
            };

            var result = await handler.GetVoucherRelatedTransactions(query);

            Assert.True(result.IsFailure);
            Assert.Equal("VoucherNumber is required", result.Error);
        }

        [Fact]
        public async Task ShouldReturnFailure_When_AwardIdDoesNotMatchVoucherNumber()
        {
            var handler = _serviceProvider.GetRequiredService<IVoucherRelatedTransactionQueryHandler>();

            var query = new VoucherRelatedTransactionQuery
            {
                CustomerId = "12345",
                VoucherNumber = "A999"
            };

            var result = await handler.GetVoucherRelatedTransactions(query);

            Assert.True(result.IsFailure);
            Assert.Equal("No award found for voucher number", result.Error);
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
                    var queryText = string.Join("|", queryDefinition.GetQueryParameters().Select(p => p.Value?.ToString() ?? string.Empty));

                    if (queryText.Contains("Award"))
                        docs = new List<object>
                        {
                            new
                            {
                                AwardNumber = "A001",
                                AwardId = "45678",
                                CustomerId = "12345"
                            }
                        };

                    if (queryText.Contains("Transaction"))
                    {
                        docs = new List<object>
                        {
                            new
                            {
                                DocType = "Transaction",
                                CustomerId = "12345",
                                TransactionId = "TX001",
                                AwardId = "45678",
                                CreatedOn = DateTime.UtcNow,
                                LastUpdatedOn = DateTime.UtcNow,
                                StatusCode = Constants.Application.StatusCode,
                                CancelledTransactionId = "",
                            }
                        };
                    }

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
    }
}
