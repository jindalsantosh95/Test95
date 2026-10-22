namespace VAA.EAPI.GraphQL.Domain.Operations.DTOs.PassengerManifest_SCVv7
{
    public class StaffDetail
    {
        public string BoardingPriority { get; set; }
        public string DateOfJoin { get; set; }
        public DateTime? FormattedDateofJoin { get; set; }
        public string SortKey { get; set; }
        public string AuthCode { get; set; }
        public string IsParty { get; set; }
        public string Remarks { get; set; }
        public string IsValid { get; set; }
    }
}
