using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;
using Xunit;
using TestHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests;

public sealed class AddressLookupV3ExpansionTests
{
    private const string Provider = "https://expansion.example.test";

    public AddressLookupV3ExpansionTests()
    {
        ConfigReader.Set(Constants.Environment.AddressLookupV3MinQueryLength, "3");
        ConfigReader.Set(Constants.Environment.AddressLookupV3SuggestionLimit, "6");
        ConfigReader.Set(Constants.Environment.AddressLookupV3ReferenceLifetimeSeconds, "300");
        ConfigReader.Set(Constants.Environment.AddressLookupV3ReferenceEncryptionKey, Convert.ToBase64String(new byte[32]));
        ConfigReader.Set(Constants.Environment.GbgServiceKeyMap, "[{\"clientId\":\"\",\"gbgKey\":\"synthetic-key\"}]");
        ConfigReader.Set(Constants.Environment.GbgV3FindEndpoint, Provider + "/find");
        ConfigReader.Set(Constants.Environment.GbgV3RetrieveEndpoint, Provider + "/retrieve");
    }

    [Fact]
    public async Task Suggestions_ExpandNestedContainersOnce_AndReturnUniqueSelectableAddresses()
    {
        using var services = CreateServices();
        var service = services.GetRequiredService<AddressLookupV3Service>();
        var response = await service.GetSuggestionsAsync(
            new AddressLookupV3SuggestionRequest { Query = "nested", CountryCode = "GB" }, TestContext.Current.CancellationToken);

        var candidates = response.Candidates.ToArray();
        Assert.Equal(7, candidates.Length); // Combined results are not truncated back to the per-Find limit of six.
        Assert.All(candidates, candidate => Assert.Equal("Address", candidate.Type));
        var protector = services.GetRequiredService<IAddressLookupV3ReferenceProtector>();
        var references = candidates.Select(candidate => protector.Unprotect(candidate.Reference)).ToArray();
        Assert.Equal(new[] { "direct", "a1", "a2", "a3", "a4", "a5", "deep-address" }, references.Select(reference => reference.ProviderId));
        Assert.All(references, reference =>
        {
            Assert.Equal(response.FlowId, reference.FlowId);
            Assert.Equal("GB", reference.CountryCode);
            Assert.NotEmpty(reference.SelectionIntentId);
        });
        var requests = services.GetRequiredService<TestHttpClientFactory>().GetResponses(Provider).Select(item => item.RequestMessage!).ToArray();
        Assert.Equal(4, requests.Length); // Initial, postcode, building and nested street; cycles/overlap add no calls.
        Assert.All(requests, request => Assert.Equal("/find", request.RequestUri!.AbsolutePath));
        Assert.All(requests.Skip(1), request => Assert.DoesNotContain("Text=", request.RequestUri!.Query));

        var selection = await service.GetSelectionAsync(new AddressLookupV3SelectionRequest { Reference = candidates[^1].Reference }, TestContext.Current.CancellationToken);
        Assert.Equal(response.FlowId, selection.FlowId);
        Assert.Equal("Synthetic address", selection.Address.AddressLine1);
        Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses(Provider),
            item => item.RequestMessage!.RequestUri!.AbsolutePath == "/retrieve");
    }

    [Fact]
    public async Task Suggestions_ExpandAllNonAddressTypes_AndIgnoreMalformedItems()
    {
        using var services = CreateServices();
        var response = await services.GetRequiredService<AddressLookupV3Service>().GetSuggestionsAsync(
            new AddressLookupV3SuggestionRequest { Query = "types", CountryCode = "GB" }, TestContext.Current.CancellationToken);

        var candidate = Assert.Single(response.Candidates);
        Assert.Equal("Address", candidate.Type);
        var reference = services.GetRequiredService<IAddressLookupV3ReferenceProtector>().Unprotect(candidate.Reference);
        Assert.Equal("typed-address", reference.ProviderId);
        Assert.Equal(response.FlowId, reference.FlowId);
        var requests = services.GetRequiredService<TestHttpClientFactory>().GetResponses(Provider).Select(item => item.RequestMessage!).ToArray();
        Assert.Equal(5, requests.Length); // Initial plus Locality, BuildingNumber, SubBuilding and a future non-Address type.
        Assert.All(requests, request => Assert.Equal("/find", request.RequestUri!.AbsolutePath));
    }

    [Fact]
    public async Task Suggestions_WhenExpansionFails_ReturnErrorWithoutRetrieveOrRetry()
    {
        using var services = CreateServices();
        var service = services.GetRequiredService<AddressLookupV3Service>();
        var exception = await Assert.ThrowsAsync<AddressLookupV3Exception>(() => service.GetSuggestionsAsync(
            new AddressLookupV3SuggestionRequest { Query = "failure", CountryCode = "GB" }, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, exception.StatusCode);
        Assert.Equal("ProviderUnavailable", exception.Code);
        var responses = services.GetRequiredService<TestHttpClientFactory>().GetResponses(Provider);
        Assert.Equal(2, responses.Count);
        Assert.All(responses, response => Assert.Equal("/find", response.RequestMessage!.RequestUri!.AbsolutePath));
    }

    private static ServiceProvider CreateServices() => new ServiceCollection()
        .AddLogging()
        .AddSingleton<TestHttpClientFactory>()
        .AddSingleton<IHttpClientFactory>(services => services.GetRequiredService<TestHttpClientFactory>())
        .AddSingleton<IHttpConnector, HttpConnector>()
        .AddSingleton<IGbgV3Service, GbgV3Service>()
        .AddSingleton<IAddressLookupV3ReferenceProtector, AddressLookupV3ReferenceProtector>()
        .AddSingleton(new TelemetryConfiguration())
        .AddSingleton<TelemetryClient>()
        .AddSingleton<AddressLookupV3Service>()
        .BuildServiceProvider();
}
