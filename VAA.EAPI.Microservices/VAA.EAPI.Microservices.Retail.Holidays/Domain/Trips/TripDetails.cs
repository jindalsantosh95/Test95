using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
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