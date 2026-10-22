using System.Reflection;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class ReleasePackContractTests
{
    private static JObject ReadContract() => JObject.Parse(File.ReadAllText(Path.Combine(
        AppContext.BaseDirectory, "Contracts", "search-book-automation-contract.json")));

    private static bool HasTrait(MethodInfo method, string name, string value)
        => method.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(TraitAttribute)
            && attribute.ConstructorArguments[0].Value as string == name
            && attribute.ConstructorArguments[1].Value as string == value);

    private static MethodInfo[] Scenarios() => typeof(ReleasePackContractTests).Assembly.GetTypes()
        .Where(type => !type.IsAbstract)
        .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        .Where(method => method.GetCustomAttribute<FactAttribute>() != null).ToArray();

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("sanity", "Sanity", 14)]
    [InlineData("regression", "Regression", 54)]
    [InlineData("backendRegression", "BackendRegression", 2)]
    public void ReleasePack_MatchesManifestWithoutSkippedOrSharedScenarios(string key, string suite, int count)
    {
        JObject contract = ReadContract();
        JObject packs = Assert.IsType<JObject>(contract["releasePacks"]);
        JArray scenarios = Assert.IsType<JArray>(contract["scenarios"]);
        string[] ids = Assert.IsType<JArray>(packs[key]).Values<string>().Select(id => id ?? string.Empty).ToArray();
        Assert.Equal(count, ids.Length);
        Assert.Equal(count, ids.Distinct().Count());
        string[] expected = ids.Select(id => Assert.Single(scenarios,
                scenario => scenario.Value<string>("id") == id).Value<string>("testMethod") ?? string.Empty)
            .OrderBy(name => name, StringComparer.Ordinal).ToArray();
        MethodInfo[] selected = Scenarios().Where(method => HasTrait(method, "Suite", suite)
            && HasTrait(method, "RegressionArea", packs.Value<string>("area") ?? string.Empty)
            && HasTrait(method, "Platform", packs.Value<string>("platform") ?? string.Empty)).ToArray();
        Assert.Equal(expected, selected.Select(method => method.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(selected.Length, selected.Select(method => method.DeclaringType).Distinct().Count());
        foreach (MethodInfo method in selected)
        {
            Assert.Null(method.GetCustomAttribute<FactAttribute>()?.Skip);
            Assert.False(HasTrait(method, "Suite", "Deferred"));
            Assert.False(HasTrait(method, "Suite", "Qualification"));
            Assert.False(HasTrait(method, "Suite", "Readiness"));
            Assert.False(HasTrait(method, "Readiness", "AppContractRequired"));
            Assert.False(HasTrait(method, "Readiness", "AppChangeRequired"));
        }
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void ReleasePacks_AccountForEverySearchBookScenarioAndKeepSanityInsideRegression()
    {
        JObject contract = ReadContract();
        JObject packs = Assert.IsType<JObject>(contract["releasePacks"]);
        string[] regression = Assert.IsType<JArray>(packs["regression"]).Values<string>().Select(id => id ?? string.Empty).ToArray();
        string[] backend = Assert.IsType<JArray>(packs["backendRegression"]).Values<string>().Select(id => id ?? string.Empty).ToArray();
        JObject excluded = Assert.IsType<JObject>(packs["excluded"]);
        foreach (JToken id in Assert.IsType<JArray>(packs["sanity"]))
        {
            Assert.Contains(id.ToString(), regression);
        }
        Assert.All(backend, id => Assert.Contains(id, regression));
        string[] accounted = regression.Concat(excluded.Properties().Select(property => property.Name)).ToArray();
        Assert.Equal(accounted.Length, accounted.Distinct().Count());
        string[] expected = Assert.IsType<JArray>(contract["scenarios"])
            .Where(scenario => (scenario.Value<string>("testMethod") ?? string.Empty).StartsWith("SearchBook_", StringComparison.Ordinal)
                || (scenario.Value<string>("testMethod") ?? string.Empty).StartsWith("LoyaltySearchBook_", StringComparison.Ordinal))
            .Select(scenario => scenario.Value<string>("id") ?? string.Empty).ToArray();
        Assert.Equal(expected.OrderBy(id => id, StringComparer.Ordinal), accounted.OrderBy(id => id, StringComparer.Ordinal));
        Assert.All(excluded.Properties(), property => Assert.NotEmpty(property.Value.ToString().Trim()));
    }
}
