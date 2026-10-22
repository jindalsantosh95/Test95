using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.RemarksV3;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface IAir4RemarksV3
    {
        Task<AddSpecialRemarkResponseType> AddSpecialRemarkRequest(AddSpecialRemarkRequestType AddSpecialRemarkRequestObj);

    }
}
