using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers
{
    public class UpsellOffersData
    {
        public string ChosenBrand { get; set; }
        public EligibleUpsellOffer[] EligibleUpsell { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}

