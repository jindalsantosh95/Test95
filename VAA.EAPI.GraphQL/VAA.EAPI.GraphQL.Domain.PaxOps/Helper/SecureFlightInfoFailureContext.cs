using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public sealed class SecureFlightInfoFailureContext
    {
        public string? PnrLocator { get; init; }
        public string? SecureFlightReservationId { get; init; }
        public LoggingFailureContext LoggingFailureContext { get; init; }
        public string Operation { get; init; } = string.Empty;
        public string DefaultMessage { get; init; } = SecureFlightInfoErrorMessages.DefaultFailureMessage;
        public string DefaultTitle { get; init; } = SecureFlightInfoErrorMessages.DefaultFailureTitle;
        public string DefaultCode { get; init; } = "500";
        public bool ReturnParsedError { get; init; } = false;
    }
}
