using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Http.Mocking;
using Xunit;

namespace VAA.EAPI.GraphQL.Common.Tests;

public sealed class HttpMockExtensionsTests
{
    private static readonly ILogger Logger = LoggerFactory.Create(static _ => { }).CreateLogger<HttpMockExtensionsTests>();

    [Theory]
    [InlineData("application/xml")]
    [InlineData("text/xml")]
    public void FindMockReturnsMockWhenXmlRulesMatch(string mediaType)
    {
        var mock = LoadMock("put-order-xml-rules.mock");
        HttpMock[] mocks = [mock];

        using var request = new HttpRequestMessage(HttpMethod.Put, "https://tempuri.org/orders/1")
        {
            Content = new StringContent("<root><id>1</id></root>", Encoding.UTF8, mediaType)
        };

        var result = mocks.FindMock(request, Logger);

        Assert.Same(mock, result);
    }

    private static HttpMock LoadMock(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, ".mocks", "HttpMockExtensions", fileName);

        Assert.True(File.Exists(path), $"Mock file not found: {path}");

        return (HttpMock)new FileInfo(path);
    }
}
