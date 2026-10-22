using System.Globalization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OrderDataMS
{
    public class TripDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PnrLocator { get; set; }
        public string CreatedDate { get; set; }

        public static explicit operator TripDetails(Trips.Models.Trips.Virgin.TripDetails request)
        {
            return new()
            {
                FirstName = request.GivenNames,
                LastName = request.Surname,
                PnrLocator = request.PnrLocator,
                CreatedDate = request.CreatedDate?.ToString("ddMMMyy", CultureInfo.InvariantCulture).ToUpperInvariant()
            };
        }
    }
}