using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class PromoCodeSearchTests : SearchBookTestBase
{
    private const string PromoCode = "HEMAN";
    private const string FareName = "Economy Classic";

    public PromoCodeSearchTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-PromoCodes")]
    [Trait("Journey", "Return")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC18_HemanPromoCode_DiscountsManchesterNewYorkReturnEconomyClassicByFivePounds()
    {
        TrySetBrowserStackSessionName();
        try
        {
            DateOnly departureDate = NextFutureMayDate(day: 12);
            FlightSearch search = new(
                "MAN",
                "JFK",
                departureDate,
                PassengerCounts.OneAdult,
                departureDate.AddDays(7));

            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            NativeFlightSearchPage page = new(Fixture.Driver);
            page.Open();
            page.SearchReturn(search);
            page.WaitForResults();

            FareOfferObservation standard = page.ReadFareOffer(0, "Economy", FareName);
            Assert.True(standard.PromotionText.Length == 0,
                $"The baseline {FareName} fare unexpectedly exposed a promotion: {standard.PromotionText}");

            page.ReturnToSearchEntry();
            page.ApplyDiscountCode(PromoCode);
            page.SubmitEditedSearch();

            Assert.Contains(PromoCode, page.ReadAppliedPromotion(0), StringComparison.OrdinalIgnoreCase);
            FareOfferObservation discounted = page.ReadFareOffer(0, "Economy", FareName, requirePromotion: true);

            Assert.Equal(standard.ItineraryFingerprint, discounted.ItineraryFingerprint);
            Assert.Contains(PromoCode, discounted.PromotionText, StringComparison.OrdinalIgnoreCase);
            Assert.NotEmpty(standard.ItineraryFingerprint);
            Assert.True(standard.Price - discounted.Price == 5m,
                $"{PromoCode}: expected a £5 reduction for {FareName}; baseline £{standard.Price}, " +
                $"discounted £{discounted.Price}, reduction £{standard.Price - discounted.Price}.");

            TrySetBrowserStackStatus(
                "passed",
                $"{PromoCode} reduced the matched May MAN-JFK return {FareName} fare from " +
                $"£{standard.Price} to £{discounted.Price}; Economy Light was intentionally excluded.");
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception));
            throw;
        }
    }

    private static DateOnly NextFutureMayDate(int day)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int year = today.Month < 5 || (today.Month == 5 && today.Day < day)
            ? today.Year
            : today.Year + 1;
        return new DateOnly(year, 5, day);
    }
}
