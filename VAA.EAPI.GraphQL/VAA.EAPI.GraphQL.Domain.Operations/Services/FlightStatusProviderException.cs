#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Services;

internal sealed class FlightStatusProviderException : Exception
{
    internal FlightStatusProviderException(FlightStatusProviderFailure failure, Exception? innerException = null)
        : base("Flight-status dependency request failed.", innerException)
    {
        Failure = failure;
    }

    internal FlightStatusProviderFailure Failure { get; }
}
