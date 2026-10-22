using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class BaggageListItem
    {
        public IEnumerable<string> RevenueClasses { get; set; }
        public IEnumerable<string> BrandedProducts { get; set; }
        public IEnumerable<string> PTC { get; set; }
        public int? MaxWeight { get; set; }
        public int? NumberOfPieces { get; set; }
        public int? XDimension { get; set; }
        public int? ZDimension { get; set; }
        public string BagType { get; set; }
        public string Remarks { get; set; }
        public IEnumerable<BaggageDisclosureRule> Disclosures { get; set; }
        public Link Link { get; set; }

    }
}
