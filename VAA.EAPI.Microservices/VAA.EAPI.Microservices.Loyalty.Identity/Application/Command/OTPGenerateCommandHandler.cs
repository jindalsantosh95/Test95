using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class OTPGenerateCommandHandler : OTPGenerateCommandHandlerBase, IOTPGenerateCommandHandler
    {
        public OTPGenerateCommandHandler(IOtpEmailEventPublisher otpEmailEventPublisher, ILogger<OTPGenerateCommand> logger, ITableStorageService<GenerateOtpDbEntities> tableStorageService)
            : base(otpEmailEventPublisher, logger, tableStorageService)
        {
        }

        public Task<Response<string>> Handle(OTPGenerateCommand request, CancellationToken cancellationToken)
        {
            return GenerateOtp(request?.EmailId, _ => Task.FromResult("OTP generated successfully"), Constants.Environment.RandomDigitMinValue, Constants.Environment.RandomDigitMaxValue, cancellationToken);
        }
    }

    public class OTPGenerateContinuationTokenCommandHandler : OTPGenerateCommandHandlerBase, IOTPGenerateContinuationTokenCommandHandler
    {
        public OTPGenerateContinuationTokenCommandHandler(IOtpEmailEventPublisher otpEmailEventPublisher, ILogger<OTPGenerateContinuationTokenCommand> logger, ITableStorageService<GenerateOtpDbEntities> tableStorageService)
            : base(otpEmailEventPublisher, logger, tableStorageService)
        {
        }

        public Task<Response<string>> Handle(OTPGenerateContinuationTokenCommand request, CancellationToken cancellationToken)
        {
            return GenerateOtp(request?.EmailId, emailID => ValidationTokenHelper.CreateToken(emailID, isContinuationToken: true), Constants.Environment.ContinuationTokenRandomDigitMinValue, Constants.Environment.ContinuationTokenRandomDigitMaxValue, cancellationToken);
        }
    }

    public abstract class OTPGenerateCommandHandlerBase
    {
        private readonly IOtpEmailEventPublisher _otpEmailEventPublisher;
        private readonly ILogger _logger;
        private readonly ITableStorageService<GenerateOtpDbEntities> _tableStorageService;

        protected OTPGenerateCommandHandlerBase(IOtpEmailEventPublisher otpEmailEventPublisher, ILogger logger, ITableStorageService<GenerateOtpDbEntities> tableStorageService)
        {
            _otpEmailEventPublisher = otpEmailEventPublisher ?? throw new ArgumentNullException(nameof(otpEmailEventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.OTPGenerateTableName);
        }

        protected async Task<Response<string>> GenerateOtp(string emailId, Func<string, Task<string>> successValueFactory, string randomDigitMinValueKey, string randomDigitMaxValueKey, CancellationToken cancellationToken)
        {

            try
            {

                bool IsValidEmailId = ValidateEmail.Validate(emailId);
                if (!IsValidEmailId)
                    return GetErrorResponse(ErrorCodes.InvalidorMissingEmailID);

                string emailID = emailId.ToLowerInvariant();
                                
                var otpGenerateData = await _tableStorageService.RetrieveAsync(Constants.Application.OtpGenerationEvent, emailID);

                int generatedOtp = RandomNumberGenerator.GetInt32(
                    ConfigReader.Get<int>(randomDigitMinValueKey),
                    ConfigReader.Get<int>(randomDigitMaxValueKey));

                if (otpGenerateData != null)
                {
                    if ((otpGenerateData.OTPGenerationCount >= ConfigReader.Get<int>(Constants.Environment.OtpLimit) && DateTime.UtcNow <= otpGenerateData.Timestamp.Add(ConfigReader.Get<TimeSpan>(Constants.Environment.OtpExpiryTimeSpan))))
                        return GetErrorResponse(ErrorCodes.MaxAttemptReached);

                    if ((otpGenerateData.OTPGenerationCount < ConfigReader.Get<int>(Constants.Environment.OtpLimit) && DateTime.UtcNow < Convert.ToDateTime(otpGenerateData.OTPExpiryTime)))
                        await _tableStorageService.InsertOrMergeAsync(GenerateOtpDbEntitiesMapper.MapToDBEntityList(emailID, generatedOtp, ++otpGenerateData.OTPGenerationCount, 0));

                    else if ((otpGenerateData.OTPGenerationCount < ConfigReader.Get<int>(Constants.Environment.OtpLimit)))
                        await _tableStorageService.InsertOrMergeAsync(GenerateOtpDbEntitiesMapper.MapToDBEntityList(emailID, generatedOtp, ++otpGenerateData.OTPGenerationCount, 0));

                    if ((otpGenerateData.OTPGenerationCount == ConfigReader.Get<int>(Constants.Environment.OtpLimit) && DateTime.UtcNow > otpGenerateData.Timestamp.Add(ConfigReader.Get<TimeSpan>(Constants.Environment.OtpExpiryTimeSpan))))
                        await _tableStorageService.InsertOrMergeAsync(GenerateOtpDbEntitiesMapper.MapToDBEntityList(emailID, generatedOtp, 1, 0));
                }
                else
                    await _tableStorageService.InsertOrMergeAsync(GenerateOtpDbEntitiesMapper.MapToDBEntityList(emailID, generatedOtp, 1, 0));

                var correlationIdForEvents = Guid.NewGuid().ToString();
                var emailCloudEvent = EmailTriggeringEventMapper.EventDataMapper(correlationIdForEvents, emailID, generatedOtp.ToString(),ConfigReader.Get(Constants.Environment.OTPEmailTemplate));

                try
                {
                    await _otpEmailEventPublisher.Publish(emailCloudEvent, correlationIdForEvents, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "error occurred while publishing email events.");
                    return GetErrorResponse(ErrorCodes.OTPGenerationFail);
                }
                return new Response<string>()
                {
                    Status = Status.OK,
                    Value = await successValueFactory(emailID),
                    Errors = null
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OTP generation failed due to technical issue.");
                return GetErrorResponse(ErrorCodes.OTPGenerationFail);
            }
        }
        private static Response<string> GetErrorResponse(string errorCode)
        {
            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };

        }

    }
}
