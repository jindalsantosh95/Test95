using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayExtra")]
    public class Extra
    {
        public string? Name { get; set; }
        public string? ElementGroup { get; set; }
        public string? ElementGroupName { get; set; }

        public LocalDateTime? StartDate { get; set; }
        public LocalDateTime? EndDate { get; set; }
        public IEnumerable<Location> PickupPoints { get; set; }
        public IEnumerable<Location> DropoffPoints { get; set; }
        public Location Destination { get; set; }
        public Location SelectedPickupPoint { get; set; }
        public Location SelectedDropoffPoint { get; set; }
        public bool Return { get; set; }
        public int OneWayDuration { get; set; }
        public string PickupPointLocation { get; set; }
        public string DropoffPointLocation { get; set; }
        public string Highlight { get; set; }
        public string PrimaryAttribute { get; set; }
        public string SecondaryAttribute { get; set; }
        public bool? IsDisneyTicket { get; set; }

        private static readonly HashSet<string> DisneyTicketElementGroups = new(StringComparer.OrdinalIgnoreCase) { "DISNEYSP", "DISEXT", "DISNEYGC", "DISNEY", "DISNEYFC", "DISNEYSM", "DISNEYSWGE" };

        public static explicit operator Extra((AncillaryProductDetails GenericProductDetails, ProductSummaryDetails Summary, string productType) source)
        {
            var (genericProductDetails, summary, productType) = source;

            return new()
            {
                Name = genericProductDetails?.ElementGroupName ?? string.Join("-", new[] { summary?.PrimaryAttribute, summary?.Name }.Where(x => !string.IsNullOrWhiteSpace(x))),
                ElementGroup = genericProductDetails?.ElementGroup ?? summary?.ElementGroup,
                ElementGroupName = genericProductDetails?.ElementGroupName ?? summary?.ElementGroupName,
                Destination = (Location)genericProductDetails?.Destination,
                PickupPoints = productType == "TRS" ? genericProductDetails?.PickupPoints?.Select(x => (Location)(x, summary?.PrimaryAttribute)) : default,
                DropoffPoints = productType == "TRS" ? genericProductDetails?.DropoffPoints?.Select(x => (Location)(x, summary?.SecondaryAttribute)) : default,
                SelectedPickupPoint = productType == "TRS" ? (Location)(genericProductDetails?.SelectedPickupPoint, summary?.PrimaryAttribute) : default,
                SelectedDropoffPoint = productType == "TRS" ? (Location)(genericProductDetails?.SelectedDropoffPoint, summary?.SecondaryAttribute) : default,
                StartDate = summary?.StartTime.HasValue == true ? LocalDateTime.FromDateTime(summary.StartTime.Value) : null,
                EndDate = summary?.EndTime.HasValue == true ? LocalDateTime.FromDateTime(summary.EndTime.Value) : null,
                OneWayDuration = genericProductDetails?.OneWayTrip?.Duration ?? 0,
                Return = genericProductDetails?.Return ?? false,
                PickupPointLocation = productType == "TRS" ? summary?.PrimaryAttribute : default,
                DropoffPointLocation = productType == "TRS" ? summary?.SecondaryAttribute : default,
                Highlight = summary?.Highlight,
                IsDisneyTicket = DisneyTicketElementGroups.Contains(genericProductDetails?.ElementGroup ?? summary?.ElementGroup)
            };
        }
    }
}
