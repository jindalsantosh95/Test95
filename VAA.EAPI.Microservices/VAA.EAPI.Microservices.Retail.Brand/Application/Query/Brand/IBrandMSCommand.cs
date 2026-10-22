using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand
{
    public interface IBrandMSCommand
    {
        Task<IEnumerable<Models.Brand>> Handle(BrandPTCQueryRequest brandReqForAllPtc, IEnumerable<string> totalPTC, CancellationToken cancellationToken);

    }
}