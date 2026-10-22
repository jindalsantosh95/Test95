using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Xml.Serialization;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;
using Xunit;
using XunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests;

public class ItaStreamingServiceTests
{
    [Fact]
    public async Task StreamOfferSolutions_ProcessesEachSolutionWithoutRetainingProviderItems()
    {
        ConfigReader.Set(Constants.Environment.ItaServiceUrl, "https://testresources.virginatlantic.com/itaservice");
        ConfigReader.Set(Constants.Environment.ItaServiceAPIMSubKey, "test-key");
        using var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IItaService, ItaService>()
            .BuildServiceProvider();
        var service = serviceProvider.GetRequiredService<IItaService>();
        var solutionIds = new List<string>();

        var response = await service.StreamOfferSolutions(
            new search(),
            (solution, _) =>
            {
                solutionIds.Add(solution.id);
                return Task.CompletedTask;
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(312, solutionIds.Count);
        Assert.Equal(312, solutionIds.Distinct().Count());
        Assert.True(response.solutionCountSpecified);
        Assert.Equal(solutionIds.Count, response.solutionCount);
        Assert.Null(response.Items);
        Assert.False(response.session.IsNullOrWhiteSpace());
        Assert.False(response.solutionSet.IsNullOrWhiteSpace());
    }

    [Fact]
    public async Task StreamOfferSolutions_PopulatesAllResultAttributes()
    {
        ConfigReader.Set(Constants.Environment.ItaServiceUrl, "https://testresources.virginatlantic.com/itaservice");
        ConfigReader.Set(Constants.Environment.ItaServiceAPIMSubKey, "test-key");
        using var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IItaService, ItaService>()
            .BuildServiceProvider();
        var service = serviceProvider.GetRequiredService<IItaService>();

        var fullyDeserializedResponse = await service.GetOffers(new search(), TestContext.Current.CancellationToken);
        var streamedResponse = await service.StreamOfferSolutions(
            new search(),
            (_, _) => Task.CompletedTask,
            TestContext.Current.CancellationToken);

        var attributeProperties = typeof(result).GetProperties()
            .Where(property => property.GetCustomAttribute<XmlAttributeAttribute>() is not null);

        foreach (var property in attributeProperties)
        {
            Assert.Equal(property.GetValue(fullyDeserializedResponse), property.GetValue(streamedResponse));

            var specifiedProperty = typeof(result).GetProperty($"{property.Name}Specified");
            if (specifiedProperty is not null)
            {
                Assert.Equal(specifiedProperty.GetValue(fullyDeserializedResponse), specifiedProperty.GetValue(streamedResponse));
            }
        }
    }

}
