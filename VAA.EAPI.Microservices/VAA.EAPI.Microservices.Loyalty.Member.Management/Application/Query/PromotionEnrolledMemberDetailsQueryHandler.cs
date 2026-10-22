using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class PromotionEnrolledMemberDetailsQueryHandler : IRequestHandler<PromotionEnrolledMemberDetailsQuery, Result<Response<PromotionEnrolledMemberDetailsResponse>>>
    {
        private readonly IReportingDBServices _reportingDBService;
        private readonly IPromotionEnrolledMemberService _promotionEnrolledMemberService;
        private readonly ILogger<PromotionEnrolledMemberDetailsQueryHandler> _logger;
        public PromotionEnrolledMemberDetailsQueryHandler(IReportingDBServices reportingDBService, IPromotionEnrolledMemberService promotionEnrolledMemberService, ILogger<PromotionEnrolledMemberDetailsQueryHandler> logger)
        {
            _reportingDBService = reportingDBService ?? throw new ArgumentNullException(nameof(reportingDBService));
            _promotionEnrolledMemberService = promotionEnrolledMemberService ?? throw new ArgumentNullException(nameof(promotionEnrolledMemberService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<PromotionEnrolledMemberDetailsResponse>>> Handle(PromotionEnrolledMemberDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving Enrolled Member Details with Promotion");
                Result<Response<string>> response = new Result<Response<string>>();
                var promotionEnrolledMemberDetailsRequestValidator = new PromotionEnrolledMemberDetailsQueryValidator().Validate(request);
                if (promotionEnrolledMemberDetailsRequestValidator.IsValid)
                {
                    var promotionEnrolledMemberList = await _reportingDBService.GetEnrolledMemberDetails(request.PromotionId);
                    if (promotionEnrolledMemberList.Any())
                    {
                        response = await _promotionEnrolledMemberService.CreateXlStatementAndUploadToBlob(promotionEnrolledMemberList);
                        return new Response<PromotionEnrolledMemberDetailsResponse>
                        {
                            Status = Status.OK,
                            Value = new PromotionEnrolledMemberDetailsResponse
                            { PromotionId = request.PromotionId, EnrolledMembersListFileURL = response.Value.Value }
                        };
                    }
                    else
                    {
                        return new Response<PromotionEnrolledMemberDetailsResponse>
                        {
                            Status = Status.OK,
                            Value = new PromotionEnrolledMemberDetailsResponse
                            { PromotionId = request.PromotionId, EnrolledMembersListFileURL = "", Message = Constants.Application.MembersNotFound }
                        };
                    }

                }
                else
                {
                    return new Response<PromotionEnrolledMemberDetailsResponse>
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                        Status = Status.BusinessException,
                        IgnoreNullValues = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Get Enrolled Member Details Response Query Handler with Promotion id:{request.PromotionId}");
                return new Response<PromotionEnrolledMemberDetailsResponse>
                {
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath),
                    IgnoreNullValues = false
                };
            }
            finally
            {
                _logger.LogInformation($"Get Enrolled Member Details Response Query Handler with Promotion id:{request.PromotionId}");
            }
        }
    }
}
