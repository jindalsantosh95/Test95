using System.Net.Http;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Interfaces
{
    public interface ITokenHelper
    {
        Task<string> GetToken(string key, string uri, HttpContent content);
    }
}
