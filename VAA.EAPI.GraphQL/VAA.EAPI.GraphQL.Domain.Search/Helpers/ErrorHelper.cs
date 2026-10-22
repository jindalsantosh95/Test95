using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferSearch;

namespace VAA.EAPI.GraphQL.Domain.Search.Helpers
{
    public class ErrorHelper
    {
        public static IEnumerable<ErrorDetails> OfferErrorMapper(DeltaErrorResponse error)
        {
            string json = ConfigReader.ReadAllText(Common.Constants.Application.FunctionAppDirPath, Constants.Application.ErrorFilePath);
            List<DeltaErrorDetails> errorConfigObject = Json.Deserialize<List<DeltaErrorDetails>>(json);
            ErrorDetails defaulError = errorConfigObject.First(e => e.Code == Constants.ErrorCodes.DefaultErrorCode);


            if (error.Code == Constants.ErrorCodes.TechnicalError)
            {
                return new List<ErrorDetails>()
                {
                    new ErrorDetails
                    {
                        Code = defaulError.Code,
                        DeveloperMessage = $"ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                        Message = defaulError.Message
                    }

                };

            }
            var errorCode = error.MoreInfo?.FirstOrDefault()?.Code ?? error.Code;
            ErrorDetails errors = errorConfigObject.FirstOrDefault(e => e.ThirdPartyServiceErrorCode == errorCode) ?? defaulError;

            return new List<ErrorDetails>()
            {
                new ErrorDetails
                {
                    Code = errors.Code,
                    DeveloperMessage = $"{errors.DeveloperMessage} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                    Message = errors.Message
                }
            };
        }
    }
}
