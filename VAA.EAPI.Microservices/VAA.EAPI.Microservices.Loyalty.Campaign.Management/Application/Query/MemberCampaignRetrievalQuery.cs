using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CampaignRetrieve.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class MemberCampaignRetrievalQuery : IRequest<Result<Response<MemberCampaignResponse>>>
    {
        public string CustomerId { get; set; }
    }
    public class MemberCampaignRetrievalResponseQueryHandler : IRequestHandler<MemberCampaignRetrievalQuery, Result<Response<MemberCampaignResponse>>>
    {
        private readonly ICampaignRetrieveRepository _campaignRetrieveRepository;
        private readonly ILogger<MemberCampaignRetrievalResponseQueryHandler> _logger;
        public MemberCampaignRetrievalResponseQueryHandler(ICampaignRetrieveRepository campaignRetrieveRepository, ILogger<MemberCampaignRetrievalResponseQueryHandler> logger)
        {
            _campaignRetrieveRepository = campaignRetrieveRepository ?? throw new ArgumentNullException(nameof(campaignRetrieveRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<MemberCampaignResponse>>> Handle(MemberCampaignRetrievalQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var campaignUpdateRequestValidator = new MemberCampaignRetrievalQueryValidator().Validate(request);
                if (campaignUpdateRequestValidator.IsValid)
                {
                    _logger.LogInformation("Retrieving Member Campaign Details");
                    var memberCampaignRetrieveResponse = await _campaignRetrieveRepository.GetMemberCampaigns(request.CustomerId);
                    return memberCampaignRetrieveResponse;
                }
                else
                {
                    return Result.Failure<Response<MemberCampaignResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Campaign Retrieval Response Query Handler with CorrelationId:{request.CustomerId}");
                return Result.Failure<Response<MemberCampaignResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
