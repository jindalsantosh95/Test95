namespace VAA.EAPI.Web.Common.Security
{
    public interface ITokenService<T>
    {
        string GenerateAccessToken(List<T> claim);
        string GenerateAccessToken(List<T> claim, string issuer, int expireAfterMinuites);
    }
}
