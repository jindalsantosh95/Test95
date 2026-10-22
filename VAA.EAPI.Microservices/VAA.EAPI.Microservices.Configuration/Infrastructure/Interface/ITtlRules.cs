using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Interface
{
    public interface ITtlRules
    {
        Task<IEnumerable<TtlRulesData>> Handle(string fareTypeCode, string fareSubTypeCode, CancellationToken cancellationToken);
    }
}

