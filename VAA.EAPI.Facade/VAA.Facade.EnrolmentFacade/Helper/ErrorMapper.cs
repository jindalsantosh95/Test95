using System;
using System.Collections.Generic;
using System.Linq;
using VAA.Facade.EnrolmentFacade.Helper.Serialisation;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities;

namespace VAA.Facade.EnrolmentFacade.Helper
{
    public class ErrorMapper
    {
        public static List<ErrorDetails> MapBusinessExceptionToEAPIException(Exception ex, string guid, string errorFilePath)
        {
            string json = ConfigReader.ReadJsonFileContent(Constants.Constants.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.DeserializeObject<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.ThirdPartyServiceErrorCode == ex.Message.ToString());

            if (errors == null)
            {
                return MapTechnicalExceptionToEAPIException(Constants.Constants.TechnicalError, guid, errorFilePath);
            }

            return new List<ErrorDetails>
            {
                 new ErrorDetails
                 {
                     Code = errors?.Code,
                     DeveloperMessage = $"{errors.DeveloperMessage} | ErrorID: {guid} | TimeStamp: {DateTime.UtcNow}",
                     ErrorSource = errors?.ErrorSource,
                     Message = errors?.Message
                 }
            };
        }

        public static List<ErrorDetails> MapTechnicalExceptionToEAPIException(string errorCode, string guid, string errorFilePath)
        {
            string json = ConfigReader.ReadJsonFileContent(Constants.Constants.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.DeserializeObject<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == errorCode);

            return new List<ErrorDetails>
            {
                new ErrorDetails
                {
                    Code = errors?.Code,
                    DeveloperMessage = $"{errors.DeveloperMessage} | ErrorID: {guid} | TimeStamp: {DateTime.UtcNow}",
                    ErrorSource = errors?.ErrorSource,
                    Message = errors?.Message
                }
            };
        }
    }
}
