using MediatR;
using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class FareFamilyQuery : IRequest<IEnumerable<FareFamilies>>
    {

    }
}
