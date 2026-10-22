using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager;

internal static class StoreManagerBookingConflict
{
    private const string ErrorCode = "ACTIVE_BOOKINGS_EXIST";

    public static ServiceResult<T>? Create<T>(
        string storeId,
        IReadOnlyCollection<StoreManagerBookingConflictRow> conflicts)
    {
        if (conflicts.Count == 0)
        {
            return null;
        }

        var orderedConflicts = conflicts
            .GroupBy(conflict => conflict.Date)
            .OrderBy(group => group.Key)
            .Select(group => new Dictionary<string, object?>
            {
                ["date"] = group.Key.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                ["activeBookingCount"] = group.Sum(conflict => conflict.ActiveBookingCount)
            })
            .ToArray();
        var earliestConflict = conflicts
            .OrderBy(conflict => conflict.Date)
            .ThenBy(conflict => conflict.RangeStart)
            .ThenBy(conflict => conflict.RangeEndExclusive)
            .ThenBy(conflict => conflict.Reason, StringComparer.Ordinal)
            .First();
        var activeBookingCount = conflicts.Sum(conflict => conflict.ActiveBookingCount);

        return ServiceResult<T>.Conflict(
            ErrorCode,
            "The requested change cannot be saved because it would affect active bookings.",
            new Dictionary<string, object?>
            {
                ["storeId"] = storeId,
                ["rangeStart"] = earliestConflict.RangeStart.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                ["rangeEndExclusive"] = earliestConflict.RangeEndExclusive.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                ["activeBookingCount"] = activeBookingCount,
                ["affectedDates"] = orderedConflicts,
                ["reason"] = earliestConflict.Reason
            });
    }
}
