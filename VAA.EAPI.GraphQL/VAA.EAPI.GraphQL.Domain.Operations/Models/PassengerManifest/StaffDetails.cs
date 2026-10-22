namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    public class StaffDetails
    {
        public DateTime? DateOfJoining { get; set; }
        public string SortKey { get; set; }
        public string BoardingCode { get; set; }
        public string AuthCode { get; set; }
        public bool IsParty { get; set; }
        [UseFiltering]
        public string Remarks { get; set; }
        public bool IsValid { get; set; }

        public static explicit operator StaffDetails(DTOs.PassengerManifest_SCVv7.StaffDetail source)
        {
            return new StaffDetails()
            {
                SortKey = source?.SortKey ?? string.Empty,
                BoardingCode = source?.BoardingPriority ?? string.Empty,
                DateOfJoining = source.FormattedDateofJoin.HasValue ? new DateTime(source.FormattedDateofJoin.Value.Date.Ticks, DateTimeKind.Utc) : DateTime.MinValue,
                IsParty = source.IsParty == "Y",
                AuthCode = source?.AuthCode ?? string.Empty,
                Remarks = source?.Remarks ?? string.Empty,
                IsValid = source?.IsValid == "Y"
            };
        }
    }
}
