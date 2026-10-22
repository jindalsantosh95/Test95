namespace VAA.EAPI.GraphQL.Domain.Search.Models.SalesBanner;

public class SalesBanner
{
    // Content
    public string Title { get; set; }
    [GraphQLDescription("Content for the sales banner, with any HTML tags removed. ")]
    public string? BodyPlainText { get; set; }
    [GraphQLDescription("Content for the sales banner in HTML format")]
    public string? BodyHtml { get; set; }
    [GraphQLDescription("Full URL of the sales banner image")]
    public string? Image { get; set; }
    [GraphQLDescription("Alt text for the sales banner image")]
    public string? ImageAltText { get; set; }

    // General visibility and ranking
    [GraphQLIgnore]
    public bool Enabled { get; set; }
    [GraphQLIgnore]
    public DateTime? DisplayFrom { get; set; }
    [GraphQLIgnore]
    public DateTime? DisplayUntil { get; set; }

    [GraphQLIgnore]
    public int? Ranking { get; set; }
    [GraphQLDescription("The display location of this banner, which will match one of the searched locations. There will be at most one banner matched for each location, unless returnAllMatches is set.")]
    public string DisplayLocation { get; set; }
    [GraphQLDescription("Identifier used to switch between a set of supported looks.")]
    public string Theme { get; set; }
}