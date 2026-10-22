namespace VAA.EAPI.Automation.Trips.Requests.Factory
{
    /// <summary>
    /// One element of the <c>request.tripDetails[]</c> array sent to the
    /// <c>TripDetails</c> GraphQL query. The endpoint accepts a mix of lookup
    /// shapes in a single call — PNR-based entries populate
    /// <see cref="PnrLocator"/> / <see cref="GivenNames"/> / <see cref="Surname"/>
    /// (optionally with <see cref="CreatedDate"/>), while Holidays-booking lookups
    /// populate <see cref="BookingReference"/> / <see cref="DateOfBirth"/> /
    /// <see cref="Surname"/>. Leave the unused fields as <c>null</c> on each entry
    /// — the factory serialises them as JSON nulls, matching the wire format the
    /// GraphQL endpoint expects.
    /// </summary>
    public class TripDetailEntry
    {
        public string? PnrLocator { get; init; }
        public string? GivenNames { get; init; }
        public string? Surname { get; init; }
        public string? CreatedDate { get; init; }
        public string? BookingReference { get; init; }
        public string? DateOfBirth { get; init; }
    }
}
