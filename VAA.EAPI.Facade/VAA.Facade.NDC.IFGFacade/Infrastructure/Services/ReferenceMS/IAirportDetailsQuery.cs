using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.ReferenceMS;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Services.ReferenceMS
{
    public interface IAirportDetailsQuery
    {
        Task<IEnumerable<AirportDetail>> Handle();
    }
}
