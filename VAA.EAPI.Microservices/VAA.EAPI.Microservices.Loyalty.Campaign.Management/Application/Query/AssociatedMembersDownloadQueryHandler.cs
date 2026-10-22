using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignsDownload;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class AssociatedMembersDownloadQueryHandler : IRequestHandler<AssociatedMembersDownloadQuery, Result<Response<AssociatedMembersDownloadResponse>>>
    {
        private readonly IReportingDBServices _reportingDBServices;
        private readonly IAssociatedMemberListDownloadService _assocMemListDownloadService;
        private readonly ILogger<AssociatedMembersDownloadQueryHandler> _logger;

        public AssociatedMembersDownloadQueryHandler(IReportingDBServices reportingDBServices, IAssociatedMemberListDownloadService assocMemListDownloadService, ILogger<AssociatedMembersDownloadQueryHandler> logger)
        {
            _reportingDBServices = reportingDBServices ?? throw new ArgumentNullException(nameof(reportingDBServices));
            _assocMemListDownloadService = assocMemListDownloadService ?? throw new ArgumentNullException(nameof(assocMemListDownloadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<AssociatedMembersDownloadResponse>>> Handle(AssociatedMembersDownloadQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.CampaignCode))
                {
                    List<AssociatedMemberData> associatedMembersList = await _reportingDBServices.GetAssociatedMembersData(request.CampaignCode);

                    if (associatedMembersList.Count > 0)
                    {
                        return new Response<AssociatedMembersDownloadResponse>
                        {
                            Status = Status.OK,
                            Value = new AssociatedMembersDownloadResponse { AssociatedMembersFileURL = await _assocMemListDownloadService.CreateXlStatementAndUploadToBlob(request.CampaignCode, associatedMembersList) },
                            IgnoreNullValues = false
                        };
                    }
                    else
                    {
                        _logger.LogInformation("Download Associated Members Query Handler is Failed");
                        return Result.Failure<Response<AssociatedMembersDownloadResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.NotFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                    }

                }
                else
                {
                    return Result.Success(new Response<AssociatedMembersDownloadResponse>
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Download Associated Members Query Handler");
                return new Response<AssociatedMembersDownloadResponse>
                {
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath),
                    IgnoreNullValues = false
                };
            }
        }
    }
}
