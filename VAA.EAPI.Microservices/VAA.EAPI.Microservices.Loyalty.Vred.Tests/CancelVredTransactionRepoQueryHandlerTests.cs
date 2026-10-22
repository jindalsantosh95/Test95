using Microsoft.Extensions.DependencyInjection;
using Moq;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Transaction;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class CancelVredTransactionRepoQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CancelVredTransactionRepoQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            VRedCommon.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<CancelVredTransactionRepoQueryHandler>()
                .AddSingleton<IUpdateVredTransactionServiceHandler>(_ =>
                {
                    var updateService = new Mock<IUpdateVredTransactionServiceHandler>();

                    updateService.Setup(h => h.Handle(It.IsAny<UpdateVredTransactionService>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new UpdateTransactionResponse
                        {
                            SuccessResponse = new TransactionResponse
                            {
                                PartnerTransactionId = "AZ171630",
                                PartnerCode = "VAA"
                            },
                            ErrorResponse = null
                        });

                    return updateService.Object;
                })
                .BuildServiceProvider();
        }

        [Fact]
        public void RepoHandler_Success()
        {
            var handler = _serviceProvider.GetRequiredService<CancelVredTransactionRepoQueryHandler>();

            var CancelVredTransactionData = new CancelVredTransactionRequestData
            {
                TransactionId = "AZ171630",
                MemberNumber = "210210",
            };

            var result = handler.Handle(new CancelVredTransactionRepoQuery(CancelVredTransactionData), System.Threading.CancellationToken.None);

            Assert.NotNull(result.Result.SuccessResponse);
            Assert.Equal("AZ171630", result.Result.SuccessResponse.PartnerTransactionId);
            Assert.Equal("VAA", result.Result.SuccessResponse.PartnerCode);
        }
    }
}
