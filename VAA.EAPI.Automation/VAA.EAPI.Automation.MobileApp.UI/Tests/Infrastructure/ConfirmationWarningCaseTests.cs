using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

[Trait("Category", "MobileApp-Offline")]
public sealed class ConfirmationWarningCaseTests
{
    public static TheoryData<ConfirmationWarningCase, string[]> Cases => new()
    {
        { ConfirmationWarningCase.AllSeatsFailed, new[] { "W001" } },
        { ConfirmationWarningCase.SomeSeatsFailed, new[] { "W002" } },
        { ConfirmationWarningCase.Proximity, new[] { "W003" } },
        { ConfirmationWarningCase.AllSeatsFailedWithProximity, new[] { "W001", "W003" } },
        { ConfirmationWarningCase.SomeSeatsFailedWithProximity, new[] { "W002", "W003" } },
        { ConfirmationWarningCase.NoWarnings, Array.Empty<string>() }
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void PreparedCase_ContainsExactlyExpectedCodesWithoutMutuallyExclusiveFulfilmentWarnings(
        ConfirmationWarningCase scenario, string[] expected)
    {
        JArray warnings = scenario.BuildWarnings();
        string[] actual = warnings.Select(warning => warning.Value<string>("code")
            ?? throw new InvalidOperationException("Missing warning code.")).ToArray();
        Assert.Equal(expected.OrderBy(code => code), actual.OrderBy(code => code));
        Assert.Equal(expected.OrderBy(code => code), scenario.ExpectedCodes.OrderBy(code => code));
        Assert.Equal(actual.Length, actual.Distinct().Count());
        Assert.False(actual.Contains("W001") && actual.Contains("W002"));
        Assert.Equal(expected.Length, scenario.ExpectedMessages.Count);
        Assert.All(scenario.ExpectedMessages, message => Assert.False(message.IsNullOrWhiteSpace()));
        Assert.Equal(scenario.ExpectedMessages.OrderBy(message => message),
            warnings.Select(warning => warning.Value<string>("message")).OrderBy(message => message));
    }

    [Theory]
    [InlineData("W001", "ALL_SEAT_FULFILMENT_FAILED", "We're sorry, the seats you selected have not been confirmed. Don't worry, you have not been charged. Please head to My Booking to select some new seats.")]
    [InlineData("W002", "PARTIAL_SEAT_FULFILMENT", "Your booking is complete, but unfortunately some of the seats you selected have not been confirmed. Please head to My Booking to select some new seats.")]
    [InlineData("W003", "SEAT_NONCOMPLIANT_PROXIMITY", "It’s a legal requirement for children to sit where they can be supervised by an adult or young adult (age 12 or over) travelling with them. This could be next to them, infront of, behind or across the aisle. Please ensure you  visit My booking before you travel to change your seat selection.")]
    public void PreparedWarning_UsesExactEapiCodeTypeAndMessage(string code, string type, string message)
    {
        JToken warning = Cases.SelectMany(row => ((ConfirmationWarningCase)row[0]).BuildWarnings())
            .First(item => item.Value<string>("code") == code);
        Assert.Equal(type, warning.Value<string>("type"));
        Assert.Equal(message, warning.Value<string>("message"));
        Assert.Equal(new[] { "code", "type", "message" }, Assert.IsType<JObject>(warning).Properties().Select(property => property.Name));
    }

    [Fact]
    public void PreparedCases_MatchDraftContractWithoutClaimingRunnableScenarios()
    {
        JObject contract = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
            "Contracts", "search-book-automation-contract.json")));
        JObject prepared = Assert.IsType<JObject>(contract["preparedConfirmationWarnings"]);
        Assert.Equal("Draft", prepared.Value<string>("readiness"));
        JArray cases = Assert.IsType<JArray>(prepared["cases"]);
        Assert.Equal(Cases.Count, cases.Count);
        foreach (object[] row in Cases)
        {
            ConfirmationWarningCase scenario = (ConfirmationWarningCase)row[0];
            JToken entry = Assert.Single(cases, item => item.Value<string>("name") == scenario.Name);
            JArray codes = Assert.IsType<JArray>(entry["codes"]);
            Assert.Equal(scenario.ExpectedCodes.OrderBy(code => code), codes.Values<string>().OrderBy(code => code));
        }
    }

    [Fact]
    public void PreparedWarning_MutationDoesNotLeakBetweenScenariosOrBuilds()
    {
        ConfirmationWarningCase scenario = ConfirmationWarningCase.AllSeatsFailedWithProximity;
        JArray first = scenario.BuildWarnings();
        first.Clear();
        Assert.Equal(2, scenario.BuildWarnings().Count);
        Assert.Single(ConfirmationWarningCase.AllSeatsFailed.BuildWarnings());
        Assert.Empty(ConfirmationWarningCase.NoWarnings.BuildWarnings());
    }
}
