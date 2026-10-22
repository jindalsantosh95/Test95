using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Services;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface
{
    public interface IServiceResponseInsert
    {
        Task<bool> Handle(ServiceResponseInsertRequest request, CancellationToken cancellationToken);
    }
}
