
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.StampFCNumToPNR;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IStampFCNumToPNRMutationsProvider
    {
        public Task<StampFCNumToPNRResponse> StampingFCNumberToPNR(StampFCNumToPNRRequest request);
    }
}
