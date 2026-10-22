namespace VAA.EAPI.GraphQL.Domain.Operations.Services;

internal enum FlightStatusProviderFailure
{
    NotFound,
    InvalidInput,
    Timeout,
    Unavailable
}
