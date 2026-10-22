using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS
{
    public class Upa
    {
        public string AttDescr { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string CtaText { get; set; }
        public string CtaUrl { get; set; }
        public IEnumerable<Fee> Fees { get; set; }
        public IEnumerable<UpaOsi> OptionalServiceIdentifiers { get; set; }
        public IEnumerable<UpaCommon> Photos { get; set; }
        public IEnumerable<UpaCommon> Tours { get; set; }
        public IEnumerable<UpaCommon> Videos { get; set; }
        public string Application { get; set; }
        public string Headline { get; set; }
        public string Description { get; set; }
        public string Assessment { get; set; }
        public string GroupCode { get; set; }
        public string SubGroupCode { get; set; }
        public string LargeImageLink { get; set; }
        public string SmallImageLink { get; set; }
    }
}
