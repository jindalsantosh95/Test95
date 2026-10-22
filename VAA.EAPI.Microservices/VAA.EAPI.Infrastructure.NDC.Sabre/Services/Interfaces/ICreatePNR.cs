using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreatePNR;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface ICreatePNR
    {
        Task<CreatePassengerNameRecordRS> CreatePNRAsync(CreatePassengerNameRecordRQRequest request);
    }
}
