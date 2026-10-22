using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class CheckedBaggageAllowance
    {
        public List<string> FlightNum { get; set; }
        public List<string> BaggageRefId { get; set; }
    }
}
