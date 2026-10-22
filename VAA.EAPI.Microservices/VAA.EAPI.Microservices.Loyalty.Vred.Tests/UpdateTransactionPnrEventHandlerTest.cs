using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using System.Reflection;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Events;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Transaction;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class UpdateTransactionPnrEventHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdateTransactionPnrEventHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            VRedCommon.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<UpdateTransactionPnrEventHandler>()
                .AddSingleton<ITransactionDBRepository>(_ =>
                {
                    var dbMock = new Mock<ITransactionDBRepository>();

                    dbMock.Setup(r => r.AddTransactionAuditInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(true);

                    dbMock.Setup(r => r.UpdateVRedResponseAuditInfo(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(true);

                    dbMock.Setup(r => r.UpdateVRedRequestAuditInfo(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

                    return dbMock.Object;
                })
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<ITransactionDBServices, TransactionDBServices>()
                .AddSingleton<VredStatementsRepoHandler>()
                .AddSingleton<VredStatementServiceHandler>()
                .AddSingleton<ServiceResponseInsertCommandHandler>()
                .AddSingleton<ITokenHelper, TokenHelper>()
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
                .AddMediatR(config =>
                {
                    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                    config.RegisterServicesFromAssemblyContaining(typeof(VredStatementsServiceQuery));
                })
                .BuildServiceProvider();
        }

        [Fact]
        public async Task PnrUpdateHandler_Success()
        {
            var handler = _serviceProvider.GetRequiredService<UpdateTransactionPnrEventHandler>();

            var updatePnrReq = new UpdateTransactionPnrRequest
            {
                UpdatedBy = "testuser",
                MemberNumber = "210210",
                TransactionDetails =
                [
                    new UpdateTransactionPnrDetails
                    {
                        TransactionId = "AZ171630",
                        BookingReference = "PNR123",
                        TransactionType = "Accrual"
                    }
                ]
            };

            var result = await handler.Handle(new UpdateTransactionPnrEvent { Data = JToken.FromObject(updatePnrReq) }, System.Threading.CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task PnrUpdateHandler_Failure()
        {
            var handler = _serviceProvider.GetRequiredService<UpdateTransactionPnrEventHandler>();

            var updatePnrReq = new UpdateTransactionPnrRequest
            {
                UpdatedBy = "testuser",
                MemberNumber = "",
                TransactionDetails =
                [
                    new UpdateTransactionPnrDetails
                    {
                        TransactionId = "AZ171630",
                        BookingReference = "PNR123",
                        TransactionType = "Accrual"
                    }
                ]
            };

            var result = await handler.Handle(new UpdateTransactionPnrEvent { Data = JToken.FromObject(updatePnrReq) }, System.Threading.CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }
    }
}
