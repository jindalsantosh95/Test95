using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferPriceRQ;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferPriceMS
{
    public class OfferPriceMicroserviceCommand : OfferPriceRQ, IRequest<OfferResponse>
    {

    }
}
