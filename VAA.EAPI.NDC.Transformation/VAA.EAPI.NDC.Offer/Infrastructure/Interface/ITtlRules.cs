using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.TTL;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Interface
{
    public interface ITtlRules
    {
        Task<IEnumerable<TtlRulesData>> Handle(string fareTypeCode, string fareSubTypeCode, CancellationToken cancellationToken);
    }
}
