namespace VAA.EAPI.Web.Common.Infrastructure.Interfaces
{
    public interface IOAuthTokenProvider
    {
        Task<string> AcquireTokenAsync(string resource);
    }
}
