using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS
{
    public interface IBrandMSCommand
    {
        Task<IEnumerable<BrandRS>> Handle(BrandMSCommand brandReq, CancellationToken cancellationToken);
    }
}
