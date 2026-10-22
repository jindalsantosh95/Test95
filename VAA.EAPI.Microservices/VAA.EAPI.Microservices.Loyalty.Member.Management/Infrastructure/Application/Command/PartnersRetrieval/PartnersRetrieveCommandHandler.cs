using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.PartnersRetrieval
{
    public class PartnersRetrieveCommandHandler : IRequestHandler<PartnersRetrieveCommand, Result<Response<PartnersResponse>>>
    {
        private readonly ICachedPartners _cachedPartners;
        private readonly ILogger<PartnersRetrieveCommandHandler> _logger;

        public PartnersRetrieveCommandHandler(ICachedPartners cachedPartners, ILogger<PartnersRetrieveCommandHandler> logger)
        {
            _cachedPartners = cachedPartners ?? throw new ArgumentNullException(nameof(cachedPartners));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<PartnersResponse>>> Handle(PartnersRetrieveCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Partners Retrieval command handler process started");
            try
            {
                var partnersList = await _cachedPartners.GetCachedPartnersReferenceData();
                return partnersList;
            }
            finally
            {
                _logger.LogInformation("Partners Retrieval command handler process ended");
            }
        }
    }
}
