using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.StampFCNumToPNR;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IEligiblePNRsQueryProvider
    {
        Task<IEnumerable<FCNumToPNRDetails>> GetEligiblePNRs(GetEligiblePNRsRequest request);
    }
}
