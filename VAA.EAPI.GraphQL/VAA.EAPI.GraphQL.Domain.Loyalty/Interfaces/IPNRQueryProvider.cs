using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PNR;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IPNRQueryProvider
    {
        Task<RetrievePNRResponse> RetrievePNR(string pnr);
    }
}
