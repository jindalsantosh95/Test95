using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper
{
    public static class ErrorMapperHelper
    {

        public static List<ErrorDetails> MapBusinessExceptionToEAPIException(Exception ex, string guid)
        {
            IEnumerable<Errors> ErrorObject = GetErrorJSONObject();
            Errors errors = ErrorObject.FirstOrDefault(f => f.ThirdPartyServiceErrorCode == ex.Message.ToString());

            if (errors == null)
                return MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, guid);

            return new List<ErrorDetails>()
            {
                new()
                {
                    Code = errors?.Code,
                    DeveloperMessage =  $"{errors?.DeveloperMessage}   ErrorID : {guid}  TimetStamp: {DateTime.UtcNow.ToString()}",
                    ErrorSource = errors?.ErrorSource,
                    Message = errors?.Message
                }
            };
        }

        public static List<ErrorDetails> MapTechnicalExceptionToEAPIException(string errorCode, string guid)
        {
            IEnumerable<Errors> ErrorObject = GetErrorJSONObject();
            Errors errors = ErrorObject.FirstOrDefault(e => e.Code == errorCode);

            return new List<ErrorDetails>()
            {
                new()
                {
                    Code = errors?.Code,
                    DeveloperMessage = $"{errors?.DeveloperMessage}   ErrorID : {guid}  TimetStamp: {DateTime.UtcNow.ToString()}",
                    ErrorSource = errors?.ErrorSource,
                    Message = errors?.Message
                }
            };

        }

        public static IEnumerable<ErrorDetails> MapValidationError(string errorCode, string guid)
        {
            IEnumerable<Errors> ErrorObject = GetErrorJSONObject();
            Errors errors = ErrorObject.FirstOrDefault(e => e.Code == errorCode);

            return new ErrorDetails[]
            {
                new()
                {
                    Code = errors?.Code,
                    DeveloperMessage = $"{errors?.DeveloperMessage}   ErrorID : {guid}  TimetStamp: {DateTime.UtcNow.ToString()}",
                    ErrorSource = errors?.ErrorSource,
                    Message = errors?.Message
                }
            };
        }

        public static IEnumerable<Errors> GetErrorJSONObject()
        {
            string json = ConfigReader.ReadAllText(EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

            return Json.Deserialize<Errors[]>(json);
        }

        public static List<ErrorDetails> ErrorResponseMap(IEnumerable<ErrorDetails> errorDetails)
        {
            return errorDetails.Select(e => new ErrorDetails()
            {
                Code = e.Code,
                DeveloperMessage = e.DeveloperMessage,
                ErrorSource = e.ErrorSource,
                Message = e.Message
            })
                .ToList();
        }

        public static Response<EnrolmentResponse> ErrorResponseMapToEnrolmentResponse(IEnumerable<ErrorDetails> errorDetails)
        {
            return new Response<EnrolmentResponse>()
            {
                Errors = ErrorResponseMap(errorDetails)
            };
        }

        public static List<ErrorDetails> MapBusinessExceptionToEAPIExceptionwithFCNumber(Exception ex, string guid, string fcnumber)
        {
            IEnumerable<Errors> ErrorObject = GetErrorJSONObject();
            Errors errors = ErrorObject.FirstOrDefault(e => e.ThirdPartyServiceErrorCode == ex.Message.ToString());

            if (errors == null)
                return MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, guid);

            return new List<ErrorDetails>()
            {
                new()
                {
                    Code = errors?.Code,
                    DeveloperMessage = $"{errors?.DeveloperMessage}   ErrorID : {guid}  TimetStamp: {DateTime.UtcNow.ToString()}",
                    ErrorSource = errors?.ErrorSource,
                    Message = errors?.Message+ " FCNumber : " +fcnumber
                }
            };
        }
    }
}
