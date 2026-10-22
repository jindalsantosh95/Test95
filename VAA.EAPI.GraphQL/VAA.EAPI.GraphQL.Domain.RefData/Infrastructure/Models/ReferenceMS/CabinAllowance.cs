using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    public class CabinAllowance
    {
        public IEnumerable<string> FareFamilies { get; set; }
        public CarryOnAndCheckedBag CarryOnBag { get; set; }
        public CarryOnAndCheckedBag CheckedBag { get; set; }
    }
}
