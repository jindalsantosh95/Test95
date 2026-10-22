using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Services.IFG;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Tests;

public class PaymentOrderExchangeCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentOrderExchangeCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        ConfigReader.Set("IFGUsername", "test-user");
        ConfigReader.Set("IFGPassword", "test-password");
        ConfigReader.Set("IFGEnvironment", "test");
        ConfigReader.Set("IFGExchangeRequestUrl", "https://tempuri.org/ifg/exchange");

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<PaymentOrderExchangeCommandHandler>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        var handler = _serviceProvider.GetRequiredService<PaymentOrderExchangeCommandHandler>();

        var result = await handler.Handle(new PaymentAuthorizeRequest { TrackID = "test-track" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Status.OK, result.Value.Status);
        Assert.Equal("EXCHANGE", result.Value.Value.Status);
    }
}
