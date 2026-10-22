using VAA.EAPI.Automation.Core.UI.Xunit.Features;

namespace VAA.EAPI.Automation.Core.UI.Extensions
{
    public static class ScenarioExtensions
    {
        public static Task Run(this Scenario source, params object[] dependencies) => ScenarioRunner.Run(source, dependencies);
    }
}
