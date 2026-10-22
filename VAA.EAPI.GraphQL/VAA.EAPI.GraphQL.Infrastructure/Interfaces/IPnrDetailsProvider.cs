using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Entities.OrderDataMS;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IPnrDetailsProvider
    {
        Task<PnrDetailsResponse> RetrievePNR(string pnr);
    }
}
