using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Entities;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IPnrUpdateMutationsProvider
    {
        Task<string> StampingFCNumberToPNR(UpdatePNRRequest request);
    }
}
