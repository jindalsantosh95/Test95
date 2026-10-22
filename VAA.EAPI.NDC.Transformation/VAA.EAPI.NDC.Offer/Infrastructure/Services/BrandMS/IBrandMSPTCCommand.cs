using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS
{
    public interface IBrandMSPTCCommand
    {
        Task<IEnumerable<BrandRS>> Handle(BrandMSCommandPTC brandReq, CancellationToken cancellationToken);
    }
}
