using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Interface
{
    public interface IitaFareTypeCodeDetails
    {
        Task<IEnumerable<ItaFareTypeCodeDbEntity>> Handle(string fareTypeCode, string fareSubTypeCode,CancellationToken cancellationToken);
    }
}

