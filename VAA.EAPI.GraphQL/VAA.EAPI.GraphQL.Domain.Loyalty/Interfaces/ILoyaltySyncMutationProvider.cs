using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.SFPD;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ILoyaltySyncMutationProvider<T>
    {
        Task<T> Mutate(SFPDSyncRequest passengerDetailRequest);
    }
}
