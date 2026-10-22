using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    [RefDataType("UpgradeOfferDetails", "UpgradeOfferDetails")]
    public class UpgradeInfo
    {
        public string Carrier { get; set; }
        public string BrandName { get; set; }
        public IEnumerable<string> Features { get; set; }
        public string FareFamily { get; set; }
        public string Image { get; set; }
        public string Refundable { get; set; }
    }
}