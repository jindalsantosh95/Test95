using MediatR;
using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class UpsellOffersDBQuery : IRequest<IEnumerable<UpsellOffersData>>
    {
    }
}
