using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class PatchVoucherRelatedTxnsPnrCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public PatchVoucherRelatedTxnsPnrCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<IAzureServiceBusTopicClient, AzureServiceBusTopicClient>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<IPatchVoucherRelatedTxnsPnrCommandHandler, PatchVoucherRelatedTxnsPnrCommandHandler>()
                .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task PatchVoucherRelatedTxnsPnrCommandHandler_ShouldReturnSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<IPatchVoucherRelatedTxnsPnrCommandHandler>();

            var updateReq = new PatchVoucherRelatedTxnsPnrRequest
            {
                CustomerId = "12345",
                MemberNumber = "1500283115",
                PNR = "TEST",
                InitiatedBy = "TestUser",
                VoucherNumbers = ["A001"]
            };

            var updateCommand = new PatchVoucherRelatedTxnsPnrCommand(updateReq);

            var result = await handler.Handle(updateCommand, CancellationToken.None);

            Assert.NotNull(result.Value);
            Assert.False(result.IsFailure);
            Assert.True(result.IsSuccess);
            Assert.Contains(result.Value.HeaderParameters, x => x.Key == "location");
        }

        [Fact]
        public async Task PatchVoucherRelatedTxnsPnrCommandHandler_ShouldReturnFailure()
        {
            var handler = _serviceProvider.GetRequiredService<IPatchVoucherRelatedTxnsPnrCommandHandler>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null, CancellationToken.None));
        }
    }
}
