using System.Collections.Generic;

namespace VAA.EAPI.Common.Security
{
    public interface ITokenService<T>
    {
        string GenerateAccessToken(IEnumerable<T> claim);
        string GenerateAccessToken(IEnumerable<T> claim, string issuer, int expireAfterMinuites);
    }
}
