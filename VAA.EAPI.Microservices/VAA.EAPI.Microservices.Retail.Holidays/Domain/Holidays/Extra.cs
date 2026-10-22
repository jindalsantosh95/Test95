using NodaTime;
using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public class Extra
    {        
        public string Name { get; set; }
        public string ElementGroup { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Type { get; set; }
        public Location GeoLocation { get; set; }
         
    }
}
