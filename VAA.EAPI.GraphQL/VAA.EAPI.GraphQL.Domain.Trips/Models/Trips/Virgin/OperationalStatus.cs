
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class OperationalStatus
    {
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public bool? RebookEligible { get; set; }

        public static explicit operator OperationalStatus(Infrastructure.Models.TripMS.OperationalStatus source)
        {
            return new()
            {
                Type = source?.Type,
                SubType = source?.SubType,
                RebookEligible = source?.RebookEligible
            };
        }

        public static explicit operator OperationalStatus(Infrastructure.Models.HolidaysMS.TripsCore.OperationalStatus source)
        {
            return new()
            {
                Type = source?.Type,
                SubType = source?.SubType,
                RebookEligible = source?.RebookEligible
            };
        }
    }
}