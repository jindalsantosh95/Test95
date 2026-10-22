using MediatR;
using VAA.EAPI.NDC.Offer.Domain.Models.OfferRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.AirShoppingMS
{
    public class AirShoppingMSQuery : OfferRequest, IRequest<OfferResponse>
    {
    }
}
