using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class UPA : SegmentPolicy
    {
        public string AttDescr { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string CtaText { get; set; }
        public string CtaUrl { get; set; }
        public IEnumerable<Fee> Fees { get; set; }
        public IEnumerable<UPAOsi> OptionalServiceIdentifiers { get; set; }
        public IEnumerable<UPACommon> Photos { get; set; }
        public IEnumerable<UPACommon> Tours { get; set; }
        public IEnumerable<UPACommon> Videos { get; set; }
    }
}