using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.CRM.Content.Domain;
using VAA.EAPI.Microservices.CRM.Content.Domain.SalesBanner;
using Environment = VAA.EAPI.Microservices.CRM.Content.Constants.Environment;

namespace VAA.EAPI.Microservices.CRM.Content.Application
{
    public sealed class SalesBannersCommandHandler : ISalesBannersCommandHandler
    {
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly IAzureBlobService _azureBlobService;

        public SalesBannersCommandHandler(IDistributedCacheService distributedCacheService, IAzureBlobService azureBlobService)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task<Dictionary<string, IEnumerable<SalesBanner>>> Handle(SalesBannersCommand request, CancellationToken cancellationToken)
        {
            var response = new Dictionary<string, IEnumerable<SalesBanner>>();
            if (request?.Filters?.DisplayLocations != null)
            {
                foreach (var displayLocation in request.Filters.DisplayLocations.Distinct())
                {
                    var salesBannersJson = await _distributedCacheService.Get<string>("sales-banners:" + displayLocation);
                    response[displayLocation] = salesBannersJson == null
                        ? Enumerable.Empty<SalesBanner>()
                        : await ApplyFilters(Json.Deserialize<IEnumerable<SalesBanner>>(salesBannersJson), request.Filters, cancellationToken);
                }
            }
            return response;
        }

        private async Task<IEnumerable<SalesBanner>> ApplyFilters(IEnumerable<SalesBanner> salesBanners, SalesBannerFilters filters, CancellationToken cancellationToken)
        {
            if (filters?.Request?.BookingType != null && filters.Request.BookingType.Length > 0)
                salesBanners = salesBanners.Where(sb => sb.BookingTypes == null || sb.BookingTypes.Length == 0 || sb.BookingTypes.Contains(filters.Request.BookingType));
            else
                salesBanners = salesBanners.Where(sb => sb.BookingTypes == null);

            if (filters?.Request?.OriginAirport != null && filters.Request.OriginAirport.Length > 0)
                salesBanners = salesBanners.Where(sb => sb.OriginAirports == null || sb.OriginAirports.Length == 0 || sb.OriginAirports.Contains(filters.Request.OriginAirport));
            else
                salesBanners = salesBanners.Where(sb => sb.OriginAirports == null || sb.OriginAirports.Length == 0);

            if (filters?.Request?.DestinationAirport != null && filters.Request.DestinationAirport.Length > 0 || filters?.Request?.DestinationURL != null && filters.Request.DestinationURL.Length > 0)
            {
                string destinationAirport = null;

                if (filters?.Request?.DestinationURL != null && filters.Request.DestinationURL.Length > 0)
                {
                    var blobName = ConfigReader.Get(Environment.DestinationURL);

                    await _azureBlobService.Download(ConfigReader.Get(Environment.BlobStorageUri), ConfigReader.Get(Environment.ContainerName), blobName, ConfigReader.Get(Environment.SasKey), cancellationToken);

                    var referenceDataResult = await _azureBlobService.BlobStringAsync(blobName, cancellationToken);

                    destinationAirport = Json.Deserialize<IEnumerable<SalesBannerDestinationUrls>>(referenceDataResult).Where(x => filters.Request.DestinationURL.StartsWith(x.DestinationURL, StringComparison.Ordinal))?.OrderByDescending(s => s.DestinationURL.Length)?.FirstOrDefault()?.DestinationAirport;
                }

                destinationAirport ??= filters?.Request?.DestinationAirport;

                salesBanners = salesBanners.Where(sb => sb.DestinationAirports == null || sb.DestinationAirports.Length == 0 || sb.DestinationAirports.Contains(destinationAirport));
            }
            else
                salesBanners = salesBanners.Where(sb => sb.DestinationAirports == null || sb.DestinationAirports.Length == 0);


            if (filters?.Request?.DepartureDate != null)
            {
                var dow = filters.Request.DepartureDate.Value.DayOfWeek.ToString().ToLower();

                salesBanners = salesBanners.Where(sb => sb.EarliestDepartureDate == null || sb.EarliestDepartureDate <= filters.Request.DepartureDate)
                                           .Where(sb => sb.LatestDepartureDate == null || sb.LatestDepartureDate >= filters.Request.DepartureDate)
                                           .Where(sb => sb.DayOfWeek == null || sb.DayOfWeek.Length == 0 || sb.DayOfWeek.Contains($"day-of-week:{dow}"));
            }
            else
                salesBanners = salesBanners.Where(sb => sb.EarliestDepartureDate == null && sb.LatestDepartureDate == null && (sb.DayOfWeek == null || sb.DayOfWeek.Length == 0));

            if (filters?.Request?.ReturnDate != null)
                salesBanners = salesBanners.Where(sb => sb.EarliestReturnDate == null || sb.EarliestReturnDate <= filters.Request.ReturnDate)
                                           .Where(sb => sb.LatestReturnDate == null || sb.LatestReturnDate >= filters.Request.ReturnDate);
            else
                salesBanners = salesBanners.Where(sb => sb.EarliestReturnDate == null && sb.LatestReturnDate == null);

            if (filters?.Request?.FlightDirection != null && filters.Request.FlightDirection.Length > 0)
                salesBanners = salesBanners.Where(sb => sb.FlightDirection == null || sb.FlightDirection.Length == 0 || sb.FlightDirection.Contains(filters.Request.FlightDirection));
            else
                salesBanners = salesBanners.Where(sb => sb.FlightDirection == null);

            if (filters?.Request?.NumAdults != null)
                salesBanners = salesBanners.Where(sb => sb.MinAdults == null || sb.MinAdults <= filters.Request.NumAdults)
                                           .Where(sb => sb.MaxAdults == null || sb.MaxAdults >= filters.Request.NumAdults);
            else
                salesBanners = salesBanners.Where(sb => sb.MinAdults == null && sb.MaxAdults == null);

            if (filters?.Request?.NumTeens != null)
                salesBanners = salesBanners.Where(sb => sb.MinTeens == null || sb.MinTeens <= filters.Request.NumTeens)
                                           .Where(sb => sb.MaxTeens == null || sb.MaxTeens >= filters.Request.NumTeens);
            else
                salesBanners = salesBanners.Where(sb => sb.MinTeens == null && sb.MaxTeens == null);

            if (filters?.Request?.NumChildren != null)
                salesBanners = salesBanners.Where(sb => sb.MinChildren == null || sb.MinChildren <= filters.Request.NumChildren)
                                           .Where(sb => sb.MaxChildren == null || sb.MaxChildren >= filters.Request.NumChildren);
            else
                salesBanners = salesBanners.Where(sb => sb.MinChildren == null && sb.MaxChildren == null);

            if (filters?.Request?.NumInfants != null)
                salesBanners = salesBanners.Where(sb => sb.MinInfants == null || sb.MinInfants <= filters.Request.NumInfants)
                                           .Where(sb => sb.MaxInfants == null || sb.MaxInfants >= filters.Request.NumInfants);
            else
                salesBanners = salesBanners.Where(sb => sb.MinInfants == null && sb.MaxInfants == null);

            return salesBanners.OrderBy(sb => sb.Ranking);
        }
    }
}
