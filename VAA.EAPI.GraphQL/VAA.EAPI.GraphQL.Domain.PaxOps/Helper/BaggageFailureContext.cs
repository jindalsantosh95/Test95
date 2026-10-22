using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public sealed class BaggageFailureContext
    {
        public string? CheckInId { get; init; }
        public bool? IsPaidBaggage { get; init; }
        public LoggingFailureContext LoggingFailureContext { get; init; }
        public string Operation { get; init; } = string.Empty;
        public string DefaultMessage { get; init; } = Application.BaggageCheckInFailDefaultMessage;
        public string DefaultTitle { get; init; } = Application.BaggageCheckInFailDefaultTiTle;
        public string DefaultCode { get; init; } = "500";
    }
}
