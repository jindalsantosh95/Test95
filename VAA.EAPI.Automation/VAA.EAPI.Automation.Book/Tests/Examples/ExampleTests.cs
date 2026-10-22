using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Xunit;

namespace VAA.EAPI.Automation.Book.Tests.Examples
{
    public class ExampleTests
    {
        //Example of using the RunWithRetry within a test
        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        public void TestThatMightFail()
        {
            RetryHelper.RunWithRetry(() =>
            {
                // Simulate a flaky test that fails randomly
                var random = new Random().Next(2);

                Assert.True(random == 0, $"Expected 0 but was {random}");

                // Only if no exception is thrown (i.e., test passes), this will be executed
                Assert.True(true); // This will only be hit if no exception is thrown
            }, 3); // Retry up to 3 times
        }
    }
}
