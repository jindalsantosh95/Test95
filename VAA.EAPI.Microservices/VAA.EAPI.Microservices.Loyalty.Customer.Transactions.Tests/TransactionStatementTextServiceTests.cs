using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TransactionStatementTextServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _provider;

        public TransactionStatementTextServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();

            _provider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ReferenceCacheHelper>()
                .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                .AddSingleton<ITransactionStatementTextService, TransactionStatementTextService>()
                .BuildServiceProvider();

            var storageAccountUrl = ConfigReader.Get("BlobConnStringForStatementTxt");
            var container = ConfigReader.Get("ContainerNameForStatementTxt");
            var name = "StatementTextValues.json";
            var sasKey = ConfigReader.Get("LoyaltyMemberMgmtStorageSASKey");
            var azureBlobService = _provider.GetService<IAzureBlobService>()!;
            var blobUri = Core.Xunit.Azure.AzureBlobService.CreateBlobUri(storageAccountUrl, container, name, sasKey);

            azureBlobService.DownloadBlobAsync(blobUri, name, CancellationToken.None)
                                .Wait();
        }

        [Fact]
        public async Task GetTransactionStatementText_shouldreturnstmttext()
        {
            var service = _provider.GetRequiredService<ITransactionStatementTextService>();

            var request = new StatementTextRequest
            {
                PartnerCode = "VS",
                TransactionType = "Accrual",
                ProductName = "Voucher Points",
                TransactionSubType = "Product"
            };

            var result = await service.GetTransactionStatementText(request, CancellationToken.None);

            Assert.Equal("Point credit for reward voucher redemption", result);
        }

        [Fact]
        public async Task GetTransactionStatementText_shouldthorwexception()
        {
            var service = _provider.GetRequiredService<ITransactionStatementTextService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetTransactionStatementText(null, CancellationToken.None));
        }
    }
}
