using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class PassengerBaggageAllowance
    {
        public FreeBaggageAllowance? FreeBaggageAllowance { get; set; }
        public IEnumerable<PaidBaggage>? PaidBaggage { get; set; }
    }
}
