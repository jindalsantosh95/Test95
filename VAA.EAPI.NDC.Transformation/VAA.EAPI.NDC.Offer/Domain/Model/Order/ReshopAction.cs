using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ReshopAction
    {
        public IEnumerable<DeleteOrderItems> DeleteOrderItems { get; set; }
        public IEnumerable<OfferSubstitution> AddOfferItems { get; set; }
    }
}
