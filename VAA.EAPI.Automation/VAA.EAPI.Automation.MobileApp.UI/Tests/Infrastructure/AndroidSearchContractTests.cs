using System.Reflection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class AndroidSearchContractTests
{
    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AndroidSearch_ProposedSelectorsMatchCentralOwner()
    {
        JObject contract = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "search-book-automation-contract.json")));
        JObject android = Assert.IsType<JObject>(contract["androidReadiness"]);
        Assert.Equal("proposed-contract-awaiting-android-delivery", android.Value<string>("status"));
        JObject selectors = Assert.IsType<JObject>(android["selectors"]);
        FieldInfo[] fields = typeof(AndroidSearchResourceIds).GetFields(BindingFlags.Public | BindingFlags.Static);
        Assert.Equal(fields.Length, selectors.Properties().Count());
        foreach (FieldInfo field in fields)
        {
            string key = char.ToLowerInvariant(field.Name[0]) + field.Name[1..];
            Assert.Equal(field.GetRawConstantValue(), selectors.Value<string>(key));
        }
        string[] values = fields.Select(field => Assert.IsType<string>(field.GetRawConstantValue())).ToArray();
        Assert.Equal(values.Length, values.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AndroidSearch_OnlyGenuinePlatformDifferencesHaveDedicatedScenarios()
    {
        Type[] classes = typeof(AndroidSearchContractTests).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(AndroidSearchPanelTestBase).IsAssignableFrom(type)).ToArray();
        Assert.Equal(2, classes.Length);
        foreach (Type type in classes)
        {
            Assert.Null(type.GetCustomAttribute<CollectionAttribute>());
            MethodInfo method = Assert.Single(type.GetMethods(), candidate => candidate.GetCustomAttribute<FactAttribute>() is not null);
            Assert.Matches("^AndroidSearchBook_TC0[5-6]_", method.Name);
            CustomAttributeData[] traits = method.CustomAttributes.Where(attribute => attribute.AttributeType == typeof(TraitAttribute)).ToArray();
            Assert.Contains(traits, attribute => Equals(attribute.ConstructorArguments[0].Value, "Readiness") &&
                Equals(attribute.ConstructorArguments[1].Value, "AppContractRequired"));
            Assert.DoesNotContain(traits, attribute =>
                attribute.ConstructorArguments.Any(argument => argument.Value is string value && value.Contains("Regression", StringComparison.Ordinal)));
        }
    }
}
