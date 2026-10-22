namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class PackageDetails
    {
        public string AgentReference { get; set; }
        public DateTime? DefiniteDueDate { get; set; }
        public DateTime? DefiniteDate { get; set; }
        public DateTime? FirmDueDate { get; set; }
        public DateTime? FirmDate { get; set; }
        public DateTime? BalanceDueDate { get; set; }
        public Location Destination { get; set; }
    }

}
