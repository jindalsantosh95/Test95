using MediatR;
using System.Collections.Generic;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS
{
    public class AirportDetailsQuery : IRequest<IEnumerable<AirportDetail>>
    {
    }
}
