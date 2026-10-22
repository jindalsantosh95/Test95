using System.Reflection;
using VAA.EAPI.Automation.Core.Xunit.Runners;
using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Core.Tests;

public sealed class FilterExpressionTests
{
    [Fact]
    public void AndRequiresEveryTraitConditionToMatch()
    {
        FilterExpression expression = (FilterExpression)"Category=HealthCheck&Tag=DotCom";

        Assert.True(expression.IsValid(CreateTestCase(("Category", "HealthCheck"), ("Tag", "DotCom"))));
        Assert.False(expression.IsValid(CreateTestCase(("Category", "HealthCheck"), ("Tag", "MobileApp"))));
        Assert.False(expression.IsValid(CreateTestCase(("Category", "HealthCheck"))));
    }

    private static ITestCase CreateTestCase(params (string Name, string Value)[] traits)
    {
        Dictionary<string, List<string>> values = traits
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Value).ToList());

        return InterfaceProxy.Create<ITestCase>(new Dictionary<string, object?>
        {
            [nameof(ITestCase.Traits)] = values
        });
    }

    private class InterfaceProxy : DispatchProxy
    {
        private IReadOnlyDictionary<string, object?> values = new Dictionary<string, object?>();

        public static T Create<T>(IReadOnlyDictionary<string, object?> values) where T : class
        {
            T proxy = Create<T, InterfaceProxy>();
            ((InterfaceProxy)(object)proxy).values = values;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is not null && targetMethod.Name.StartsWith("get_", StringComparison.Ordinal))
            {
                string property = targetMethod.Name[4..];
                if (values.TryGetValue(property, out object? value))
                    return value;
            }

            Type returnType = targetMethod?.ReturnType ?? typeof(void);
            return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
        }
    }
}
