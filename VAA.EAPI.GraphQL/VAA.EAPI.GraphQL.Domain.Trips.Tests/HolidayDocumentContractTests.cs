using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests;

public sealed class HolidayDocumentContractTests
{
    private readonly IServiceProvider _serviceProvider;

    public HolidayDocumentContractTests()
    {
        ConfigReader.Set("TripSettings:HolidayProvider:HolidaysMs:Url", "https://testresources.virginatlantic.com/dev/digital/holidays");
        ConfigReader.Set("TripSettings:HolidayProvider:HolidaysMs:OcpSubID", Guid.NewGuid().ToString());

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IHolidayProvider, HolidayProvider>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task RetrieveDocument_ReturnsValidatedIdentifierAndBytes()
    {
        var provider = _serviceProvider.GetRequiredService<IHolidayProvider>();

        var response = await provider.RetrieveDocument(new RetrieveDocumentDownloadRequest
        {
            BookingId = "booking-123",
            DocumentId = "document-123",
            DocumentUrl = "https://documents.example.test/document-123.pdf"
        }, CancellationToken.None);

        Assert.Equal("document-123", response.DocumentId);
        Assert.Equal(new byte[] { 37, 80, 68, 70, 45 }, response.Document);
    }

    [Fact]
    public void HolidayDocumentDetails_DoesNotSerializeTheInternalDocumentUrl()
    {
        var details = new DocumentDetails
        {
            DocumentId = "document-123",
            DocumentToken = "signed-document-token",
            DocumentUrl = "https://documents.example.test/document-123.pdf"
        };

        var serialized = JsonConvert.SerializeObject(details);

        Assert.DoesNotContain("documents.example.test", serialized);
        Assert.Contains("signed-document-token", serialized);
    }
}
