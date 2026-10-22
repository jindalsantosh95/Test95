using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class CabinAllowance
    {
        public IEnumerable<string> FareFamilies { get; set; }
        public CarryOnAndCheckedBag CarryOnBag { get; set; }
        public CarryOnAndCheckedBag CheckedBag { get; set; }
    }
}
