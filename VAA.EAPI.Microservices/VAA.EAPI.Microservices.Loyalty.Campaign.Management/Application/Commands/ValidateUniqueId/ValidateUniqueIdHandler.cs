using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Constants;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.ValidateUniqueId;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.ValidateUniqueId.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.ValidateUniqueId
{
    public class ValidateUniqueIdHandler : IValidateUniqueIdHandler
    {
        private readonly IValidateUniqueIdRepository _validateUniqueIdRepository;
        private readonly ILogger<ValidateUniqueIdHandler> _logger;

        public ValidateUniqueIdHandler(IValidateUniqueIdRepository validateUniqueIdRepository, ILogger<ValidateUniqueIdHandler> logger)
        {
            _validateUniqueIdRepository = validateUniqueIdRepository ?? throw new ArgumentNullException(nameof(validateUniqueIdRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<ValidateUniqueIdResponse>>> Handle(ValidateUniqueIdCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.UniqueId))
                {
                    return Result.Success(new Response<ValidateUniqueIdResponse>
                    {
                        Status = Status.BadRequest,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }
                var uniqueIdEntity = await _validateUniqueIdRepository.GetByUniqueIdAsync(request.UniqueId.ToUpper());
                if (uniqueIdEntity == null)
                {
                    return Result.Success(new Response<ValidateUniqueIdResponse>
                    {
                        Status = Status.BusinessException,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.PcmUniqueIdInvalid),Guid.NewGuid().ToString(),VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }
                var hasFcNumber = !string.IsNullOrWhiteSpace(uniqueIdEntity.FCNumber) && !string.Equals(uniqueIdEntity.FCNumber, "null", StringComparison.OrdinalIgnoreCase);
                var hasUsedTimestamp = !string.IsNullOrWhiteSpace(uniqueIdEntity.UsedTimestamp) && !string.Equals(uniqueIdEntity.UsedTimestamp, "null", StringComparison.OrdinalIgnoreCase);

                DateTime? claimedDateTime = null;

                if (hasUsedTimestamp && DateTimeOffset.TryParse(uniqueIdEntity.UsedTimestamp, out var parsedUsedTimestamp))
                {
                    claimedDateTime = parsedUsedTimestamp.UtcDateTime;
                }
                return Result.Success(new Response<ValidateUniqueIdResponse>
                {
                    Status = Status.OK,
                    IgnoreNullValues = false,
                    Value = new ValidateUniqueIdResponse
                    {
                        Valid = true,
                        Claimed = hasFcNumber || hasUsedTimestamp,
                        FcNumber = hasFcNumber ? uniqueIdEntity.FCNumber : null,
                        ClaimedDateTime = claimedDateTime
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while validating campaign unique ID");
                return Result.Failure<Response<ValidateUniqueIdResponse>>(
                Json.Serialize(
                ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError,Guid.NewGuid().ToString(),VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
