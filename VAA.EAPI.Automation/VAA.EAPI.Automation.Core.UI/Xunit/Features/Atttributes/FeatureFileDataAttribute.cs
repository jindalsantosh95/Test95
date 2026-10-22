using System.Reflection;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using Xunit.Sdk;

namespace VAA.EAPI.Automation.Core.UI.Xunit.Features.Atttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class FeatureFileDataAttribute<T>(string filePath, string scenarioFilter = "") : DataAttribute
    {
        private readonly string _filePath = $"./{filePath.TrimStart('.').TrimStart('\\').TrimStart('/')}";
        private readonly string _scenarioFilter = scenarioFilter;

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var feature = FeatureParser.Parse<T>(_filePath);

            foreach (var s in feature.Scenarios.Where(x => _scenarioFilter.IsNullOrWhiteSpace() || RegexCompiled.IsMatch(x.Name, _scenarioFilter)))
                yield return new object[] { s };
        }
    }
}