using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class ControlledSeatSaveFailureTests(ITestOutputHelper output)
    : ControlledSearchFailureTestBase(output, EapiFaultCase.SeatSaveFailure)
{
    [MobileFaultFact]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Seats-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC11_SeatSaveFailure_ShowsErrorWithoutOpeningPayment() => VerifyControlledFailureAsync();
}
