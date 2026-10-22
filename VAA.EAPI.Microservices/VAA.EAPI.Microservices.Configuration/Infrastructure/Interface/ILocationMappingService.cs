using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Interface
{
    public interface ILocationMappingService
    {
        Task<List<LocationMapDbEntity>> GetLocationMapping();
    }
}
