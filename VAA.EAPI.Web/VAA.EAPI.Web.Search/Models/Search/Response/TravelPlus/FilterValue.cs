using System;
using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class FilterValue
    {
        //public FilterValueDetails Values { get; set; }
        public bool Selected { get; set; }
        public bool Enabled { get { return Count > 0; } }
        public int Count { get; set; }
        public decimal MinPrice { get; set; }
        public string Currency { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<Link> Links { get; set; }
    }

    public class FilterValueDetails
    {
        public string? Airline { get; set; }
        public int? Connections { get; set; }
        public string? Cabin { get; set; }
        public DateTime? Lower { get; set; }
        public DateTime? Upper { get; set; }
    }
}
