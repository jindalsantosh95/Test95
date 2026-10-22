using MediatR;
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
    public class RedeemVoucherCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public RedeemVoucherCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<IMessageBus>(azureServiceBusFactory.GetClient("testConnectionString", "testSource"))
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<IAzureServiceBusTopicClient, AzureServiceBusTopicClient>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<IRedeemVoucherCommandHandler, RedeemVoucherCommandHandler>()
                .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task RedeemVoucherCommandHandler_ShouldReturnSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<IRedeemVoucherCommandHandler>();

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
                    VoucherName = "Test Voucher",
                    VoucherNumber = "A001"
                }
            };

            var createCommand = new RedeemVoucherCommand(redeemVoucherReq);

            var result = await handler.Handle(createCommand, CancellationToken.None);

            Assert.NotNull(result.Value);
            Assert.False(result.IsFailure);
            Assert.True(result.IsSuccess);
            Assert.Contains(result.Value.HeaderParameters, x => x.Key == "location");
        }

        [Fact]
        public async Task RedeemVoucherCommandHandler_ShouldReturnFailure()
        {
            var handler = _serviceProvider.GetRequiredService<IRedeemVoucherCommandHandler>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null, CancellationToken.None));
        }
    }
}
