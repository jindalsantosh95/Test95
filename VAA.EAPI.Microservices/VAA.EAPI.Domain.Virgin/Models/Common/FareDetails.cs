using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class FareDetails
    {
        public string FareBasisCode { get; set; }
        public string DominantFareProduct { get; set; }
        public IEnumerable<FareSegments> FareSegments { get; set; }
        public string FareCalcText { get; set; }
    }
}