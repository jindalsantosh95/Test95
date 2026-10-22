using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface ITelemetryService
    {
       Task TrackTelemetry(TelemetryEvent telemetryEvent);
    }
}
