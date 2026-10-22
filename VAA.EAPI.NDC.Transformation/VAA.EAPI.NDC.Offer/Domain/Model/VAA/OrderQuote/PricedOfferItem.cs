using System.Collections.Generic;



namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote
{
    public partial class PricedOfferItem
    {
        public string OfferItemId { get; set; }

        public Products Product { get; set; }

        public IEnumerable<string> ServiceBeneficiaries { get; set; }

    }
}
