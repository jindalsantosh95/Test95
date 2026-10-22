using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Flight.Services.Domain;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Helper.Interface
{
    public interface ICachedSsrDetails
    {
        public Task<List<SsrDetails>> GetSsrDetails();
    }
}
