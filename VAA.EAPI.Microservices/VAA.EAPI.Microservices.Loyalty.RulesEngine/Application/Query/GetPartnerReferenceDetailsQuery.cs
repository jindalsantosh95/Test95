using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Query
{
    public class GetPartnerReferenceDetailsQuery : IRequest<string>
    {
        public string PartnerCode { get; set; }
    }

    public class GetPartnerReferenceDetailsQueryHandler : IRequestHandler<GetPartnerReferenceDetailsQuery, string>
    {
        private readonly ICachedVSPartner _cachedVSPartner;
        private readonly ILogger<GetPartnerReferenceDetailsQuery> _logger;
        public GetPartnerReferenceDetailsQueryHandler(ICachedVSPartner cachedVSPartner, ILogger<GetPartnerReferenceDetailsQuery> logger)
        {
            _cachedVSPartner = cachedVSPartner ?? throw new ArgumentNullException(nameof(cachedVSPartner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<string> Handle(GetPartnerReferenceDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Get Birthday Bonus Partner Id Query Handler - Start");
                return await _cachedVSPartner.GetCachedVSPartnerData(request.PartnerCode);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get Birthday Bonus Partner Id Query Handler failed due to Exception");
                throw;
            }
            finally
            {
                _logger.LogInformation($"Get Birthday Bonus Partner Id Query Handler - End");
            }
        }
    }
}
