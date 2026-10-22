using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Common.Helper
{
    public class ErrorMapper
    {
        public static List<ErrorDetails> MapBusinessExceptionToEAPIException(Exception ex, string guid, string errorFilePath)
        {
            string json = ConfigReader.ReadAllText(Constants.Application.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.ThirdPartyServiceErrorCode == ex.Message.ToString());

            if (errors == null) return MapTechnicalExceptionToEAPIException(Constants.Application.TechnicalError, guid, errorFilePath);

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
            string json = ConfigReader.ReadAllText(Constants.Application.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == errorCode);

            return new List<ErrorDetails>
            {
                new ErrorDetails
                {
                    Code = errors?.Code,
                    DeveloperMessage = $"{errors?.DeveloperMessage} | ErrorID: {guid} | TimeStamp: {DateTime.UtcNow}",
                    ErrorSource = errors?.ErrorSource,
                    Message = errors?.Message
                }
            };
        }

        public static List<ErrorDetails> MapDeltaExceptionToEAPIException(string errorCode, string guid, string errorFilePath, string message)
        {
            string json = ConfigReader.ReadAllText(Constants.Application.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == errorCode);

            return new List<ErrorDetails>
            {
                new ErrorDetails
                {
                    Code = errors?.Code,
                    DeveloperMessage = $"{errors?.DeveloperMessage} | ErrorID: {guid} | TimeStamp: {DateTime.UtcNow}",
                    ErrorSource = errors?.ErrorSource,
                    Message = message
                }
            };
        }
        public static List<ErrorDetails> MapITAExceptionToEAPIException(string errorCode, string guid, string errorFilePath, string message)
        {
            string json = ConfigReader.ReadAllText(Constants.Application.FunctionAppDirPath, errorFilePath);
            List<ErrorConfig> errorConfigObject = Json.Deserialize<List<ErrorConfig>>(json);
            ErrorConfig errors = errorConfigObject.FirstOrDefault(e => e.Code == errorCode);

            return new List<ErrorDetails>
            {
                new ErrorDetails
                {
                    Code = errors?.Code,
                    DeveloperMessage = $"{message} | ErrorID: {guid} | TimeStamp: {DateTime.UtcNow}",
                    ErrorSource = errors?.ErrorSource,
                    Message = $"{message}"
                }
            };
        }
    }
}
