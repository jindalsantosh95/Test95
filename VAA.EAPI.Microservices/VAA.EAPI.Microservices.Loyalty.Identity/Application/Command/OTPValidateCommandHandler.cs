using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;


namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class OTPValidateCommandHandler : IRequestHandler<OTPValidateCommand, Response<string>>
    {
        private readonly ILogger<OTPValidateCommandHandler> _logger;
        private readonly ITableStorageService<GenerateOtpDbEntities> _tableStorageService;

        public OTPValidateCommandHandler(ITableStorageService<GenerateOtpDbEntities> tableStorageService, ILogger<OTPValidateCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        }

        public async Task<Response<string>> Handle(OTPValidateCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"OTP Validate command Handler. Request(email Id): {request.EmailId}");

            try
            {
                string emailId = request.EmailId.ToLower();
                int requestOtp;

                if (string.IsNullOrEmpty(emailId))
                    return GetErrorResponse(ErrorCodes.EmailIdMissing, _logger);

                if (!string.IsNullOrEmpty(request.Otp))
                {
                    bool otpConvert = int.TryParse(request.Otp, out requestOtp);
                    if (!otpConvert || (request.Otp.Length != ConfigReader.Get<int>(Constants.Environment.OTPDigitLength) && request.Otp.Length != ConfigReader.Get<int>(Constants.Environment.OtpEntraDigitLength)))
                        return GetErrorResponse(ErrorCodes.MissingOTP, _logger);
                }

                else
                    return GetErrorResponse(ErrorCodes.MissingOTP, _logger);

                var otpGenerateData = await _tableStorageService.RetrieveAsync(Constants.Application.OtpGenerationEvent, emailId);

                if (otpGenerateData != null)
                {
                    if (Convert.ToDateTime(otpGenerateData.OTPExpiryTime) < DateTime.UtcNow)
                    {
                        await _tableStorageService.DeleteAsync(otpGenerateData);
                        return GetErrorResponse(ErrorCodes.ExpiredOTP, _logger);
                    }

                    if (otpGenerateData.OTPValidationCount >= ConfigReader.Get<int>(Constants.Environment.OtpLimit))
                    {
                        await _tableStorageService.DeleteAsync(otpGenerateData);
                        return GetErrorResponse(ErrorCodes.OTPValidationMaxAttemptReached, _logger);
                    }

                    if (otpGenerateData.OTP == requestOtp)
                    {
                        await _tableStorageService.DeleteAsync(otpGenerateData);

                        return new Response<string>()
                        {
                            Status = Status.OK,
                            Value = await ValidationTokenHelper.CreateToken(emailId),
                            Errors = null
                        };
                    }

                    else
                    {
                        await _tableStorageService.InsertOrMergeAsync(GenerateOtpDbEntitiesMapper.MapToDBEntityList(emailId, otpGenerateData.OTP, otpGenerateData.OTPGenerationCount, ++otpGenerateData.OTPValidationCount));
                        return GetErrorResponse(ErrorCodes.InvalidOtp, _logger);
                    }
                }

                else
                {
                    return GetErrorResponse(ErrorCodes.UnavailableData, _logger);
                }

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"OTP validation failed due to an unexpected technical error.");
                return GetErrorResponse(ErrorCodes.UnavailableDataSource, _logger);
            }


        }


        private static Response<string> GetErrorResponse(string errorCode, ILogger<OTPValidateCommandHandler> logger)
        {
            List<ErrorDetails> errorList = new();
            errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
            logger.LogError($"Error: {Json.Serialize(errorList)}");
            return new Response<string>()
            {
                Status = Status.BusinessException,
                Value = null,
                Errors = errorList
            };
        }
    }

}
