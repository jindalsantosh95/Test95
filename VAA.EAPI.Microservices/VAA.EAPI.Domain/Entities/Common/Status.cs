namespace VAA.EAPI.Domain.Entities.Common
{
    public enum Status
    {
        Accepted = 0,
        Error = 1,
        BusinessException = 2,
        BusinessWarnings = 3,
        OK = 4,
        Unauthorized = 5,
        Forbidden = 6,
        BadRequest = 7,
        RedirectMethod = 8,
        NotFound = 9,
        TooManyRequests = 11,
        BadGateway = 12,
        ServiceUnavailable = 13,
        GatewayTimeout = 14
    }
}
