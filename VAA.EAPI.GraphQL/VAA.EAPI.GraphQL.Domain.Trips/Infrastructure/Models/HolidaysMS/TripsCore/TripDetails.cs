using System;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class TripDetails
    {
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string PnrLocator { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string FlyingClubNumber { get; set; }

    }
}