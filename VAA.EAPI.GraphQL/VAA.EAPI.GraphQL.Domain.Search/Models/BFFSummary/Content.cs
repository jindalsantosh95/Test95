using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;

public class Content
{
    public string? CabinName { get; set; }
    public string? UpgradeImage { get; set; }
    public IEnumerable<Feature>? Features { get; set; }

    public static explicit operator Content(UpgradeInfo source)
    {
        return new Content()
        {
            CabinName = source.BrandName,
            UpgradeImage = source.Image,
            Features = source.Features.Select(feature => new Feature { Description = feature })
        };
    }
}