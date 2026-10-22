using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;


namespace VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS
{
    public interface IBrandMSPTCCommand
    {
        Task<IEnumerable<BrandRS>> Handle(BrandMSCommandPTC brandReq, CancellationToken cancellationToken);
    }
}
