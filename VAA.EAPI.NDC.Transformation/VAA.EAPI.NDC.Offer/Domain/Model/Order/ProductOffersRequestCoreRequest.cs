using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ProductOffersRequestCoreRequest
    {
        public bool? AncillariesOnly { get; set; }
        public OrderNumber OrderToComplement { get; set; }
        public ProductOffersRequestCoreRequestOfferToComplement OfferToComplement { get; set; }
        public AffinityRequest Affinity { get; set; }
        public FlightSpecificOfferRequest FlightSpecific { get; set; }
        public IEnumerable<OriginDestinationRequest> OriginDestinations { get; set; }
        public GeneralFlightRequest GeneralFlightRequest { get; set; }
        public AddOnRequest ProductRequest { get; set; }

    }
}
