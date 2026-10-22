using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.Enrolment
{
    public class ErrorMapper : IErrorMapper
    {
        readonly StringBuilder _logText = new StringBuilder();
        private readonly ILogger<ErrorMapper> _logger;
        public ErrorMapper(ILogger<ErrorMapper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public Result<Response<EnrolmentResponse>> ErrorsMapper(Exception exception, string description, string errorCodeDescription, string errorCode)
        {
            Result<Response<EnrolmentResponse>> enrolmentResponse = new Result<Response<EnrolmentResponse>>();
            List<ErrorDetails> errors;
            string guid = Guid.NewGuid().ToString();
            _logText.AppendLine($"{description}");

            if (string.IsNullOrEmpty(errorCode))
            {
                errors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(exception, guid);
                enrolmentResponse = ErrorMapperHelper.ErrorResponseMapToEnrolmentResponse(errors);
                enrolmentResponse.Value.Status = Status.BusinessException;
                _logger.LogInformation($"ErrorID  {guid}   {errorCodeDescription} {errors?.FirstOrDefault()?.Code} ");
                return enrolmentResponse;
            }

            errors = ErrorMapperHelper.MapTechnicalExceptionToEAPIException(errorCode, guid);
            enrolmentResponse = ErrorMapperHelper.ErrorResponseMapToEnrolmentResponse(errors);
            enrolmentResponse.Value.Status = Status.Error;
            _logger.LogInformation($"ErrorID  {guid} Exception  MemberEnrolmentService.Enrolment API Exception with Error code: {errors?.FirstOrDefault()?.Code} || Message : {exception?.Message} || StackTrace: {exception?.StackTrace} || InnerException: {exception?.InnerException}");
            return enrolmentResponse;
        }
    }
}
