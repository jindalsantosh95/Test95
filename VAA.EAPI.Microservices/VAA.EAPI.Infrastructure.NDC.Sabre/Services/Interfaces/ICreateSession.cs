
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface ICreateSession
    {
       Task<SessionCreateRQResponse>  CreateSessionAsync(SessionCreateRQRequest sessionCreateRQ);
    }
}
