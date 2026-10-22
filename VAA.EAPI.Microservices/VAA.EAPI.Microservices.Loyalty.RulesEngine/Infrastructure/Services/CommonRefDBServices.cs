using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services
{
    public class CommonRefDBServices : ICommonRefDBServices
    {
        private readonly CommonReferenceDataContext _commonReferenceDataContext;
        private readonly ILogger<CommonRefDBServices> _logger;

        public CommonRefDBServices(CommonReferenceDataContext commonReferenceDataContext, ILogger<CommonRefDBServices> logger)
        {
            _commonReferenceDataContext = commonReferenceDataContext ?? throw new ArgumentNullException(nameof(commonReferenceDataContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetPartnerDetailsByPartnerCode(string partnerCode)
        {
            return (await _commonReferenceDataContext.PartnerProfile.FirstOrDefaultAsync(x => x.PartnerCode == partnerCode))?.RowId;
        }

    }
}
