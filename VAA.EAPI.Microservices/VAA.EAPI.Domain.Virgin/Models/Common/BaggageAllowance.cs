using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class BaggageAllowance
    {
        public List<CheckedBaggageAllowance> CheckedBaggageAllowances { get; set; }
        public List<CarryOnBaggageAllowance> CarryOnBaggageAllowances { get; set; }
    }
}
