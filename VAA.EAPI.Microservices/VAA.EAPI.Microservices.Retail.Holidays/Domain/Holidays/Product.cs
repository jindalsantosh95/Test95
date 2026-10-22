using Newtonsoft.Json.Linq;
using System;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Core;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public partial class Product : BaseProduct
    {
        public Property Property { get; set; }
        public Vehicle Vehicle { get; set; }
        public Rentalperiod RentalPeriod { get; set; }
        public Location PickUpLocation { get; set; }
        public Location DropOffLocation { get; set; }
        public DriverDetails Driver { get; set; }
        public Extra ExtraDetails { get; set; }
    }
}