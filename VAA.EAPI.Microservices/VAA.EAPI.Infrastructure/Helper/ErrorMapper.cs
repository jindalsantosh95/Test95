using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Infrastructure.Helper
{
    public static class ErrorMapper
    {
        public static List<ErrorDetails> MapCsharpExceptionToEAPIException(Exception ex, string guid, string errorFilePath)
        {
            string json = ConfigReader.ReadAllText(Constants.Constants.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            var code = ex.InnerException == null ? ex.GetType().Name : ex.GetType().Name + "+" + ex.InnerException.GetType().Name;
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == code);

            if (errors == null) return MapTechnicalExceptionToEAPIException(Constants.Constants.TechnicalError, guid, errorFilePath);

            return new List<ErrorDetails>
            {
                 new ErrorDetails
                 {
                     Id = guid,
                     Code = errors?.Code,
                     DeveloperMessage = $"{errors.DeveloperMessage} | TimeStamp: {DateTime.UtcNow}",
                     ErrorSource = errors?.ErrorSource,
                     Message = errors?.Message
                 }
            };
        }


        public static List<ErrorDetails> MapBusinessExceptionToEAPIException(Exception ex, Guid guid, string errorFilePath) => MapBusinessExceptionToEAPIException(ex, guid.ToString(), errorFilePath);
        public static List<ErrorDetails> MapBusinessExceptionToEAPIException(Exception ex, string guid, string errorFilePath) => MapBusinessExceptionToEAPIException(ex.Message, guid, errorFilePath);
        public static List<ErrorDetails> MapBusinessExceptionToEAPIException(string message, string guid, string errorFilePath)
        {
            string json = ConfigReader.ReadAllText(Constants.Constants.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.ThirdPartyServiceErrorCode == message.ToString());

            if (errors == null) return MapTechnicalExceptionToEAPIException(Constants.Constants.TechnicalError, guid, errorFilePath);

            return new List<ErrorDetails>
            {
                 new ErrorDetails
                 {
                     Id = guid,
                     Code = errors?.Code,
                     DeveloperMessage = $"{errors.DeveloperMessage} | TimeStamp: {DateTime.UtcNow}",
                     ErrorSource = errors?.ErrorSource,
                     Message = errors?.Message,
                     Title = errors?.Title
                 }
            };
        }

        public static List<ErrorDetails> MapTechnicalExceptionToEAPIException(string errorCode, string guid, string errorFilePath)
        {
            string json = ConfigReader.ReadAllText(Constants.Constants.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == errorCode);

            return new List<ErrorDetails>
            {
                new ErrorDetails
                {
                    Id = guid,
                    Code = errors?.Code,
                    DeveloperMessage = $"{errors?.DeveloperMessage} | TimeStamp: {DateTime.UtcNow}",
                    ErrorSource = errors?.ErrorSource,
                    Message = errors?.Message
                }
            };
        }

        public static List<ErrorDetails> MapDeltaExceptionToEAPIException(string errorCode, string guid, string errorFilePath, string message, string developerMessage = null)
        {
            string json = ConfigReader.ReadAllText(Constants.Constants.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == errorCode);

            return new List<ErrorDetails>
            {
                new ErrorDetails
                {
                    Id = guid,
                    Code = errors?.Code,
                    DeveloperMessage = developerMessage != null ? $"{developerMessage} | TimeStamp: {DateTime.UtcNow}" : $"TimeStamp: {DateTime.UtcNow}",
                    ErrorSource = errors?.ErrorSource,
                    Message = message
                }
            };
        }

        public static List<ErrorDetails> MapITAExceptionToEAPIException(string errorCode, string guid, string errorFilePath, string message)
        {
            string json = ConfigReader.ReadAllText(Constants.Constants.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == errorCode);

            return new List<ErrorDetails>
            {
                new ErrorDetails
                {
                    Id = guid,
                    Code = errors?.Code,
                    DeveloperMessage = $"TimeStamp: {DateTime.UtcNow}",
                    ErrorSource = errors?.ErrorSource,
                    Message = $"{message}"
                }
            };
        }

        public static ErrorDetails MapCPDExceptionToEAPIException(string errorCode, string guid, string errorFilePath, string message)
        {
            string json = ConfigReader.ReadAllText(Constants.Constants.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == errorCode);

            return new ErrorDetails
            {
                Id = guid,
                Code = errors?.Code,
                DeveloperMessage = $"TimeStamp: {DateTime.UtcNow}",
                ErrorSource = errors?.ErrorSource,
                Message = $"{message}"
            };
        }

        public static List<ErrorDetails> MapIFGExceptionToEAPIException(string errorCode, string guid, string errorFilePath, string message)
        {
            string json = ConfigReader.ReadAllText(Constants.Constants.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == errorCode);

            if (errors == null) return MapTechnicalExceptionToEAPIException(Constants.Constants.TechnicalError, guid, errorFilePath);
            return new List<ErrorDetails>
            {
                new ErrorDetails
                {
                    Id = guid,
                    Code = errors?.Code,
                    DeveloperMessage = $"TimeStamp: {DateTime.UtcNow}",
                    ErrorSource = errors?.ErrorSource,
                    Message = $"{message}"
                }
            };
        }
    }
}
