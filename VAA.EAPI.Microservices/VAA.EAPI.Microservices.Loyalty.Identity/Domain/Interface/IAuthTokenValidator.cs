using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.Interface
{
    public interface IAuthTokenValidator
    {
        public Task<IEnumerable<Claim>> Verify(string token);
    }
}
