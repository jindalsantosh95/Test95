using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

[Trait("Category", "MobileApp-Offline")]
public sealed class EapiFaultCaseTests
{
    [Theory]
    [InlineData(false, "SearchAndBookRetrieveSeatMap", "retrieveSeatMap", "load the seat map")]
    [InlineData(true, "SearchAndBookSelectSeat", "selectSeat", "save your seat selection")]
    public void SeatFixtures_MatchEapiGenericErrorsAndActualAppOperations(bool save, string operation, string root, string uiMessage)
    {
        EapiFaultCase fault = save ? EapiFaultCase.SeatSaveFailure : EapiFaultCase.SeatMapFailure;
        JObject rule = fault.BuildRule();
        Assert.False(fault.PostHpp);
        Assert.False(fault.Basket);
        Assert.False(fault.Timeout);
        Assert.Equal(operation, rule.Value<string>("operationName"));
        Assert.Equal(root, rule.Value<string>("rootField"));
        Assert.Equal("response", rule.Value<string>("mode"));
        Assert.Equal(JTokenType.Null, rule["response"]?["data"]?[root]?.Type);
        JToken error = Assert.Single(Assert.IsType<JArray>(rule["response"]?["errors"]));
        Assert.Equal(save ? "Unhandled Exception in select seat mutation." : "Unhandled Exception in retrieve seat map query.", error.Value<string>("message"));
        Assert.Equal("500", error["extensions"]?.Value<string>("code"));
        Assert.Contains("Synthetic", error["extensions"]?.Value<string>("Exception"));
        Assert.Contains(uiMessage, fault.ExpectedUiMessage);
    }

    [Fact]
    public void PostHppFixtures_TargetActualCompletionMutationsAndNeverContainSuccessData()
    {
        foreach (EapiFaultCase fault in new[] { EapiFaultCase.PaymentStatusFailure,
            EapiFaultCase.BookingCompletionFailure, EapiFaultCase.BookingCompletionTimeout })
        {
            Assert.True(fault.PostHpp);
            JObject rule = fault.BuildRule();
            Assert.Equal(fault.Completion == PostHppOperation.UpdatePaymentStatus
                ? "SearchAndBookUpdatePaymentStatus" : "SearchAndBookCreateOrder", fault.Operation);
            Assert.Equal(JTokenType.Null, rule["response"]?["data"]?[fault.RootField]?.Type);
            Assert.NotEmpty(Assert.IsType<JArray>(rule["response"]?["errors"]));
            Assert.Equal("Sorry, we’re unable to confirm the status of your payment or booking.\nFor more information you can call our Contact Centre on 0344 472 1422", fault.ExpectedUiMessage);
            Assert.Equal(fault.Timeout ? "timeout" : "response", rule.Value<string>("mode"));
        }
        Assert.Equal("500", EapiFaultCase.PaymentStatusFailure.BuildRule()["response"]?["errors"]?[0]?["extensions"]?.Value<string>("code"));
    }
    public static TheoryData<EapiFaultCase> Cases => new()
    {
        EapiFaultCase.PastDate, EapiFaultCase.SearchUnavailable, EapiFaultCase.NoAvailability,
        EapiFaultCase.SupplierSessionMissing, EapiFaultCase.BasketMissing
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Rule_ContainsOnlyReviewedQueryAndSanitisedError(EapiFaultCase scenario)
    {
        JObject rule = scenario.BuildRule();
        Assert.Equal("https://digital-test.virginatlantic.com/sit/graphql/mobile/public/v1", rule.Value<string>("endpoint"));
        Assert.Equal(scenario.Operation, rule.Value<string>("operationName"));
        Assert.Equal(scenario.RootField, rule.Value<string>("rootField"));
        JObject data = Assert.IsType<JObject>(rule["response"]?["data"]);
        Assert.Single(data.Properties());
        Assert.Equal(JTokenType.Null, data[scenario.RootField]?.Type);
        JArray errors = Assert.IsType<JArray>(rule["response"]?["errors"]);
        JToken error = Assert.Single(errors);
        string detailKey = scenario.Basket ? "response" : "Reason";
        JArray details = JArray.Parse(error["extensions"]?.Value<string>(detailKey)
            ?? throw new InvalidOperationException("Missing serialised error details."));
        Assert.Equal(scenario.Code, Assert.Single(details).Value<string>("code"));
        Assert.Equal(scenario.Message, details[0].Value<string>("message"));
        Assert.Equal(scenario.Basket ? "500" : "422", error["extensions"]?.Value<string>("code"));
        Assert.DoesNotContain("Authorization", rule.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
