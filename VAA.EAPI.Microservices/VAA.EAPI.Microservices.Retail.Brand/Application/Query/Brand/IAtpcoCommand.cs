using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand
{
    public interface IAtpcoCommand
    {
        Task<Models.Brand> Send(BrandQueryRequest brandReqForAllPtc,CancellationToken cancellationToken);
    }
}
