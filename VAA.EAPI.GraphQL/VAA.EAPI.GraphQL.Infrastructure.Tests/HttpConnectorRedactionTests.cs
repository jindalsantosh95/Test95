using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Infrastructure.Tests;

public sealed class HttpConnectorRedactionTests
{
    [Fact]
    public async Task SendAsync_RedactsSensitiveAccountLinkingFailurePayloads()
    {
        var connector = CreateConnector();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => connector.SendAsync<object>(
            "https://connector.test/account-linking-proof",
            [],
            "token",
            HttpMethod.Post,
            "{\"linkingProof\":\"sensitive-proof\",\"transactionBindingHash\":\"sensitive-binding\"}"));

        Assert.Equal("[REDACTED]", exception.Data["Request"]);
        Assert.Equal("[REDACTED]", exception.Data["Response"]);
        Assert.Equal(true, exception.Data["Sensitive"]);
        Assert.Equal("[REDACTED]", await exception.ReadResponseContentAsStringAsync());
    }

    [Fact]
    public async Task SendAsync_RedactsLinkedBrokerTokenAssertionOnFailure()
    {
        var connector = CreateConnector();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => connector.SendAsync<object>(
            "https://connector.test/linked-flying-club-broker-token",
            [],
            "token",
            HttpMethod.Post,
            "{\"accountBindingAssertion\":\"signed-sensitive-assertion\"}"));

        Assert.Equal("[REDACTED]", exception.Data["Request"]);
        Assert.Equal("[REDACTED]", exception.Data["Response"]);
        Assert.Equal(true, exception.Data["Sensitive"]);
    }

    [Fact]
    public async Task SendAsync_RedactsVoucherRedemptionFailurePayloads()
    {
        var connector = CreateConnector();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => connector.SendAsync<object>(
            "https://connector.test/basket/basket-id/voucherredemption",
            [],
            "token",
            HttpMethod.Post,
            "{\"selectedVoucherIds\":[\"sensitive-voucher-id\"]}"));

        Assert.Equal("[REDACTED]", exception.Data["Request"]);
        Assert.Equal("[REDACTED]", exception.Data["Response"]);
        Assert.Equal(true, exception.Data["Sensitive"]);
    }

    [Fact]
    public async Task SendAsync_PreservesDiagnosticsForNonSensitiveFailure()
    {
        var connector = CreateConnector();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => connector.SendAsync<object>(
            "https://connector.test/diagnostic",
            [],
            null,
            HttpMethod.Post,
            "{\"request\":\"diagnostic-request\"}"));

        Assert.Contains("diagnostic-request", exception.Data["Request"]?.ToString());
        Assert.Contains("diagnostic-response", exception.Data["Response"]?.ToString());
        Assert.Equal(false, exception.Data["Sensitive"]);
        Assert.Contains("diagnostic-response", await exception.ReadResponseContentAsStringAsync());
    }

    private static HttpConnector CreateConnector() =>
        new(NullLogger<HttpConnector>.Instance, new HttpClientFactory());
}
