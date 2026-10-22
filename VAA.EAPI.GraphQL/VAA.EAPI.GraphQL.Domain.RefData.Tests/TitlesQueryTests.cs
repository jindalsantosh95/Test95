using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.RefData.Queries;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.RefData.Tests;

public sealed class TitlesQueryTests
{
    private readonly IServiceProvider _serviceProvider;

    public TitlesQueryTests()
    {
        ConfigReader.Set(Common.Constants.Environment.RefDataMSGenericUrl, "https://tempuri.org/{0}");
        ConfigReader.Set(Common.Constants.Environment.RefDataMSOcpKey, "test-ocp-key");

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IRefDataService<TitleDetails>, RefDataService<TitleDetails>>()
            .AddSingleton<TitlesQuery>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task GetTitles_CallsReferenceDataServiceAndReturnsTitles()
    {
        var query = _serviceProvider.GetRequiredService<TitlesQuery>();
        var refDataService = _serviceProvider.GetRequiredService<IRefDataService<TitleDetails>>();
        var logger = _serviceProvider.GetRequiredService<ILogger<TitlesQuery>>();

        var result = await query.GetTitles(refDataService, logger);
        var httpClientFactory = (VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>();
        var request = httpClientFactory.GetResponses("https://tempuri.org/titles").Single().RequestMessage!;

        Assert.Equal("https://tempuri.org/titles", request.RequestUri!.ToString());
        Assert.Equal(
            new[]
            {
                "Mr", "Mrs", "Ms", "Miss", "Mstr", "Prof", "Captain", "Reverend", "Sir", "Lady", "Lord", "Dame",
                "Count", "Countess", "Baroness", "Baron", "Sheikh", "Master", "Hon", "Eng", "Chief", "Prince", "Justice",
                "Princess", "", ".", "Major", "Madam", "Comm", "Viscountess", "Viscount", "Laird", "Mayor", "Mx", "Dr"
            },
            result.Select(title => title.Title));
    }
}
