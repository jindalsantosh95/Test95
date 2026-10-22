using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.Clid;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Interface
{
    public interface ICorporateQuery
    {
        Task<CorporateData> Handle(string cbaClid, CancellationToken cancellationToken);
    }
}
