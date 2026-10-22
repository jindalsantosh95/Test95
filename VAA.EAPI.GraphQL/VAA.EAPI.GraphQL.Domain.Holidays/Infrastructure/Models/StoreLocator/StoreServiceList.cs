namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class StoreServiceList
{
    public IEnumerable<string> InStoreFacilities { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<StoreAccessibilityFeature> AccessibilityFeatures { get; set; } = Enumerable.Empty<StoreAccessibilityFeature>();
}
