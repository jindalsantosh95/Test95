using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Interface
{
    public interface IitaFareTypeCode
    {
        Task<IEnumerable<ItaFareTypeCodeData>> Handle(string fareTypeCode, string fareSubTypeCode, CancellationToken cancellationToken);
    }
}
