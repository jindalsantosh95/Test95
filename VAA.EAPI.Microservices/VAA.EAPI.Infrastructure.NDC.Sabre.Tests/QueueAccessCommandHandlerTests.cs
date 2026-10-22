using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Tests;

public sealed class QueueAccessCommandHandlerTests
{
    private readonly VAA.EAPI.Core.Xunit.Http.HttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;

    public QueueAccessCommandHandlerTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>()
            .AddSingleton<IHttpClientFactory>(services => services.GetRequiredService<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>())
            .AddSingleton<IHttpConnector, HttpConnector>()
            .BuildServiceProvider();

        _httpClientFactory = _serviceProvider.GetRequiredService<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>();
    }

    [Fact]
    public async Task GetAsync_ReturnsMockedQueueAccessResponse()
    {
        var httpConnector = _serviceProvider.GetRequiredService<IHttpConnector>();
        var responseCount = _httpClientFactory.GetResponses("https://tempuri.org").Count;

        var response = await httpConnector.GetAsync("https://tempuri.org/queue-access", null, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Queue access approved", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var capturedResponse = Assert.Single(_httpClientFactory.GetResponses("https://tempuri.org").Skip(responseCount));
        Assert.Equal(HttpMethod.Get, capturedResponse.RequestMessage!.Method);
        Assert.Equal("https://tempuri.org/queue-access", capturedResponse.RequestMessage.RequestUri!.ToString());
    }
}
