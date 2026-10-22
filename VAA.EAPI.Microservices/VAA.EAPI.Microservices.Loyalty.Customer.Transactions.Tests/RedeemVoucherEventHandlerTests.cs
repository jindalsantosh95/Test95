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
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class RedeemVoucherEventHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;


        public RedeemVoucherEventHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            ContactNoteTestHelpers.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton(CreateCosmosFactoryMock().Object)
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<ServiceResponseInsertCommandHandler>()
                .AddSingleton<IRedeemVoucherEventHandler, RedeemVoucherEventHandler>()
                .AddSingleton<TransactionCreateEventHandler>()
                .AddSingleton<TransactionCommandStatusQueryHandler>()
                .AddSingleton<ReferenceCacheHelper>()
                .AddSingleton<IReferenceService, ReferenceService>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<AwardsQueryHandler>()
                .AddSingleton<FlyingClubMemberQueryHandler>()
                .AddSingleton<TransactionDocumentQueryHandler>()
                .AddSingleton<ITransactionStatementTextService>(_ =>
                {
                    var txnStmtService = new Mock<ITransactionStatementTextService>();

                    txnStmtService.Setup(x => x.GetTransactionStatementText(It.IsAny<StatementTextRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync("Test Statement");

                    return txnStmtService.Object;
                })
                .AddMediatR(config =>
                {
                    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                    config.RegisterServicesFromAssemblyContaining(typeof(AwardsQueryHandler));
                    config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQuery));
                })
                .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
            InsertTableEntity();
        }

        [Fact]
        public async Task RedeemVoucherHandler_ShouldReturnSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<IRedeemVoucherEventHandler>();

            var redeemVoucherReq = new RedeemVoucherRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                TransactionChannel = "TEST",
                InitiatedBy = "TestUser",
                RedemptionType = "Test Voucher",
                PartnerCode = "PR",
                FlightDetails = new RedeemVoucherFlightDetails
                {
                    ActivityDate = DateTime.Now.Date,
                    BookingClass = "J",
                    OriginAirport = "LHR",
                    DestinationAirport = "SFO",
                    BookingReference = "EFEGGG"
                },
                VoucherDetails = new VoucherDetails
                {
                    PointsToAward = 1000,
                    VoucherName = "Test Voucher",
                    VoucherNumber = "A001"
                }
            };

            var createEvent = new RedeemVoucherEvent { Data = JToken.FromObject(redeemVoucherReq) };

            var result = await handler.Handle(createEvent, CancellationToken.None);
            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value.TransactionId);
        }

        [Fact]
        public async Task RedeemVoucherHandler_MissingMandatoryAttribute()
        {
            var handler = _serviceProvider.GetRequiredService<IRedeemVoucherEventHandler>();
            var redeemVoucherReq = new RedeemVoucherRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                TransactionChannel = "TEST",
                InitiatedBy = "TestUser",
                PartnerCode = "PR",
                RedemptionType = "Test Voucher",
                FlightDetails = new RedeemVoucherFlightDetails
                {
                    ActivityDate = DateTime.Now.Date,
                    BookingClass = "J",
                    OriginAirport = "LHR",
                    DestinationAirport = "SFO",
                    BookingReference = "EFEGGG"
                },
                VoucherDetails = new VoucherDetails
                {
                    VoucherName = "Test Voucher",
                    VoucherNumber = "A001"
                }
            };

            var createEvent = new RedeemVoucherEvent { Data = JToken.FromObject(redeemVoucherReq) };

            var result = await handler.Handle(createEvent, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
        }

        [Fact]
        public async Task RedeemVoucherHandler_VoucherValidationFailure()
        {
            var handler = _serviceProvider.GetRequiredService<IRedeemVoucherEventHandler>();
            var redeemVoucherReq = new RedeemVoucherRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                TransactionChannel = "TEST",
                InitiatedBy = "TestUser",
                PartnerCode = "PR",
                RedemptionType = "TP",
                FlightDetails = new RedeemVoucherFlightDetails
                {
                    ActivityDate = DateTime.Now.Date,
                    BookingClass = "J",
                    OriginAirport = "LHR",
                    DestinationAirport = "SFO",
                    BookingReference = "EFEGGG"
                },
                VoucherDetails = new VoucherDetails
                {
                    VoucherName = "Test Voucher",
                    VoucherNumber = "A0010",
                    PointsToAward = 1000
                }
            };

            var createEvent = new RedeemVoucherEvent { Data = JToken.FromObject(redeemVoucherReq) };

            var result = await handler.Handle(createEvent, CancellationToken.None);
            Assert.True(result.IsFailure);
            Assert.Equal("Voucher Number not found", result.Error);
        }

        [Fact]
        public async Task RedeemVoucherHandler_InvalidInput()
        {
            var handler = _serviceProvider.GetRequiredService<IRedeemVoucherEventHandler>();
            var redeemVoucherReq = new RedeemVoucherRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                TransactionChannel = "TEST",
                InitiatedBy = "TestUser",
                PartnerCode = "PR",
                FlightDetails = new RedeemVoucherFlightDetails
                {
                    ActivityDate = DateTime.Now.Date,
                    BookingClass = "J",
                    OriginAirport = "LHR",
                    DestinationAirport = "SFO",
                    BookingReference = "EFEGGG"
                },
                VoucherDetails = new VoucherDetails
                {
                    PointsToAward = 1000,
                    VoucherName = "Test Voucher",
                    VoucherNumber = "A001"
                }
            };

            var createEvent = new RedeemVoucherEvent { Data = null };

            var result = await handler.Handle(createEvent, CancellationToken.None);
            Assert.True(result.IsFailure);
            Assert.Equal("Redeem Voucher request payload is missing.", result.Error);
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

                    if (queryText.Contains("FlyingClubMember"))
                        docs = [
                            new
                            {
                                DocType = "FlyingClubMember",
                                CustomerId = "12345",
                                MemberNumber = "1500283115",
                                MemberType = "Individual",
                                NameOnCard = "Test User",
                                TierCode = "Gold",
                                Status = "Active",
                                CreatedOn = DateTime.UtcNow,
                                LastUpdatedOn = DateTime.UtcNow,
                                MemberAttributes = new List<object>(),
                                MemberShipCardDetails = new { }
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
                                LastUpdatedOn = DateTime.UtcNow
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

        private void InsertTableEntity()
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