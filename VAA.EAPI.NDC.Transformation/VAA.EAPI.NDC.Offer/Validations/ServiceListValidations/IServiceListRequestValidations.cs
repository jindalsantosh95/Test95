using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;


namespace VAA.EAPI.NDC.Offer.Validations.ServiceListValidations
{
    public interface IServiceListRequestValidations<T>
    {
        Response<T> ValidateServiceListRequest(IATA_ServiceListRQ serviceListReq, bool isOfferServiceRq);
        Response<T> ValidateServiceListOrder(OrderDocument orderDocument, bool isOfferServiceRq, OfferResponse offerDocument);
        Response<T> ValidateAgency(ValidationRS validationRS, OrderDocument orderDocument, bool isOfferServiceRq, OfferResponse offerDocument);

    }
}