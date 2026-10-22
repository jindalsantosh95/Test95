namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}
