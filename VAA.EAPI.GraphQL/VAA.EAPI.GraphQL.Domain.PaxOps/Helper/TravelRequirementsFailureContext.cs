using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public sealed class TravelRequirementsFailureContext
    {
        public string? TravelRequirementsId { get; init; }
        public string? PnrLocator { get; init; }
        public LoggingFailureContext LoggingFailureContext { get; init; }
        public string Operation { get; init; } = string.Empty;
        public string DefaultMessage { get; init; } = string.Empty;
        public string DefaultTitle { get; init; } = string.Empty;
        public string DefaultCode { get; init; } = "500";
        public bool ReturnParsedError { get; init; } = true;
    }
}
