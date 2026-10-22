using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using CommonEnvironment = VAA.EAPI.GraphQL.Common.Constants.Environment;
using TestHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests;

public sealed class VoucherRedemptionProviderTests
{
    [Fact]
    public async Task RedeemAsync_UsesTheVRedLocationWithVRedCredentials()
    {
        using var mockDirectory = new CurrentDirectoryScope(AppContext.BaseDirectory);
        ConfigureSettings();
        var httpClientFactory = new TestHttpClientFactory();
        var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactory)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, TokenProvider>()
            .AddSingleton<VoucherRedemptionProvider>()
            .BuildServiceProvider()
            .GetRequiredService<VoucherRedemptionProvider>();

        await provider.RedeemAsync(new VoucherRedemptionRequest { CustomerId = "customer-123" });

        var requests = httpClientFactory.GetResponses("https://testresources.virginatlantic.com/customer-transactions")
            .Select(response => response.RequestMessage!)
            .ToArray();

        Assert.Equal(2, requests.Length);
        Assert.Equal(HttpMethod.Post, requests[0].Method);
        Assert.Equal("Bearer customer-transactions-token", requests[0].Headers.Authorization!.ToString());
        Assert.Equal(HttpMethod.Get, requests[1].Method);
        Assert.Equal("/VRED/CreateTransaction/response/transaction-123", requests[1].RequestUri!.AbsolutePath);
        Assert.Equal("Bearer vred-token", requests[1].Headers.Authorization!.ToString());
        Assert.Equal("vred-key", requests[1].Headers.GetValues(Application.OcpKeyHeader).Single());
    }

    [Fact]
    public async Task RollbackAsync_UsesTheVRedLocationWithVRedCredentials()
    {
        using var mockDirectory = new CurrentDirectoryScope(AppContext.BaseDirectory);
        ConfigureSettings();
        var httpClientFactory = new TestHttpClientFactory();
        var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactory)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, TokenProvider>()
            .AddSingleton<VoucherRedemptionProvider>()
            .BuildServiceProvider()
            .GetRequiredService<VoucherRedemptionProvider>();

        await provider.RollbackAsync(new VoucherRollbackRequest { CustomerId = "customer-123" });

        var requests = httpClientFactory.GetResponses("https://testresources.virginatlantic.com/customer-transactions")
            .Select(response => response.RequestMessage!)
            .ToArray();

        Assert.Equal(2, requests.Length);
        Assert.Equal(HttpMethod.Post, requests[0].Method);
        Assert.EndsWith("/rollbackVoucherRedemption", requests[0].RequestUri!.AbsolutePath);
        Assert.Equal("Bearer customer-transactions-token", requests[0].Headers.Authorization!.ToString());
        Assert.Equal(HttpMethod.Get, requests[1].Method);
        Assert.Equal("/VRED/CancelTransaction/response/transaction-456", requests[1].RequestUri!.AbsolutePath);
        Assert.Equal("Bearer vred-token", requests[1].Headers.Authorization!.ToString());
        Assert.Equal("vred-key", requests[1].Headers.GetValues(Application.OcpKeyHeader).Single());
    }

    [Fact]
    public async Task RedeemAsync_WhenVRedCallbackFails_ReportsTheAcceptedVoucherToTheCaller()
    {
        ConfigureSettings();
        var httpClientFactory = new FailingCallbackHttpClientFactory();
        var provider = new VoucherRedemptionProvider(
            new HttpConnector(new LoggerFactory().CreateLogger<HttpConnector>(), httpClientFactory),
            new TokenProvider());
        var request = new VoucherRedemptionRequest
        {
            CustomerId = "customer-123",
            MemberNumber = "member-123",
            InitiatedBy = "EAPI",
            VoucherDetails = new VoucherDetails { VoucherNumber = "voucher-123" }
        };

        var exception = await Assert.ThrowsAsync<VoucherRedemptionAcceptedException>(() => provider.RedeemAsync(request));

        Assert.Equal("voucher-123", exception.VoucherId);
        Assert.DoesNotContain(httpClientFactory.Requests, request =>
            request.Method == HttpMethod.Post && request.RequestUri!.AbsolutePath.EndsWith("rollbackVoucherRedemption"));
    }

    [Fact]
    public async Task RedeemAsync_WhenAcceptedResponseHasNoCallbackLocation_ReportsTheAcceptedVoucherToTheCaller()
    {
        ConfigureSettings();
        var provider = new VoucherRedemptionProvider(
            new HttpConnector(new LoggerFactory().CreateLogger<HttpConnector>(), new MissingCallbackLocationHttpClientFactory()),
            new TokenProvider());

        var exception = await Assert.ThrowsAsync<VoucherRedemptionAcceptedException>(() => provider.RedeemAsync(new VoucherRedemptionRequest
        {
            CustomerId = "customer-123",
            VoucherDetails = new VoucherDetails { VoucherNumber = "voucher-123" }
        }));

        Assert.Equal("voucher-123", exception.VoucherId);
    }

    private static void ConfigureSettings()
    {
        ConfigReader.Set(CommonEnvironment.CustTransAppId, "customer-transactions-app");
        ConfigReader.Set(CommonEnvironment.CustTransOcpKey, "customer-transactions-key");
        ConfigReader.Set(CommonEnvironment.CustTransactionMsUrl, "https://testresources.virginatlantic.com/customer-transactions");
        ConfigReader.Set(CommonEnvironment.VRedAppId, "vred-app");
        ConfigReader.Set(CommonEnvironment.VRedOcpSubID, "vred-key");
        ConfigReader.Set(CommonEnvironment.VredMsUrl, "https://testresources.virginatlantic.com/vred");
    }

    private sealed class TokenProvider : IOAuthTokenProvider
    {
        public Task<string> AcquireTokenAsync(string resource) => Task.FromResult(resource switch
        {
            "customer-transactions-app" => "customer-transactions-token",
            "vred-app" => "vred-token",
            _ => throw new InvalidOperationException($"Unexpected token resource: {resource}")
        });
    }

    private sealed class CurrentDirectoryScope : IDisposable
    {
        private readonly string _previousDirectory = Directory.GetCurrentDirectory();

        public CurrentDirectoryScope(string directory) => Directory.SetCurrentDirectory(directory);

        public void Dispose() => Directory.SetCurrentDirectory(_previousDirectory);
    }

    private sealed class FailingCallbackHttpClientFactory : IHttpClientFactory
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        public HttpClient CreateClient(string name) => new(new FailingCallbackHandler(Requests));
    }

    private sealed class MissingCallbackLocationHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new MissingCallbackLocationHandler());
    }

    private sealed class MissingCallbackLocationHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted) { RequestMessage = request });
        }
    }

    private sealed class FailingCallbackHandler(List<HttpRequestMessage> requests) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requests.Add(request);

            if (request.Method == HttpMethod.Get)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway) { RequestMessage = request });

            if (request.RequestUri!.AbsolutePath.EndsWith("redeemvoucher"))
            {
                var response = new HttpResponseMessage(HttpStatusCode.Accepted) { RequestMessage = request };
                response.Headers.Location = new Uri("https://testresources.virginatlantic.com/VRED/CreateTransaction/response/transaction-123");
                return Task.FromResult(response);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request });
        }
    }
}
