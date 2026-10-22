using System;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Brand.Helper;

public class ErrorMapper
{
    public static ErrorDetails MapAtpcoErrorToEapiErrorDetails(string message, string guid)
    {
        return new ErrorDetails
        {
            Code = "BMS-AC",
            DeveloperMessage = $"{message} | ErrorID: {guid} | TimeStamp: {DateTime.UtcNow}",
            ErrorSource = "ATPCO",
            Message = $"{message}"
        };
    }
}
