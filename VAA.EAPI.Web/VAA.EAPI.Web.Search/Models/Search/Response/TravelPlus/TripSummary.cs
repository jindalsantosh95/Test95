using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class TripSummary
    {
        public IEnumerable<SliceDetails> SliceDetails { get; set; } = Enumerable.Empty<SliceDetails>();
        public string Currency { get; set; }
        public decimal TotalPrice => SliceDetails.Sum(slc => slc.selectedPrice);
        public string TotalAwardPoints { get; set; }
    }
}
