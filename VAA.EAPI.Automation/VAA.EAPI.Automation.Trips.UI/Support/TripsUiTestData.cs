using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;

namespace VAA.EAPI.Automation.Trips.UI.Support;

public sealed record TripsUiTestData(string Pnr, string FirstName, string LastName)
{
    public static TripsUiTestData FromStorage()
        => FromStorage("OneWay");

    public static TripsUiTestData FromStorage(string journeyType)
    {
        Dictionary<string, string> details = RetrieveFromStorageTable
            .RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyType, "REVENUE");

        AssertValid(details);
        return new(details["pnr"], details["firstName"], details["lastName"]);
    }

    private static void AssertValid(Dictionary<string, string> details)
    {
        if (details.ContainsKey("error"))
        {
            throw new InvalidOperationException(details["error"]);
        }

        if (details["pnr"].IsNullOrWhiteSpace() ||
            details["firstName"].IsNullOrWhiteSpace() ||
            details["lastName"].IsNullOrWhiteSpace() ||
            !int.TryParse(details["passengersCount"], out int passengers) || passengers < 1)
        {
            throw new InvalidOperationException("Storage returned incomplete Trips UI PNR test data.");
        }
    }

}
