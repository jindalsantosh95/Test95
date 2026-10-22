namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class OperationalStatus
    {
        public string Type { get; set; }
        public string SubType { get; set; }
        public bool? RebookEligible { get; set; }
    }
}