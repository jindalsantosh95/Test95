using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Application.Command;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Services;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Tests
{
    public class TransactionCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("ArcTransactionClientId", "abc");
            ConfigReader.Set("ArcTransactionClientSecret", "xxx");
            ConfigReader.Set("ArcTransactionTokenUrl", "https://tempuri.org/ArcTransactionToken");
            ConfigReader.Set("ArcTransactionAudience", "arcTransactionScope");
            ConfigReader.Set("TicketingSystemCode", "abcd");
            ConfigReader.Set("ARCEnvironmentName", "Test");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<ITransactionService, TransactionServiceHandler>()
                                                      .AddSingleton<TransactionCommandHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            ConfigReader.Set("ArcTransactionUrl", "https://tempuri.org/ArcTransaction");
            ConfigReader.Set("OrderApiRetryCount", "3");
            ConfigReader.Set("OrderRetryDelayMilliseconds", "1000");

            var handler = _serviceProvider.GetService<TransactionCommandHandler>()!;

            var request = TestDataProvider.GetTestData<TransactionRequest>("ARCPaymentMS\\TransactionRequest");

            var result = await handler.Handle(request);
            
            Assert.NotNull(result);

            Assert.Equal("SUCCESS", result.Status);
            Assert.Equal("5e7bb423-ff1b-4784-81cf-7daa5b39b0cd", result.RequestId);
        }

        [Fact]
        public async Task Handle_Error()
        {
            ConfigReader.Set("ArcTransactionUrl", "https://tempuri.org/ArcTransactionFailure");
            ConfigReader.Set("OrderApiRetryCount", "3");
            ConfigReader.Set("OrderRetryDelayMilliseconds", "1000");

            var handler = _serviceProvider.GetService<TransactionCommandHandler>()!;

            var request = TestDataProvider.GetTestData<TransactionRequest>("ARCPaymentMS\\TransactionRequest");

            var result = await handler.Handle(request);

            Assert.NotNull(result);

            Assert.Equal("ERROR", result.Status);
            Assert.Equal("TRANSACTION_REJECTED", result.errorDetails.First().Message);
        }
    }
}
