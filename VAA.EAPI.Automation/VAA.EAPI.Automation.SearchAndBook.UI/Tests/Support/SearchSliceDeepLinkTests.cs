using VAA.EAPI.Automation.SearchAndBook.UI.Support;
using Xunit;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Tests.Support;

public sealed class SearchSliceDeepLinkTests
{
    [Fact]
    public void ToRelativeUrl_ReturnJourney_EncodesBothDirectionalSlices()
    {
        SearchSliceDeepLink search = new()
        {
            Origin = "LHR",
            Destination = "JFK",
            DepartureDate = new DateOnly(2027, 1, 20),
            ReturnDate = new DateOnly(2027, 2, 9),
            Adults = 1
        };

        string url = search.ToRelativeUrl();

        Assert.Equal(
            "/flights/search/slice?origin=LHR&destination=JFK&departing=2027-01-20" +
            "&origin=JFK&destination=LHR&departing=2027-02-09&passengers=a1t0c0i0",
            url);
    }

    [Fact]
    public void ToRelativeUrl_OneWayJourney_EncodesOneSlice()
    {
        SearchSliceDeepLink search = new()
        {
            Origin = "LHR",
            Destination = "JFK",
            DepartureDate = new DateOnly(2027, 1, 20),
            Adults = 2,
            Children = 1
        };

        Assert.Equal(
            "/flights/search/slice?origin=LHR&destination=JFK&departing=2027-01-20&passengers=a2t0c1i0",
            search.ToRelativeUrl());
    }
}
