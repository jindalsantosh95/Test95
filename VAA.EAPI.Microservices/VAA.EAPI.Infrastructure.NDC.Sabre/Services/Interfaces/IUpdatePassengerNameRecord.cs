using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IUpdatePassengerNameRecord
    {
        Task<UpdatePassengerNameRecordRQResponse> UpdatePassengerNameRecordRQAsync(UpdatePassengerNameRecordRQRequest request);
    }
}
