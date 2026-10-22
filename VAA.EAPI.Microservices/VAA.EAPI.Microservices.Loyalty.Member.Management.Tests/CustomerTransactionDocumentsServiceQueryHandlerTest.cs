using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs;
using Xunit;
using CoreXunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class CustomerTransactionDocumentsServiceQueryHandlerTest
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomerTransactionDocumentsServiceQueryHandlerTest()
        {
            ConfigReader.Set(
                VAA.EAPI.Common.Constants.Environment.EapiMsClientID,
                "customer-transaction-test-client-id");
            ConfigReader.Set(
                VAA.EAPI.Common.Constants.Environment.EapiMSSecret,
                "customer-transaction-test-client-secret");
            ConfigReader.Set(
                VAA.EAPI.Common.Constants.Environment.AADAuthority,
                "https://tempuri.org/");
            ConfigReader.Set(
                VAA.EAPI.Common.Constants.Environment.AADTenant,
                "customer-transaction-test-tenant");
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.CustomerTxnMSDetailsEndpoint,
                "https://tempuri.org/customer/{0}/documents/{1}");
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.CustomerTxnMSResourceId,
                "wings-active");
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.LoyaltyMbrMgmtOcpKey,
                "customer-transaction-test-subscription-key");

            VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                .Application.LoyaltyMbrMgmtOcpKeyCached =
                "customer-transaction-test-subscription-key";

            AsyncContext.Current = new AsyncContext
            {
                Context = new TestFunctionContext()
            };

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<CoreXunitHttpClientFactory>()
                .AddSingleton<IHttpClientFactory>(serviceProvider =>
                    serviceProvider.GetRequiredService<CoreXunitHttpClientFactory>())
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<
                    IOAuthTokenProvider,
                    VAA.EAPI.Core.Xunit.Azure.OAuthTokenProvider>()
                .AddSingleton<
                    ICustomerTransactionTokenProvider,
                    CustomerTransactionTokenProvider>()
                .AddSingleton<CustomerTransactionDocumentsServiceQueryHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public void ConstructorShouldSucceed()
        {
            Assert.NotNull(_serviceProvider.GetRequiredService<
                CustomerTransactionDocumentsServiceQueryHandler>());
        }

        [Fact]
        public async Task Handle_ShouldReturnTransactionDocuments()
        {
            var handler = _serviceProvider.GetRequiredService<
                CustomerTransactionDocumentsServiceQueryHandler>();

            var result = await handler.Handle(
                new CustomerTransactionDocumentsServiceQuery
                {
                    CustomerId = "customer123"
                },
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value.Value);
        }
    }
}
