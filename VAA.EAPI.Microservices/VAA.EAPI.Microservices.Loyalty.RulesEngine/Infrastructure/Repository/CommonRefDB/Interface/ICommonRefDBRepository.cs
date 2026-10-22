
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository.CommonRefDB.Interface
{
    public interface ICommonRefDBRepository
    {
        Task<string> GetPartnerId(string partnerCode);
    }
}
