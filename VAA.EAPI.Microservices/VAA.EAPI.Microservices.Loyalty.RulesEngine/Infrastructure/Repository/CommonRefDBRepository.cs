using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository
{
    public class CommonRefDBRepository : ICommonRefDBRepository
    {
        private readonly ICommonRefDBServices _commonRefDBServices;
        public CommonRefDBRepository(ICommonRefDBServices commonRefDBServices)
        {
            _commonRefDBServices = commonRefDBServices ?? throw new ArgumentNullException(nameof(commonRefDBServices));
        }

        public async Task<string> GetPartnerId(string partnerCode)
        {
            return await _commonRefDBServices.GetPartnerDetailsByPartnerCode(partnerCode);
        }

    }
}
