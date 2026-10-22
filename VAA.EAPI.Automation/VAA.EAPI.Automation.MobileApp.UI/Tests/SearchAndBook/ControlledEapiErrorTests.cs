using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class ControlledRequestTimeoutTests(ITestOutputHelper output) : ControlledSearchFailureTestBase(output, EapiFaultCase.RequestTimeout)
{
    [MobileFaultFact]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Search-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC06_SearchRequestTimeout_StopsLoadingAndAllowsSearch() => VerifyControlledFailureAsync();
}

public sealed class ControlledSearchUnavailableTests(ITestOutputHelper output) : ControlledSearchFailureTestBase(output, EapiFaultCase.SearchUnavailable)
{
    [MobileFaultFact]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Search-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC02_SearchUnavailable_ShowsErrorAndAllowsSearch() => VerifyControlledFailureAsync();
}

public sealed class ControlledNoAvailabilityTests(ITestOutputHelper output) : ControlledSearchFailureTestBase(output, EapiFaultCase.NoAvailability)
{
    [MobileFaultFact]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Search-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC03_NoAvailabilityError_StopsSearchAndAllowsCorrection() => VerifyControlledFailureAsync();
}

public sealed class ControlledSupplierSessionTests(ITestOutputHelper output) : ControlledSearchFailureTestBase(output, EapiFaultCase.SupplierSessionMissing)
{
    [MobileFaultFact]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Search-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC04_SupplierSessionMissing_ShowsErrorAndAllowsSearch() => VerifyControlledFailureAsync();
}

public sealed class ControlledBasketUnavailableTests(ITestOutputHelper output) : ControlledSearchFailureTestBase(output, EapiFaultCase.BasketMissing)
{
    [MobileFaultFact]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Basket-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC05_BasketUnavailable_StaysOnSummaryWithoutOpeningPassengers() => VerifyControlledFailureAsync();
}
