using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using XunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Core.Benchmark.Tests;

[Collection(ItaOfferMemoryLoadCollection.Name)]
public class ItaOfferHandlerMemoryLoadTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    [InlineData(512)]
    public Task FullOfferHandler_ConcurrentLargeResponses_ReportsPeakMemory(int concurrency) =>
        RunFullOfferHandlerConcurrentLargeResponses(concurrency, "ATL");

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    [InlineData(512)]
    public Task FullOfferHandler_ConcurrentLargerResponses_ReportsPeakMemory(int concurrency) =>
        RunFullOfferHandlerConcurrentLargeResponses(concurrency, "MCO");

    private static async Task RunFullOfferHandlerConcurrentLargeResponses(int concurrency, string destination)
    {
        using var provider = CreateServices();
        var handler = provider.GetRequiredService<OfferQueryHandler>();
        var request = TestDataProvider.GetTestData<OfferQuery>("OfferMS\\OfferQueryHandlerRequest");
        request.FlightsearchRequest.SearchOriginDestinations[0].Origin = "LHR";
        request.FlightsearchRequest.SearchOriginDestinations[0].Destination = destination;
        request.FlightsearchRequest.SearchOriginDestinations[0].DepartureTimeRange = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var process = Process.GetCurrentProcess();
        process.Refresh();
        var baselineManagedBytes = GC.GetTotalMemory(false);
        var baselineWorkingSetBytes = process.WorkingSet64;
        var peakManagedBytes = baselineManagedBytes;
        var peakWorkingSetBytes = baselineWorkingSetBytes;
        using var samplingCompleted = new CancellationTokenSource();

        var sampler = Task.Run(async () =>
        {
            while (!samplingCompleted.IsCancellationRequested)
            {
                peakManagedBytes = Math.Max(peakManagedBytes, GC.GetTotalMemory(false));
                process.Refresh();
                peakWorkingSetBytes = Math.Max(peakWorkingSetBytes, process.WorkingSet64);
                await Task.Delay(10, CancellationToken.None);
            }
        }, TestContext.Current.CancellationToken);

        var requestDurationsMilliseconds = new double[concurrency];
        var results = await Task.WhenAll(Enumerable.Range(0, concurrency)
            .Select(async requestIndex =>
            {
                var startedAt = Stopwatch.GetTimestamp();

                try
                {
                    return await handler.Handle(request, TestContext.Current.CancellationToken);
                }
                finally
                {
                    requestDurationsMilliseconds[requestIndex] = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
                }
            }));

        peakManagedBytes = Math.Max(peakManagedBytes, GC.GetTotalMemory(false));
        process.Refresh();
        peakWorkingSetBytes = Math.Max(peakWorkingSetBytes, process.WorkingSet64);
        samplingCompleted.Cancel();
        await sampler;

        Assert.All(results, result =>
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotEmpty(result.Value.Value.Offers);
        });

        Array.Sort(requestDurationsMilliseconds);

        Console.WriteLine(
            "Concurrency={0}; ManagedBaselineMB={1:F1}; ManagedPeakMB={2:F1}; ManagedDeltaMB={3:F1}; WorkingSetBaselineMB={4:F1}; WorkingSetPeakMB={5:F1}; WorkingSetDeltaMB={6:F1}; RequestMeanMs={7:F1}; RequestP50Ms={8:F1}; RequestP95Ms={9:F1}; RequestP99Ms={10:F1}; RequestMinMs={11:F1}; RequestMaxMs={12:F1}",
            concurrency,
            ToMegabytes(baselineManagedBytes),
            ToMegabytes(peakManagedBytes),
            ToMegabytes(peakManagedBytes - baselineManagedBytes),
            ToMegabytes(baselineWorkingSetBytes),
            ToMegabytes(peakWorkingSetBytes),
            ToMegabytes(peakWorkingSetBytes - baselineWorkingSetBytes),
            requestDurationsMilliseconds.Average(),
            Percentile(requestDurationsMilliseconds, 50),
            Percentile(requestDurationsMilliseconds, 95),
            Percentile(requestDurationsMilliseconds, 99),
            requestDurationsMilliseconds[0],
            requestDurationsMilliseconds[^1]);

        GC.KeepAlive(results);
    }

    private static ServiceProvider CreateServices()
    {
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.ItaServiceUrl, "https://testresources.virginatlantic.com/itaservice");
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.ItaServiceAPIMSubKey, "load-test-key");
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.OfferExpiryLimitInMinutes, "20");
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.ItaServiceApi, "revenue");
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.ItaServiceKey, "load-test-key");
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.ItaServiceVersion, "142");
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.DaysDifferenceDepartureAndBooking, "4:0:0:0");
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.ThresholdDaysBeforeDeparture, "3:0:0:0");
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.ThresholdDaysAfterBooking, "3:0:0:0");
        ConfigReader.Set(VAA.EAPI.Microservices.Retail.Offer.Constants.Environment.HoursDifferenceDepartureAndBooking, "0:4:0:0");

        var referenceData = new ReferenceDataFake();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IHttpClientFactory, XunitHttpClientFactory>();
        services.AddSingleton<IHttpConnector, HttpConnector>();
        services.AddSingleton<IItaService, ItaService>();
        services.AddSingleton<ICachedCabin>(referenceData);
        services.AddSingleton<ICachedPriceClass>(referenceData);
        services.AddSingleton<ICachedPieceAllowance>(referenceData);
        services.AddSingleton<ICachedCabinBaggage>(referenceData);
        services.AddSingleton<ICachedAirportDetails>(referenceData);
        services.AddSingleton<IDistributedCacheService, NonRetainingCache>();
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<OfferQueryHandler>());
        services.AddTransient<OfferQueryHandler>();
        return services.BuildServiceProvider();
    }

    private static double ToMegabytes(long bytes) => bytes / 1024d / 1024d;

    private static double Percentile(double[] sortedValues, int percentile)
    {
        var index = (int)Math.Ceiling(percentile / 100d * sortedValues.Length) - 1;
        return sortedValues[Math.Max(index, 0)];
    }

    private sealed class ReferenceDataFake : ICachedCabin, ICachedPriceClass, ICachedPieceAllowance, ICachedCabinBaggage, ICachedAirportDetails
    {
        public Task<CabinTableEntity> GetCachedCabinData(string rowKey) => Task.FromResult(new CabinTableEntity { CabinCode = rowKey, CabinName = rowKey });
        public Task<PriceClassTableEntity> GetCachedPriceClassData(string rowKey) => Task.FromResult(new PriceClassTableEntity { RowKey = rowKey, ProductName = rowKey });
        public Task<IEnumerable<PieceAllowanceTableEntity>> GetCachedPieceAllowanceInformation(string codeshare) => Task.FromResult<IEnumerable<PieceAllowanceTableEntity>>([]);
        public Task<IEnumerable<CabinBaggageEntity>> GetCachedCabinBaggageInformation(string allowance) => Task.FromResult<IEnumerable<CabinBaggageEntity>>([]);
        public Task<AirportDetailsTableEntity> GetCachedAirportDetails(string rowKey) => Task.FromResult(new AirportDetailsTableEntity { RowKey = rowKey, CityCode = rowKey });
    }

    private sealed class NonRetainingCache : IDistributedCacheService
    {
        public Task<T> Get<T>(string key) => Task.FromResult(default(T)!);
        public Task Set<T>(string key, T value, bool isSlidingExpiration, TimeSpan slidingExpirationTime, TimeSpan absoluteExpirationTime) => Task.CompletedTask;
        public Task Refresh(string key) => Task.CompletedTask;
        public Task Remove(string key) => Task.CompletedTask;
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ItaOfferMemoryLoadCollection
{
    public const string Name = "ITA Offer memory load";
}
