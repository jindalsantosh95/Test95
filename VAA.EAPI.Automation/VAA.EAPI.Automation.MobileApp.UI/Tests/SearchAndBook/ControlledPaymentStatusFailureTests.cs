using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class ControlledPaymentStatusFailureTests(ITestOutputHelper output)
    : ControlledSearchFailureTestBase(output, EapiFaultCase.PaymentStatusFailure)
{
    [MobileFaultFact(postHpp: true)]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "PostHpp-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC07_PostHppPaymentStatusFailure_ShowsErrorWithoutConfirmation() => VerifyControlledFailureAsync();
}
