using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Leg
    {
        public TimeSpan TravelTime { get; set; }
        public ICollection<Fare> Fares { get; set; } = new List<Fare>();

    }
}
