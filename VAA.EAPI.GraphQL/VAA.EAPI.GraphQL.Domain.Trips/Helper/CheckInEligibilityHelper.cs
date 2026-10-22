using VAA.EAPI.GraphQL.Common.Extensions;
using HolidaySlice = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Slice;
using TripSlice = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Slice;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    public static class CheckInEligibilityHelper
    {
        public static bool IsCancelled(TripSlice slice) =>
            slice?.Segments?.Any(IsCancelled) == true;

        public static bool IsCancelled(HolidaySlice slice) =>
            slice?.Segments?.Any(IsCancelled) == true;

        private static bool IsCancelled(Infrastructure.Models.TripMS.Segment segment) =>
            segment?.Legs?.Any(leg => leg?.Status.Is("CANCELLED") == true) == true ||
            IsIropsCancellation(segment?.OperationalStatus?.Type, segment?.OperationalStatus?.SubType);

        private static bool IsCancelled(Infrastructure.Models.HolidaysMS.TripsCore.Segment segment) =>
            segment?.Legs?.Any(leg => leg?.Status.Is("CANCELLED") == true) == true ||
            IsIropsCancellation(segment?.OperationalStatus?.Type, segment?.OperationalStatus?.SubType);

        private static bool IsIropsCancellation(string type, string subType) =>
            (type.Is("IROP") || type.Is("IROPS")) && IsActiveCancellationSubtype(subType);

        private static bool IsActiveCancellationSubtype(string subType)
        {
            if (subType.Is("CANCEL") || subType.Is("CANCELLED"))
                return true;

            var values = subType?.Split(' ');
            return values?.Any(value => value.Is("CANCEL") || value.Is("CANCELLED")) == true &&
                values.Any(value => value.Is("DIRTY"));
        }
    }
}
