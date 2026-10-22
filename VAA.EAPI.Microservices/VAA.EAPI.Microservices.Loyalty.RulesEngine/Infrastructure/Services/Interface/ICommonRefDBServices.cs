

using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services.Interface
{
    public interface ICommonRefDBServices
    {
        Task<string> GetPartnerDetailsByPartnerCode(string partnerCode);
    }
}
