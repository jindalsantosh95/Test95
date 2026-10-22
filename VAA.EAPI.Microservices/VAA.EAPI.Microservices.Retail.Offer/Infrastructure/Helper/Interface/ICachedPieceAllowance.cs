using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface
{
    public interface ICachedPieceAllowance
    {
        Task<IEnumerable<PieceAllowanceTableEntity>> GetCachedPieceAllowanceInformation(string codeshare);
    }
}
