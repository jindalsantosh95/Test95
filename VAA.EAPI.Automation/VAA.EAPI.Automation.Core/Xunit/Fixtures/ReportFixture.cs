using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;

namespace VAA.EAPI.Automation.Core.Xunit.Fixtures
{
    public sealed class ReportFixture : IDisposable
    {
        private static readonly string reportPath = Path.Combine(".", "ExtentReport.html");
        private static readonly ExtentSparkReporter sparkReporter = new(reportPath);
        private static readonly AventStack.ExtentReports.ExtentReports extentReports = new();

        static ReportFixture()
        {
            sparkReporter.Config.Theme = Theme.Standard;
            sparkReporter.Config.DocumentTitle = "Test Execution Report";
            sparkReporter.Config.ReportName = "Automation Results";

            extentReports.AttachReporter(sparkReporter);
        }

        public AventStack.ExtentReports.ExtentReports Current => extentReports;

        public void Dispose()
        {
            extentReports.Flush();
        }
    }
}
