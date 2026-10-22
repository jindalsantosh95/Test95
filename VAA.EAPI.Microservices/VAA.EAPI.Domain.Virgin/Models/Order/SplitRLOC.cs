using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Order
{
    public class SplitRLOC
    {
        public string ParentRLOC { get; set; }
        public bool ParentRLOCLink { get; set; }
        public IEnumerable<ChildRLOCDetails> ChildRLOCDetails { get; set; }
        public string ParentExternalRLOC { get; set; }
    }
}