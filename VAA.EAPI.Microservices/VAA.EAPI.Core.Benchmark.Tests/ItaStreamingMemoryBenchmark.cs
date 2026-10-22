using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;
using XunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Core.Benchmark.Tests;

[MemoryDiagnoser]
[InProcess]
[WarmupCount(1)]
[IterationCount(3)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class ItaStreamingMemoryBenchmark
{
    private ServiceProvider _serviceProvider = null!;
    private IItaService _service = null!;

    [Params(1, 4, 8, 16, 32, 64, 128, 256, 512)]
    public int ResponsesPerInvocation { get; set; }

    [Params("ATL", "MCO")]
    public string Destination { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        ConfigReader.Set(
            VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.ItaServiceUrl,
            "https://testresources.virginatlantic.com/itaservice");
        ConfigReader.Set(
            VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.ItaServiceAPIMSubKey,
            "benchmark-key");

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IItaService, ItaService>()
            .BuildServiceProvider();
        _service = _serviceProvider.GetRequiredService<IItaService>();
    }

    [Benchmark]
    public async Task<int> StreamLargeResponsesAsync()
    {
        var solutionCount = 0;
        var searchRequest = new search
        {
            inputs = new searchInputs
            {
                slice =
                [
                    new searchInputsSlice { origin = "LHR", destination = Destination },
                    new searchInputsSlice { origin = Destination, destination = "LHR" }
                ]
            }
        };

        for (var responseIndex = 0; responseIndex < ResponsesPerInvocation; responseIndex++)
        {
            await _service.StreamOfferSolutions(
                searchRequest,
                (_, _) =>
                {
                    solutionCount++;
                    return Task.CompletedTask;
                },
                CancellationToken.None);
        }

        return solutionCount;
    }

    [GlobalCleanup]
    public void Cleanup() => _serviceProvider.Dispose();
}
