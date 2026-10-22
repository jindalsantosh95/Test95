using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class ControlledBookingCompletionTimeoutTests(ITestOutputHelper output)
    : ControlledSearchFailureTestBase(output, EapiFaultCase.BookingCompletionTimeout)
{
    [MobileFaultFact(postHpp: true)]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "PostHpp-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC09_PostHppBookingTimeout_StopsLoadingWithoutConfirmation() => VerifyControlledFailureAsync();
}
