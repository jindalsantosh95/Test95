using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionsDownload;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class PromotionDownloadQueryHandler : IRequestHandler<PromotionDownloadQuery, Result<Response<PromotionsDownloadResponse>>>
    {
        private readonly ICommonRefDBServices _referenceDBService;
        private readonly IPromotionListDownloadService _promotionListDownloadService;
        private readonly ILogger<PromotionDownloadQueryHandler> _logger;
        public PromotionDownloadQueryHandler(ICommonRefDBServices referenceDBService, IPromotionListDownloadService promotionListDownloadService, ILogger<PromotionDownloadQueryHandler> logger)
        {
            _referenceDBService = referenceDBService ?? throw new ArgumentNullException(nameof(referenceDBService));
            _promotionListDownloadService = promotionListDownloadService ?? throw new ArgumentNullException(nameof(promotionListDownloadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<PromotionsDownloadResponse>>> Handle(PromotionDownloadQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving Enrolled Member Details with Promotion");
                Result<Response<string>> response = new Result<Response<string>>();

                var promotionEnrolledMemberList = await _referenceDBService.GetPromotionslist();
                if (promotionEnrolledMemberList != null)
                {
                    response = await _promotionListDownloadService.CreateXlStatementAndUploadToBlob(promotionEnrolledMemberList);
                }
                var result = new Response<PromotionsDownloadResponse>
                {
                    Status = Status.OK,
                    Value = new PromotionsDownloadResponse
                    { PromotionsFileURL = response.Value.Value }
                };
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Download Promotions Response Query Handler");
                return new Response<PromotionsDownloadResponse>
                {
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath),
                    IgnoreNullValues = false
                };
            }
            finally
            {
                _logger.LogInformation($"Get Enrolled Member Details Response Query Handler with Promotion id:");
            }
        }
    }
}
