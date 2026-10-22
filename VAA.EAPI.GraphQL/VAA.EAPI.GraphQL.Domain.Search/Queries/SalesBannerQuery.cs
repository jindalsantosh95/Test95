using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.SalesBanner;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class SalesBannerQuery
{
    private readonly IContentProvider _contentProvider;
    private readonly ILogger<SalesBannerQuery> _logger;

    public SalesBannerQuery(IContentProvider contentProvider, ILogger<SalesBannerQuery> logger)
    {
        _contentProvider = contentProvider ?? throw new ArgumentNullException(nameof(contentProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [UseSorting]
    [UseFiltering]
    [GraphQLName("salesBanners")]
    [GraphQLDescription("Get the sales banners for a given search. Pass a set of displayLocations and details of the user's current request (e.g. the flight route they have searched for). Up to one banner will be returned for each display location. This banner will have enabled = true, displayFrom/displayUntil will include today's date, and it will match all filters passed in request. If a filter is omitted, either in the banner configuration or in the search, then that filter will be ignored. If multiple banners match, then the one with the lowest ranking will be selected.")]
    public async Task<IEnumerable<SalesBanner>> SalesBanner(
        [GraphQLDescription("A code representing where the banner will be displayed, e.g. 'search-top'.")] string[] displayLocations,
        [GraphQLDescription("Details of the user's search query.")] SalesBannerRequest request,
        [GraphQLDescription("Optional, used for debugging: Return all matching banners, rather than just the one with the lowest ranking.")] bool returnAllMatches = false)
    {
        try
        {
            var salesBannersByDisplayLocation = await _contentProvider.RetrieveSalesBannersAsync(new() { DisplayLocations = displayLocations, Request = request });
            var today = DateTime.UtcNow.Date;
            var visibleSalesBanners = new List<SalesBanner>();

            foreach (string displayLocation in salesBannersByDisplayLocation.Keys)
            {
                var matchingSalesBanners = salesBannersByDisplayLocation[displayLocation]
                    .Where(sb => sb.Enabled = true && sb.Title != null && (sb.DisplayFrom == null || sb.DisplayFrom <= today) && (sb.DisplayUntil == null || sb.DisplayUntil >= today));

                if (matchingSalesBanners.Any())
                {
                    if (returnAllMatches)
                    {
                        visibleSalesBanners.AddRange(matchingSalesBanners.OrderBy(sb => sb.Ranking));
                    }
                    else
                    {
                        visibleSalesBanners.Add(matchingSalesBanners.OrderBy(sb => sb.Ranking).First());
                    }
                }
            }

            return visibleSalesBanners;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception in SalesBannerQuery");
            throw;
        }
    }
}
