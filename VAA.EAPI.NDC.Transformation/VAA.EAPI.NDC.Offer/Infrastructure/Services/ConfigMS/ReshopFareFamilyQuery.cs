using MediatR;
using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.ReshopFareFamily;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class ReshopFareFamilyQuery : IRequest<IEnumerable<ReshopFareFamily>>
    {

    }
}
