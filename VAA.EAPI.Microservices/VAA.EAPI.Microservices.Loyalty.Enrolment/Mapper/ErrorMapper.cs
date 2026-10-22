using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper
{
    public class ErrorMapper : IErrorMapper
    {
        readonly StringBuilder _logText = new StringBuilder();
        private readonly ILogger<EnrolmentEventCommandHandler> _logger;
        public ErrorMapper(ILogger<EnrolmentEventCommandHandler> logger)
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
                errors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(exception, guid);
                enrolmentResponse = Helper.ErrorMapperHelper.ErrorResponseMap(errors);
                enrolmentResponse.Value.Status = Status.BusinessException;
                _logger.LogInformation($"ErrorID  {guid}   {errorCodeDescription} {errors?.FirstOrDefault()?.Code} ");
            }
            else
            {
                errors = Helper.ErrorMapperHelper.MapTechnicalExceptionToEAPIException(errorCode, guid);
                enrolmentResponse = Helper.ErrorMapperHelper.ErrorResponseMap(errors);
                enrolmentResponse.Value.Status = Status.Error;
                _logger.LogInformation($"ErrorID  {guid} Exception  MemberEnrolmentService.Enrolment API Exception with Error code: {errors?.FirstOrDefault()?.Code} || Message : {exception?.Message} || StackTrace: {exception?.StackTrace} || InnerException: {exception?.InnerException}");
            }
            return enrolmentResponse;
        }
    }
}
