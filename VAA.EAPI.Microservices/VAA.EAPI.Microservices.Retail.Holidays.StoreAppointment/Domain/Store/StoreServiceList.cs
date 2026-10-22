namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreServiceList
{
    public IEnumerable<string> InStoreFacilities { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<StoreAccessibilityFeature> AccessibilityFeatures { get; set; } = Enumerable.Empty<StoreAccessibilityFeature>();
}
