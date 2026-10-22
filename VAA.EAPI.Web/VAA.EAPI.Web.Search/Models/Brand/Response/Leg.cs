using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class Leg
    {
        public string TravelTime { get; set; }
        public ICollection<Fare> Fares { get; set; }
    }
}