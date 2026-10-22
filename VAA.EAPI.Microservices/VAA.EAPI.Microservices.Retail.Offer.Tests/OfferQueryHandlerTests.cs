using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using XunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests;

public class OfferQueryHandlerTests
{
    [Fact]
    public async Task Handler_ReturnsOffers_FromStreamingItaMock()
    {
        ConfigureEnvironment();
        using var serviceProvider = CreateServices();
        var handler = serviceProvider.GetRequiredService<OfferQueryHandler>();
        var request = TestDataProvider.GetTestData<OfferQuery>("OfferMS\\OfferQueryHandlerRequest");
        request.FlightsearchRequest.SearchOriginDestinations[0].Origin = "LHR";
        request.FlightsearchRequest.SearchOriginDestinations[0].Destination = "DEL";
        request.FlightsearchRequest.SearchOriginDestinations[0].DepartureTimeRange = null;

        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(Status.OK, result.Value.Status);
        var returnedOffers = result.Value.Value.Offers.ToArray();
        Assert.Equal(312, returnedOffers.Length);
        Assert.Equal(312, returnedOffers.Select(offer => offer.OfferId).Distinct().Count());
        Assert.All(returnedOffers, offer => Assert.False(offer.OfferId.IsNullOrWhiteSpace()));
        Assert.NotNull(result.Value.Value.DataList);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Handler_BlankAccountCode_ReturnsNonAccountCodeOffers(string accountCode)
    {
        ConfigureEnvironment();
        using var serviceProvider = CreateServices();
        var handler = serviceProvider.GetRequiredService<OfferQueryHandler>();
        var request = TestDataProvider.GetTestData<OfferQuery>("OfferMS\\OfferQueryHandlerRequest");
        request.Parties.Participants.First().AccountCode = accountCode;
        request.FlightsearchRequest.SearchOriginDestinations[0].Origin = "LHR";
        request.FlightsearchRequest.SearchOriginDestinations[0].Destination = "DEL";
        request.FlightsearchRequest.SearchOriginDestinations[0].DepartureTimeRange = null;

        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(312, result.Value.Value.Offers.Count());
    }

    private static ServiceProvider CreateServices()
    {
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

    private static void ConfigureEnvironment()
    {
        ConfigReader.Set(Constants.Environment.ItaServiceUrl, "https://testresources.virginatlantic.com/itaservice");
        ConfigReader.Set(Constants.Environment.ItaServiceAPIMSubKey, "test-key");
        ConfigReader.Set(Constants.Environment.OfferExpiryLimitInMinutes, "20");
        ConfigReader.Set(Constants.Environment.ItaServiceApi, "revenue");
        ConfigReader.Set(Constants.Environment.ItaServiceKey, "test-key");
        ConfigReader.Set(Constants.Environment.ItaServiceVersion, "142");
        ConfigReader.Set(Constants.Environment.DaysDifferenceDepartureAndBooking, "4:0:0:0");
        ConfigReader.Set(Constants.Environment.ThresholdDaysBeforeDeparture, "3:0:0:0");
        ConfigReader.Set(Constants.Environment.ThresholdDaysAfterBooking, "3:0:0:0");
        ConfigReader.Set(Constants.Environment.HoursDifferenceDepartureAndBooking, "0:4:0:0");
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
