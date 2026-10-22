using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.Holidays.GraphQL;

public sealed class RetrieveHolidaysContentFragmentTests
{
    private readonly string _env = Config.Environment;

    [SkippableFact]
    [Trait("Category", Traits.HolidaysContentFragment)]
    public async Task RetrieveHolidaysContentFragment_HotelProduct_ReturnsHotelDetails()
    {
        string[] optionStatuses = ["FIRM", "DEFINITE", "FULLY PAID"];
        string[] requiredItems = [HolidayBookingItem.GEN.ToString(), HolidayBookingItem.HTL.ToString(), HolidayBookingItem.FLT.ToString()];
        Dictionary<string, string> booking = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
            optionStatus: optionStatuses,
            requiredItems: requiredItems,
            departureDate: HolidaysHelper.GenerateFutureDepartureDate());

        TestDataPrerequisiteHelper.ValidateHolidayDetailsOrSkip(booking);
        string bookingReference = booking["bookingReference"];
        string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(booking["dateOfBirth"]);
        string surname = HolidaysHelper.ExtractSurname(booking["leadName"]);
        string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, booking["leadName"], dateOfBirth);

        JObject retrieveResult = JObject.Parse(await GraphQlEndpoint
            .RetrieveHolidayBooking(_env, "HOLIDAYS", bookingReference, dateOfBirth, surname)
            .EnsureGQLResponseSuccess().Content.ReadAsStringAsync());

        JToken products = retrieveResult.RequireToken("data.retrieveHolidayBooking.product");
        JObject hotelProduct = products.Children<JObject>().FirstOrDefault(product =>
            "HOTEL".Equals(product.SelectToken("bookingType")?.ToString(), StringComparison.OrdinalIgnoreCase))
            ?? throw new Xunit.Sdk.XunitException($"Expected an HOTEL product. {debugContext}");

        string hotelId = hotelProduct.SelectToken("hotelId")?.ToString()
            ?? hotelProduct.SelectToken("hotelDetails.hotelId")?.ToString()
            ?? throw new Xunit.Sdk.XunitException($"Expected hotelId on the HOTEL product. {debugContext}");

        JObject contentResult = JObject.Parse(await GraphQlEndpoint.GetContentFragment(_env, hotelId)
            .EnsureGQLResponseSuccess().Content.ReadAsStringAsync());
        JToken contentToken = contentResult.RequireToken("data.GetContentFragment[0]");
        JObject content = JObject.Parse(contentToken.ToString());

        Assert.False(string.IsNullOrWhiteSpace(content.Value<string>("hotelDescription")), $"hotelDescription was empty. {debugContext}");
        Assert.False(string.IsNullOrWhiteSpace(content.Value<string>("whereIsIt")), $"whereIsIt was empty. {debugContext}");

        JArray keyFeatures = content["keyFeatures"] as JArray ?? new JArray();
        bool facilitiesPresent = keyFeatures.Any(feature =>
            feature["groups"] is JArray groups && groups.Values<string>().Any(group =>
                group.Contains("facilit", StringComparison.OrdinalIgnoreCase)))
            || !string.IsNullOrWhiteSpace(content.Value<string>("facilities"));

        Assert.True(facilitiesPresent, $"Expected hotel facilities to be present. {debugContext}");
    }
}
