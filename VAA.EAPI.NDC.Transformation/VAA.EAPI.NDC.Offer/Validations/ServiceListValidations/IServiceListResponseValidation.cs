using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Validations.ServiceListValidations
{
    public interface IServiceListResponseValidation
    {
        IEnumerable<NDCWarning> PopulateNonVSSegmentWarningsOrder(OrderDocument orderDocument);
        IEnumerable<NDCWarning> PopulateNonVSSegmentWarningsOffer(OfferResponse offerResponse);
    }
}
