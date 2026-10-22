using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMSReshopCancel
{
    public class CancelReshopResponse
    {
        public string ShoppingResponseId { get; set; }
        public IEnumerable<ReshopOffer> Offers { get; set; }
        public DataList DataList { get; set; }
        public bool IsRfc { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
