using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateToken;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface ICreateToken
    {
        Task<TokenCreateRQResponse> CreateTokenAsync(TokenCreateRQRequest tokenCreate);
    }
}
