using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Helper
{
    public static class ErrorMapperHelper
    {

        public static List<ErrorDetails> MapBusinessExceptionToEAPIException(Exception ex, string guid)
        {

            List<Errors> ErrorObject = GetErrorJSONObject();
            Errors errors = ErrorObject.FirstOrDefault(f => f.ThirdPartyServiceErrorCode == ex.Message.ToString());

            if (errors == null)
            {
                return MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, guid);
            }
            return new List<ErrorDetails>()
                {
                 new ErrorDetails()
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
            List<Errors> ErrorObject = GetErrorJSONObject();
            Errors errors = ErrorObject.FirstOrDefault(e => e.Code == errorCode);
            return
                new List<ErrorDetails>()
                {
                 new ErrorDetails()
                 {
                  Code = errors?.Code,
                  DeveloperMessage = $"{errors?.DeveloperMessage}   ErrorID : {guid}  TimetStamp: {DateTime.UtcNow.ToString()}",
                  ErrorSource = errors?.ErrorSource,
                   Message = errors?.Message

                 }
            };

        }

        public static List<ErrorDetails> MapValidationError(string errorCode, string guid)
        {
            List<Errors> ErrorObject = GetErrorJSONObject();
            Errors errors = ErrorObject.FirstOrDefault(e => e.Code == errorCode);
            return
                new List<ErrorDetails>()
                {
                 new ErrorDetails()
                 {
                  Code = errors?.Code,
                  DeveloperMessage = $"{errors?.DeveloperMessage}   ErrorID : {guid}  TimetStamp: {DateTime.UtcNow.ToString()}",
                  ErrorSource = errors?.ErrorSource,
                   Message = errors?.Message

                 }
            };

        }

        public static List<Errors> GetErrorJSONObject()
        {
            string json = ConfigReader.ReadAllText(EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

            return Json.Deserialize<List<Errors>>(json);
        }

        public static Response<EnrolmentResponse> ErrorResponseMap(List<ErrorDetails> errorDetails)
        {
            List<ErrorDetails> responseErrors = new List<ErrorDetails>();
            errorDetails.ForEach(e =>
            {
                responseErrors.Add(new ErrorDetails()
                {
                    Code = e.Code,
                    DeveloperMessage = e.DeveloperMessage,
                    ErrorSource = e.ErrorSource,
                    Message = e.Message
                });
            });
            return new Response<EnrolmentResponse>()
            {
                Errors = responseErrors
            };

        }

        public static List<ErrorDetails> MapBusinessExceptionToEAPIExceptionwithFCNumber(Exception ex, string guid, string fcnumber)
        {

            List<Errors> ErrorObject = GetErrorJSONObject();
            Errors errors = ErrorObject.FirstOrDefault(e => e.ThirdPartyServiceErrorCode == ex.Message.ToString());
            if (errors == null)
            {
                return MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, guid);
            }
            return new List<ErrorDetails>()
                {
                 new ErrorDetails()
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
