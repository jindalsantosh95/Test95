using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper
{
    public static class CheckInMessageHelper
    {
        private static readonly Dictionary<string, Tuple<string, string>> StatusMessagesJson =
            Json.Deserialize<Dictionary<string, Tuple<string, string>>>(ConfigReader.ReadAllText(EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath, Constants.Application.CheckInStatusCodes));

        private static readonly Dictionary<string, Tuple<string, string>> TrmErrorMessagesJson =
            Json.Deserialize<Dictionary<string, Tuple<string, string>>>(ConfigReader.ReadAllText(EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath, Constants.Application.TrmPatchExceptions));

        public static Tuple<string, string> GetStatusMessage(string code)
        {
            return code != null && StatusMessagesJson.ContainsKey(code) ? StatusMessagesJson[code] : StatusMessagesJson["defaultMessage"];
        }
        public static Tuple<string, string> GetTrmErrorMessage(string code)
        {
            return code != null && TrmErrorMessagesJson.ContainsKey(code) ? TrmErrorMessagesJson[code] : TrmErrorMessagesJson["defaultMessage"];
        }

        public static Tuple<string, string> GetTrmErrorMessageTravelRequirements(string code)
        {
            return code != null && TrmErrorMessagesJson.ContainsKey(code) ? TrmErrorMessagesJson[code] : TrmErrorMessagesJson["defaultMessageTravelRequirements"];
        }

        public static IEnumerable<ErrorDetails> GetNoReturnDateandSecondaryDocErrorMessages(string code)
        {

            return
                [
                ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(code), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault(),
                ];
        }
    }
}
